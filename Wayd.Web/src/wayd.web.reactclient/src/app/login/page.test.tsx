import { render, screen, waitFor } from '@testing-library/react'
import '@testing-library/jest-dom'

// --- Mocks ---

const mockUseIsAuthenticated = jest.fn()
const mockUseMsal = jest.fn()

jest.mock('@azure/msal-react', () => ({
  useIsAuthenticated: () => mockUseIsAuthenticated(),
  useMsal: () => mockUseMsal(),
}))

jest.mock('next/navigation', () => ({
  useRouter: () => ({ replace: jest.fn() }),
}))

jest.mock('@/src/services/clients', () => ({
  isAuthActive: () => false,
  getAuthClient: jest.fn(),
  setRememberMe: jest.fn(),
  storeAuth: jest.fn(),
}))

// Controlled return for the capabilities hook — each test sets the response it
// wants before rendering. The hook's real source (RTK Query) is stubbed here
// because the login page behavior is the contract under test, not network I/O.
const mockUseGetAuthProvidersQuery = jest.fn()

jest.mock('@/src/store/features/common/auth-providers-api', () => ({
  useGetAuthProvidersQuery: () => mockUseGetAuthProvidersQuery(),
}))

// Imports after mocks
import LoginPage from './page'

describe('LoginPage provider gating', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockUseIsAuthenticated.mockReturnValue(false)
    mockUseMsal.mockReturnValue({
      instance: { loginRedirect: jest.fn() },
      inProgress: 'none',
    })
  })

  it('hides the Microsoft tab and login button when entra is disabled', async () => {
    mockUseGetAuthProvidersQuery.mockReturnValue({
      data: { local: true, entra: false },
      isLoading: false,
    })

    render(<LoginPage />)

    await waitFor(() => {
      // Only the local form should be visible. The Microsoft button and tab
      // must not render — visible-but-broken is worse than absent.
      expect(screen.queryByText('Sign in with Microsoft')).not.toBeInTheDocument()
      expect(screen.queryByRole('button', { name: 'Microsoft' })).not.toBeInTheDocument()
    })

    expect(screen.getByPlaceholderText('Email')).toBeInTheDocument()
    expect(screen.getByPlaceholderText('Password')).toBeInTheDocument()
  })

  it('shows both tabs when both providers are enabled', async () => {
    mockUseGetAuthProvidersQuery.mockReturnValue({
      data: { local: true, entra: true },
      isLoading: false,
    })

    render(<LoginPage />)

    await waitFor(() => {
      expect(screen.getByRole('button', { name: 'Microsoft' })).toBeInTheDocument()
    })
    expect(
      screen.getByRole('button', { name: 'Email & Password' }),
    ).toBeInTheDocument()
  })

  it('defaults to the Microsoft tab when entra is enabled', async () => {
    mockUseGetAuthProvidersQuery.mockReturnValue({
      data: { local: true, entra: true },
      isLoading: false,
    })

    render(<LoginPage />)

    await waitFor(() => {
      expect(screen.getByText('Sign in with Microsoft')).toBeInTheDocument()
    })
  })

  it('stays on the local form while the capabilities query is loading', () => {
    // Prevents a Microsoft button from flashing in for local-only deployments
    // before the capabilities response arrives.
    mockUseGetAuthProvidersQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
    })

    render(<LoginPage />)

    expect(screen.queryByText('Sign in with Microsoft')).not.toBeInTheDocument()
    expect(screen.getByPlaceholderText('Email')).toBeInTheDocument()
  })
})
