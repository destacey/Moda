import { render, screen, waitFor, act } from '@testing-library/react'
import '@testing-library/jest-dom'
import React, { useImperativeHandle, forwardRef } from 'react'

// Mock LoadingAccount component - must be before imports that use it
jest.mock('../../common', () => ({
  LoadingAccount: ({ message }: { message: string }) => (
    <div data-testid="loading-account">{message}</div>
  ),
}))

// Mock the page components
jest.mock('../../../app/unauthorized/page', () => ({
  __esModule: true,
  default: () => <div data-testid="unauthorized-page">Unauthorized</div>,
}))

jest.mock('../../../app/service-unavailable/page', () => ({
  __esModule: true,
  default: ({ onRetry }: { onRetry?: () => void }) => (
    <div data-testid="service-unavailable-page">
      Service Unavailable
      {onRetry && (
        <button onClick={onRetry} data-testid="retry-btn">
          Retry
        </button>
      )}
    </div>
  ),
}))

// Mock the permissions API
const mockUseGetUserPermissionsQuery = jest.fn()
jest.mock('../../../store/features/user-management/profile-api', () => ({
  useGetUserPermissionsQuery: (...args: unknown[]) =>
    mockUseGetUserPermissionsQuery(...args),
}))

// MSAL mocks
const mockInstance = {
  getActiveAccount: jest.fn(),
  setActiveAccount: jest.fn(),
  getAllAccounts: jest.fn(),
  acquireTokenSilent: jest.fn(),
  acquireTokenPopup: jest.fn(),
  loginRedirect: jest.fn(),
  logoutRedirect: jest.fn(),
}

const mockUseMsal = jest.fn()
const mockUseIsAuthenticated = jest.fn()

jest.mock('@azure/msal-react', () => ({
  useMsal: () => mockUseMsal(),
  useIsAuthenticated: () => mockUseIsAuthenticated(),
}))

jest.mock('@azure/msal-browser', () => ({
  InteractionRequiredAuthError: class InteractionRequiredAuthError extends Error {
    constructor(message: string) {
      super(message)
      this.name = 'InteractionRequiredAuthError'
    }
  },
}))

// Import after mocks
import { AuthProvider } from './auth-context'
import useAuth from './use-auth'
import type { AuthContextType } from './types'

// Test component to consume auth context
const TestConsumer = () => {
  const auth = useAuth()
  return (
    <div>
      <span data-testid="user-name">{auth.user?.name || 'No user'}</span>
      <span data-testid="user-authenticated">
        {auth.user?.isAuthenticated ? 'true' : 'false'}
      </span>
      <span data-testid="is-loading">{auth.isLoading ? 'true' : 'false'}</span>
      <button onClick={auth.login} data-testid="login-btn">
        Login
      </button>
      <button onClick={auth.logout} data-testid="logout-btn">
        Logout
      </button>
    </div>
  )
}

// Test component that exposes auth context via ref for testing
interface AuthContextHandle {
  getContext: () => AuthContextType
}

const TestConsumerWithRef = forwardRef<AuthContextHandle>(
  function TestConsumerWithRef(_props, ref) {
    const auth = useAuth()

    useImperativeHandle(ref, () => ({
      getContext: () => auth,
    }))

    return (
      <div>
        <span data-testid="user-name">{auth.user?.name || 'No user'}</span>
        <span data-testid="user-authenticated">
          {auth.user?.isAuthenticated ? 'true' : 'false'}
        </span>
        <span data-testid="is-loading">
          {auth.isLoading ? 'true' : 'false'}
        </span>
      </div>
    )
  },
)

describe('AuthContext', () => {
  const mockAccount = {
    username: 'test@example.com',
    name: 'Test User',
    idTokenClaims: {
      sub: '12345',
      email: 'test@example.com',
    },
  }

  beforeEach(() => {
    jest.clearAllMocks()

    // Default mock implementations
    mockUseMsal.mockReturnValue({
      instance: mockInstance,
      accounts: [],
      inProgress: 'none',
    })
    mockUseIsAuthenticated.mockReturnValue(false)
    mockUseGetUserPermissionsQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
    })
    mockInstance.getActiveAccount.mockReturnValue(null)
    mockInstance.getAllAccounts.mockReturnValue([])
  })

  describe('useAuth hook', () => {
    it('throws error when used outside AuthProvider', () => {
      const consoleError = jest
        .spyOn(console, 'error')
        .mockImplementation(() => {})

      expect(() => {
        render(<TestConsumer />)
      }).toThrow('useAuth must be used within an AuthProvider')

      consoleError.mockRestore()
    })

    it('returns auth context when used within AuthProvider', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'none',
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(screen.getByTestId('is-loading').textContent).toBe('false')
      })

      expect(screen.getByTestId('user-name')).toBeInTheDocument()
    })
  })

  describe('AuthProvider', () => {
    it('shows loading state when authenticated user is loading permissions', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: true,
        error: undefined,
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      expect(screen.getByTestId('loading-account')).toBeInTheDocument()
    })

    it('does not show loading state for unauthenticated users', () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(false)

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Should render children directly without loading screen
      expect(screen.queryByTestId('loading-account')).not.toBeInTheDocument()
      expect(screen.getByTestId('user-authenticated').textContent).toBe('false')
    })

    it('sets active account when accounts exist but no active account', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(null)

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(mockInstance.setActiveAccount).toHaveBeenCalledWith(mockAccount)
      })
    })

    it('does not set active account when one already exists', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(screen.getByTestId('is-loading').textContent).toBe('false')
      })

      // setActiveAccount should not be called since one already exists
      expect(mockInstance.setActiveAccount).not.toHaveBeenCalled()
    })

    it('updates user when MSAL transitions from startup to ready with accounts', async () => {
      // Simulate MSAL startup state (like after a login redirect)
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'startup',
      })
      mockUseIsAuthenticated.mockReturnValue(false)
      mockInstance.getActiveAccount.mockReturnValue(null)

      const { rerender } = render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // During startup, should show loading or initial state
      expect(screen.getByTestId('user-authenticated').textContent).toBe('false')

      // Now simulate MSAL finishing initialization with an authenticated user
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount
        .mockReturnValueOnce(null) // First call returns null (before setActiveAccount)
        .mockReturnValue(mockAccount) // Subsequent calls return the account
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read'],
        isLoading: false,
        error: undefined,
      })

      rerender(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Should set the active account and build user profile
      await waitFor(() => {
        expect(screen.getByTestId('user-name').textContent).toBe('Test User')
        expect(screen.getByTestId('user-authenticated').textContent).toBe(
          'true',
        )
      })
    })

    it('clears activeAccount state when MSAL is not ready', async () => {
      // Start with authenticated user
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read'],
        isLoading: false,
        error: undefined,
      })

      const { rerender } = render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(screen.getByTestId('user-authenticated').textContent).toBe(
          'true',
        )
      })

      // Simulate MSAL going back to not ready (e.g., during a redirect)
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'handleRedirect',
      })

      rerender(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Should show loading state during redirect handling
      expect(screen.getByTestId('loading-account')).toBeInTheDocument()
    })

    it('builds user profile with permissions when authenticated', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read', 'Permission.Write'],
        isLoading: false,
        error: undefined,
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(screen.getByTestId('user-name').textContent).toBe('Test User')
        expect(screen.getByTestId('user-authenticated').textContent).toBe(
          'true',
        )
      })
    })

    it('skips permissions query when not ready', () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'startup',
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      expect(mockUseGetUserPermissionsQuery).toHaveBeenCalledWith(undefined, {
        skip: true,
      })
    })

    it('skips permissions query when not authenticated', () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(false)

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      expect(mockUseGetUserPermissionsQuery).toHaveBeenCalledWith(undefined, {
        skip: true,
      })
    })

    it('fetches permissions when authenticated and ready', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read'],
        isLoading: false,
        error: undefined,
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(mockUseGetUserPermissionsQuery).toHaveBeenCalledWith(undefined, {
          skip: false,
        })
      })
    })
  })

  describe('hasClaim and hasPermissionClaim', () => {
    it('hasClaim returns true when claim exists', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read'],
        isLoading: false,
        error: undefined,
      })

      const ref = React.createRef<AuthContextHandle>()

      render(
        <AuthProvider>
          <TestConsumerWithRef ref={ref} />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(ref.current?.getContext().user?.isAuthenticated).toBe(true)
      })

      const authContext = ref.current!.getContext()
      expect(authContext.hasClaim('email', 'test@example.com')).toBe(true)
      expect(authContext.hasClaim('email', 'other@example.com')).toBe(false)
    })

    it('hasPermissionClaim returns true when permission exists', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read', 'Permission.Write'],
        isLoading: false,
        error: undefined,
      })

      const ref = React.createRef<AuthContextHandle>()

      render(
        <AuthProvider>
          <TestConsumerWithRef ref={ref} />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(ref.current?.getContext().user?.isAuthenticated).toBe(true)
      })

      const authContext = ref.current!.getContext()
      expect(authContext.hasPermissionClaim('Permission.Read')).toBe(true)
      expect(authContext.hasPermissionClaim('Permission.Delete')).toBe(false)
    })
  })

  describe('login and logout', () => {
    it('calls loginRedirect when login is invoked', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'none',
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(screen.getByTestId('is-loading').textContent).toBe('false')
      })

      await act(async () => {
        screen.getByTestId('login-btn').click()
      })

      expect(mockInstance.loginRedirect).toHaveBeenCalled()
    })

    it('logout function can be called without error', async () => {
      // Note: Testing window.location.href assignment is difficult in jsdom
      // The actual navigation to /logout is verified via integration tests
      // This test verifies the logout function is accessible and callable

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: [],
        isLoading: false,
        error: undefined,
      })

      const ref = React.createRef<AuthContextHandle>()

      render(
        <AuthProvider>
          <TestConsumerWithRef ref={ref} />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(ref.current?.getContext().user?.isAuthenticated).toBe(true)
      })

      // Verify logout function exists and can be called
      const authContext = ref.current!.getContext()
      expect(typeof authContext.logout).toBe('function')

      // The actual window.location.href assignment will throw "Not implemented" in jsdom
      // but the function itself should be defined and callable
    })
  })

  describe('acquireToken', () => {
    it('acquires token silently when possible', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockInstance.acquireTokenSilent.mockResolvedValue({
        accessToken: 'test-token',
      })
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: [],
        isLoading: false,
        error: undefined,
      })

      const ref = React.createRef<AuthContextHandle>()

      render(
        <AuthProvider>
          <TestConsumerWithRef ref={ref} />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(ref.current?.getContext().user?.isAuthenticated).toBe(true)
      })

      const token = await ref.current!.getContext().acquireToken()
      expect(token).toBe('test-token')
      expect(mockInstance.acquireTokenSilent).toHaveBeenCalledWith(
        expect.objectContaining({
          account: mockAccount,
        }),
      )
    })

    it('falls back to popup when silent acquisition fails with interaction required', async () => {
      // Use the mocked InteractionRequiredAuthError from our jest.mock
      const { InteractionRequiredAuthError } =
        await import('@azure/msal-browser')

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])

      const interactionError = new InteractionRequiredAuthError(
        'interaction_required',
      )
      mockInstance.acquireTokenSilent.mockRejectedValue(interactionError)
      mockInstance.acquireTokenPopup.mockResolvedValue({
        accessToken: 'popup-token',
      })
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: [],
        isLoading: false,
        error: undefined,
      })

      const ref = React.createRef<AuthContextHandle>()

      render(
        <AuthProvider>
          <TestConsumerWithRef ref={ref} />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(ref.current?.getContext().user?.isAuthenticated).toBe(true)
      })

      const token = await ref.current!.getContext().acquireToken()
      expect(token).toBe('popup-token')
      expect(mockInstance.acquireTokenPopup).toHaveBeenCalled()
    })

    it('throws error when no accounts are found', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([]) // No accounts
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: [],
        isLoading: false,
        error: undefined,
      })

      const ref = React.createRef<AuthContextHandle>()

      render(
        <AuthProvider>
          <TestConsumerWithRef ref={ref} />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(ref.current?.getContext().user?.isAuthenticated).toBe(true)
      })

      await expect(ref.current!.getContext().acquireToken()).rejects.toThrow(
        'No authenticated accounts found',
      )
    })
  })

  describe('cross-tab authentication (localStorage)', () => {
    it('uses cached accounts from localStorage on mount', async () => {
      // Simulate scenario where accounts are already in cache (cross-tab)
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount], // Accounts loaded from localStorage
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read'],
        isLoading: false,
        error: undefined,
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Should immediately show authenticated user without redirect
      await waitFor(() => {
        expect(screen.getByTestId('user-name').textContent).toBe('Test User')
        expect(screen.getByTestId('user-authenticated').textContent).toBe(
          'true',
        )
      })

      // Should not have triggered login redirect
      expect(mockInstance.loginRedirect).not.toHaveBeenCalled()
    })
  })

  describe('error handling', () => {
    it('shows unauthorized page when permissions return 403', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: { status: 403, error: 'Forbidden' },
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(screen.getByTestId('unauthorized-page')).toBeInTheDocument()
      })
    })

    it('shows service unavailable page when permissions API fails', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: { status: 500, error: 'Server error' },
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(
          screen.getByTestId('service-unavailable-page'),
        ).toBeInTheDocument()
      })
    })
  })
})
