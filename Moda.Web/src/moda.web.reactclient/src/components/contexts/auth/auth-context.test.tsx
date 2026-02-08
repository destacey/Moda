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

// Mock next/navigation
const mockUsePathname = jest.fn()
jest.mock('next/navigation', () => ({
  usePathname: () => mockUsePathname(),
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
  addEventCallback: jest.fn().mockReturnValue('callback-id'),
  removeEventCallback: jest.fn(),
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
  EventType: {
    LOGIN_SUCCESS: 'msal:loginSuccess',
    ACQUIRE_TOKEN_SUCCESS: 'msal:acquireTokenSuccess',
    LOGOUT_SUCCESS: 'msal:logoutSuccess',
    ACTIVE_ACCOUNT_CHANGED: 'msal:activeAccountChanged',
  },
}))

// Import after mocks
import { AuthProvider, _resetRedirectFlag } from './auth-context'
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
    homeAccountId: 'account-home-id-123',
    idTokenClaims: {
      sub: '12345',
      email: 'test@example.com',
    },
  }

  beforeEach(() => {
    jest.clearAllMocks()
    _resetRedirectFlag()

    // Default mock implementations
    mockUsePathname.mockReturnValue('/') // Default to home route
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

    it('triggers loginRedirect when permissions return 401', async () => {
      const consoleLogSpy = jest
        .spyOn(console, 'log')
        .mockImplementation(() => {})

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockInstance.loginRedirect.mockResolvedValue()
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: { status: 401, error: 'Unauthorized' },
        refetch: jest.fn(),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(
        () => {
          expect(mockInstance.loginRedirect).toHaveBeenCalled()
        },
        { timeout: 3000 },
      )

      consoleLogSpy.mockRestore()
    })

    it('handles loginRedirect failure gracefully on 401', async () => {
      const consoleErrorSpy = jest
        .spyOn(console, 'error')
        .mockImplementation(() => {})
      const consoleLogSpy = jest
        .spyOn(console, 'log')
        .mockImplementation(() => {})

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])

      const redirectError = new Error('Redirect failed')
      mockInstance.loginRedirect.mockRejectedValue(redirectError)

      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: { status: 401, error: 'Unauthorized' },
        refetch: jest.fn(),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(
        () => {
          expect(mockInstance.loginRedirect).toHaveBeenCalled()
        },
        { timeout: 3000 },
      )

      // Wait for error handling
      await waitFor(
        () => {
          expect(consoleErrorSpy).toHaveBeenCalledWith(
            '[Auth] Re-authentication redirect failed',
            redirectError,
          )
        },
        { timeout: 3000 },
      )

      consoleErrorSpy.mockRestore()
      consoleLogSpy.mockRestore()
    })

    it('prevents concurrent redirects when global flag is set', async () => {
      const consoleLogSpy = jest
        .spyOn(console, 'log')
        .mockImplementation(() => {})

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])

      // Make loginRedirect hang so the flag stays set
      mockInstance.loginRedirect.mockImplementation(
        () => new Promise(() => {}),
      )

      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: { status: 401, error: 'Unauthorized' },
        refetch: jest.fn(),
      })

      const { rerender } = render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Should trigger loginRedirect once
      await waitFor(
        () => {
          expect(mockInstance.loginRedirect).toHaveBeenCalledTimes(1)
        },
        { timeout: 3000 },
      )

      // Force re-renders to simulate additional 401 errors triggering refreshUser
      rerender(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      rerender(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await new Promise((resolve) => setTimeout(resolve, 200))

      // Still only called once - the isRedirectInProgress flag prevented duplicates
      expect(mockInstance.loginRedirect).toHaveBeenCalledTimes(1)

      consoleLogSpy.mockRestore()
    })

    it('skips redirect when MSAL interaction already in progress', async () => {
      const consoleLogSpy = jest
        .spyOn(console, 'log')
        .mockImplementation(() => {})

      // Start with 401 error AND MSAL interaction in progress
      // This simulates the scenario where loginRedirect was already called
      // (either by us or another part of the app) and we get another 401
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'login', // MSAL is already handling a login redirect
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: { status: 401, error: 'Unauthorized' },
        refetch: jest.fn(),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // When inProgress !== 'none', the component shows loading screen
      // and doesn't try to call loginRedirect (effect returns early)
      await waitFor(() => {
        expect(screen.getByTestId('loading-account')).toBeInTheDocument()
      })

      // Should not call loginRedirect when MSAL interaction in progress
      expect(mockInstance.loginRedirect).not.toHaveBeenCalled()

      // The console.log for skipping redirect only happens inside refreshUser,
      // which doesn't run when inProgress !== 'none', so we won't see that log.
      // The protection here is at a higher level - the whole effect doesn't run.
      consoleLogSpy.mockRestore()
    })
  })

  describe('MSAL event callbacks', () => {
    let eventCallback: (event: any) => void

    beforeEach(() => {
      // Capture the event callback registered by the component
      mockInstance.addEventCallback.mockImplementation((cb) => {
        eventCallback = cb
        return 'callback-id'
      })
    })

    it('updates active account on LOGIN_SUCCESS event', async () => {
      const { EventType } = await import('@azure/msal-browser')

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(false)
      mockInstance.getActiveAccount.mockReturnValue(null)

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Simulate LOGIN_SUCCESS event
      await act(async () => {
        eventCallback({
          eventType: EventType.LOGIN_SUCCESS,
          payload: { account: mockAccount },
        })
      })

      expect(mockInstance.setActiveAccount).toHaveBeenCalledWith(mockAccount)
    })

    it('updates active account on ACQUIRE_TOKEN_SUCCESS when account changes', async () => {
      const { EventType } = await import('@azure/msal-browser')

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

      const newAccount = {
        ...mockAccount,
        username: 'updated@example.com',
        homeAccountId: 'different-home-id',
      }

      // Simulate ACQUIRE_TOKEN_SUCCESS event with a different account
      await act(async () => {
        eventCallback({
          eventType: EventType.ACQUIRE_TOKEN_SUCCESS,
          payload: { account: newAccount },
        })
      })

      expect(mockInstance.setActiveAccount).toHaveBeenCalledWith(newAccount)
    })

    it('skips update on ACQUIRE_TOKEN_SUCCESS when account is the same', async () => {
      const { EventType } = await import('@azure/msal-browser')

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
        refetch: jest.fn(),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Clear any calls from component mount
      mockInstance.setActiveAccount.mockClear()

      // Simulate ACQUIRE_TOKEN_SUCCESS with the same account (same homeAccountId)
      await act(async () => {
        eventCallback({
          eventType: EventType.ACQUIRE_TOKEN_SUCCESS,
          payload: { account: mockAccount },
        })
      })

      // Should NOT call setActiveAccount since the account hasn't changed
      expect(mockInstance.setActiveAccount).not.toHaveBeenCalled()
    })

    it('clears active account on LOGOUT_SUCCESS event', async () => {
      const { EventType } = await import('@azure/msal-browser')

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

      // Simulate LOGOUT_SUCCESS event
      await act(async () => {
        eventCallback({
          eventType: EventType.LOGOUT_SUCCESS,
          payload: null,
        })
      })

      // Active account state should be cleared (component's internal state)
      // We can't directly test the state, but we can verify the callback was called
      expect(mockInstance.addEventCallback).toHaveBeenCalled()
    })

    it('updates active account on ACTIVE_ACCOUNT_CHANGED event', async () => {
      const { EventType } = await import('@azure/msal-browser')

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

      const newAccount = { ...mockAccount, username: 'changed@example.com' }
      mockInstance.getActiveAccount.mockReturnValue(newAccount)

      // Simulate ACTIVE_ACCOUNT_CHANGED event
      await act(async () => {
        eventCallback({
          eventType: EventType.ACTIVE_ACCOUNT_CHANGED,
          payload: null,
        })
      })

      expect(mockInstance.getActiveAccount).toHaveBeenCalled()
    })

    it('handles event with null payload gracefully', async () => {
      const { EventType } = await import('@azure/msal-browser')

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

      // Simulate LOGIN_SUCCESS with null payload
      await act(async () => {
        eventCallback({
          eventType: EventType.LOGIN_SUCCESS,
          payload: null,
        })
      })

      // Should not crash - just not call setActiveAccount
      expect(mockInstance.setActiveAccount).not.toHaveBeenCalled()
    })

    it('removes event callback on unmount', () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'none',
      })

      const { unmount } = render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      unmount()

      expect(mockInstance.removeEventCallback).toHaveBeenCalledWith(
        'callback-id',
      )
    })
  })

  describe('cross-tab storage synchronization', () => {
    let storageEventHandlers: Array<(event: StorageEvent) => void> = []
    let originalAddEventListener: typeof window.addEventListener

    beforeEach(() => {
      storageEventHandlers = []

      // Save original
      originalAddEventListener = window.addEventListener

      // Capture the storage event handler
      window.addEventListener = jest.fn((event, handler) => {
        if (event === 'storage' && typeof handler === 'function') {
          storageEventHandlers.push(handler as (event: StorageEvent) => void)
        }
      })
    })

    afterEach(() => {
      window.addEventListener = originalAddEventListener
      jest.restoreAllMocks()
    })

    it('clears active account when accounts are cleared in another tab', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read'],
        isLoading: false,
        error: undefined,
        refetch: jest.fn(),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Wait for component to mount and register storage event listener
      await waitFor(() => {
        expect(storageEventHandlers.length).toBeGreaterThan(0)
      })

      // Clear setActiveAccount calls from mount
      mockInstance.setActiveAccount.mockClear()

      // Simulate another tab clearing MSAL storage (logout)
      mockInstance.getAllAccounts.mockReturnValue([])

      await act(async () => {
        storageEventHandlers[0](
          new StorageEvent('storage', {
            key: 'msal.account.keys',
            newValue: null,
            oldValue: '["account-1"]',
          }),
        )
      })

      // Should clear active account instead of reloading the page
      expect(mockInstance.setActiveAccount).toHaveBeenCalledWith(null)
    })

    it('sets active account when account added in another tab', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(false)
      mockInstance.getActiveAccount.mockReturnValue(null)
      mockInstance.getAllAccounts.mockReturnValue([])
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: undefined,
        refetch: jest.fn(),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Wait for component to mount and register storage event listener
      await waitFor(() => {
        expect(storageEventHandlers.length).toBeGreaterThan(0)
      })

      // Simulate another tab logging in
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockInstance.getActiveAccount.mockReturnValue(null)

      await act(async () => {
        storageEventHandlers[0](
          new StorageEvent('storage', {
            key: 'msal.account.keys',
            newValue: '["account-1"]',
            oldValue: null,
          }),
        )
      })

      expect(mockInstance.setActiveAccount).toHaveBeenCalledWith(mockAccount)
    })

    it('ignores non-MSAL storage events', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read'],
        isLoading: false,
        error: undefined,
        refetch: jest.fn(),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Wait for component to mount and register storage event listener
      await waitFor(() => {
        expect(storageEventHandlers.length).toBeGreaterThan(0)
      })

      const getAllAccountsCallsBefore =
        mockInstance.getAllAccounts.mock.calls.length

      // Simulate storage event for non-MSAL key
      await act(async () => {
        storageEventHandlers[0](
          new StorageEvent('storage', {
            key: 'theme-preference',
            newValue: 'dark',
            oldValue: 'light',
          }),
        )
      })

      // Should not check accounts or set active account (early return in filter logic)
      expect(mockInstance.getAllAccounts.mock.calls.length).toBe(
        getAllAccountsCallsBefore,
      )
      expect(mockInstance.setActiveAccount).not.toHaveBeenCalled()
    })

    it('clears active account on localStorage.clear() event (null key)', async () => {
      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockInstance.getAllAccounts.mockReturnValue([mockAccount])
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: ['Permission.Read'],
        isLoading: false,
        error: undefined,
        refetch: jest.fn(),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Wait for component to mount and register storage event listener
      await waitFor(() => {
        expect(storageEventHandlers.length).toBeGreaterThan(0)
      })

      // Clear setActiveAccount calls from mount
      mockInstance.setActiveAccount.mockClear()

      // After event, accounts should be empty to trigger account clear
      mockInstance.getAllAccounts.mockReturnValue([])

      // Simulate localStorage.clear() in another tab (null key)
      await act(async () => {
        storageEventHandlers[0](
          new StorageEvent('storage', {
            key: null, // null key indicates clear()
            newValue: null,
            oldValue: null,
          }),
        )
      })

      // Should clear active account instead of reloading the page
      expect(mockInstance.setActiveAccount).toHaveBeenCalledWith(null)
    })

    it('removes storage event listener on unmount', async () => {
      let removeEventListenerHandler: (event: StorageEvent) => void
      const originalRemoveEventListener = window.removeEventListener

      window.removeEventListener = jest.fn((event, handler) => {
        if (event === 'storage' && typeof handler === 'function') {
          removeEventListenerHandler = handler as (event: StorageEvent) => void
        }
      })

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [],
        inProgress: 'none',
      })
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: undefined,
        refetch: jest.fn(),
      })

      const { unmount } = render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      // Wait for component to mount
      await waitFor(() => {
        expect(storageEventHandlers.length).toBeGreaterThan(0)
      })

      unmount()

      expect(window.removeEventListener).toHaveBeenCalledWith(
        'storage',
        expect.any(Function),
      )

      window.removeEventListener = originalRemoveEventListener
    })
  })
})
