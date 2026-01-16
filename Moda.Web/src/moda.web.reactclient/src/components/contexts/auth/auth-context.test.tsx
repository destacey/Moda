import { render, screen, waitFor, act } from '@testing-library/react'
import '@testing-library/jest-dom'
import { useContext } from 'react'

// Mock LoadingAccount component - must be before imports that use it
jest.mock('../../common', () => ({
  LoadingAccount: ({ message }: { message: string }) => (
    <div data-testid="loading-account">{message}</div>
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
  acquireTokenSilent: jest.fn(),
  acquireTokenPopup: jest.fn(),
  loginRedirect: jest.fn(),
  logoutRedirect: jest.fn(),
}

const mockUseMsal = jest.fn()
const mockUseIsAuthenticated = jest.fn()
const mockUseMsalAuthentication = jest.fn()

jest.mock('@azure/msal-react', () => ({
  useMsal: () => mockUseMsal(),
  useIsAuthenticated: () => mockUseIsAuthenticated(),
  useMsalAuthentication: (...args: unknown[]) =>
    mockUseMsalAuthentication(...args),
}))

jest.mock('@azure/msal-browser', () => ({
  InteractionRequiredAuthError: class InteractionRequiredAuthError extends Error {
    constructor(message: string) {
      super(message)
      this.name = 'InteractionRequiredAuthError'
    }
  },
  InteractionType: {
    Redirect: 'redirect',
  },
}))

// Import after mocks
import { AuthProvider, AuthContext } from './auth-context'
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
    mockUseMsalAuthentication.mockReturnValue({ error: null })
    mockUseGetUserPermissionsQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
      error: undefined,
    })
    mockInstance.getActiveAccount.mockReturnValue(null)
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
    it('shows loading state initially when MSAL is in progress', () => {
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

      expect(screen.getByTestId('loading-account')).toBeInTheDocument()
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

      let authContext: AuthContextType | null = null
      const ContextCapture = () => {
        authContext = useContext(AuthContext)
        return null
      }

      render(
        <AuthProvider>
          <ContextCapture />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(authContext?.user?.isAuthenticated).toBe(true)
      })

      expect(authContext?.hasClaim('email', 'test@example.com')).toBe(true)
      expect(authContext?.hasClaim('email', 'other@example.com')).toBe(false)
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

      let authContext: AuthContextType | null = null
      const ContextCapture = () => {
        authContext = useContext(AuthContext)
        return null
      }

      render(
        <AuthProvider>
          <ContextCapture />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(authContext?.user?.isAuthenticated).toBe(true)
      })

      expect(authContext?.hasPermissionClaim('Permission.Read')).toBe(true)
      expect(authContext?.hasPermissionClaim('Permission.Delete')).toBe(false)
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

    it('calls logoutRedirect and clears user when logout is invoked', async () => {
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

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(screen.getByTestId('user-authenticated').textContent).toBe(
          'true',
        )
      })

      await act(async () => {
        screen.getByTestId('logout-btn').click()
      })

      expect(mockInstance.setActiveAccount).toHaveBeenCalledWith(null)
      expect(mockInstance.logoutRedirect).toHaveBeenCalled()
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
      mockInstance.acquireTokenSilent.mockResolvedValue({
        accessToken: 'test-token',
      })
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: [],
        isLoading: false,
        error: undefined,
      })

      let authContext: AuthContextType | null = null
      const ContextCapture = () => {
        authContext = useContext(AuthContext)
        return null
      }

      render(
        <AuthProvider>
          <ContextCapture />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(authContext?.user?.isAuthenticated).toBe(true)
      })

      const token = await authContext?.acquireToken()
      expect(token).toBe('test-token')
      expect(mockInstance.acquireTokenSilent).toHaveBeenCalled()
    })

    it('falls back to popup when silent acquisition fails with interaction required', async () => {
      // Use the mocked InteractionRequiredAuthError from our jest.mock
      const { InteractionRequiredAuthError } = await import(
        '@azure/msal-browser'
      )

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)

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

      let authContext: AuthContextType | null = null
      const ContextCapture = () => {
        authContext = useContext(AuthContext)
        return null
      }

      render(
        <AuthProvider>
          <ContextCapture />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(authContext?.user?.isAuthenticated).toBe(true)
      })

      const token = await authContext?.acquireToken()
      expect(token).toBe('popup-token')
      expect(mockInstance.acquireTokenPopup).toHaveBeenCalled()
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
    it('logs auth errors from useMsalAuthentication', async () => {
      const consoleError = jest
        .spyOn(console, 'error')
        .mockImplementation(() => {})

      const authError = new Error('Auth failed')
      mockUseMsalAuthentication.mockReturnValue({ error: authError })
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
        expect(consoleError).toHaveBeenCalledWith(
          '[Auth] Authentication error:',
          authError,
        )
      })

      consoleError.mockRestore()
    })

    it('handles permissions error gracefully', async () => {
      const consoleError = jest
        .spyOn(console, 'error')
        .mockImplementation(() => {})

      mockUseMsal.mockReturnValue({
        instance: mockInstance,
        accounts: [mockAccount],
        inProgress: 'none',
      })
      mockUseIsAuthenticated.mockReturnValue(true)
      mockInstance.getActiveAccount.mockReturnValue(mockAccount)
      mockUseGetUserPermissionsQuery.mockReturnValue({
        data: undefined,
        isLoading: false,
        error: new Error('Failed to fetch permissions'),
      })

      render(
        <AuthProvider>
          <TestConsumer />
        </AuthProvider>,
      )

      await waitFor(() => {
        expect(screen.getByTestId('user-authenticated').textContent).toBe(
          'true',
        )
      })

      // User should still be authenticated even if permissions failed
      expect(screen.getByTestId('user-name').textContent).toBe('Test User')

      consoleError.mockRestore()
    })
  })
})
