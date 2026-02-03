import { render, screen, waitFor } from '@testing-library/react'
import '@testing-library/jest-dom'

// MSAL mocks
const mockInstance = {
  setActiveAccount: jest.fn(),
  logoutRedirect: jest.fn(),
}

const mockUseMsal = jest.fn()

jest.mock('@azure/msal-react', () => ({
  useMsal: () => mockUseMsal(),
}))

// Import after mocks
import LogoutPage from './page'

describe('LogoutPage', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockUseMsal.mockReturnValue({
      instance: mockInstance,
    })
    mockInstance.logoutRedirect.mockResolvedValue(undefined)
  })

  it('renders the logout page with loading state', () => {
    render(<LogoutPage />)

    expect(screen.getByText('Signing out...')).toBeInTheDocument()
    expect(
      screen.getByText('Please wait while we sign you out of your account.'),
    ).toBeInTheDocument()
  })

  it('renders the Moda logo', () => {
    render(<LogoutPage />)

    expect(screen.getByAltText('Moda')).toBeInTheDocument()
    expect(screen.getByText('moda')).toBeInTheDocument()
  })

  it('triggers logout on mount', async () => {
    render(<LogoutPage />)

    await waitFor(() => {
      expect(mockInstance.setActiveAccount).toHaveBeenCalledWith(null)
      expect(mockInstance.logoutRedirect).toHaveBeenCalledWith({
        postLogoutRedirectUri: expect.stringContaining('/login'),
      })
    })
  })

  it('only triggers logout once even with React strict mode re-renders', async () => {
    const { rerender } = render(<LogoutPage />)

    await waitFor(() => {
      expect(mockInstance.logoutRedirect).toHaveBeenCalledTimes(1)
    })

    // Simulate a re-render (like React strict mode would do)
    rerender(<LogoutPage />)

    // Should still only have been called once
    expect(mockInstance.logoutRedirect).toHaveBeenCalledTimes(1)
  })

  it('clears active account before calling logoutRedirect', async () => {
    const callOrder: string[] = []
    mockInstance.setActiveAccount.mockImplementation(() => {
      callOrder.push('setActiveAccount')
    })
    mockInstance.logoutRedirect.mockImplementation(() => {
      callOrder.push('logoutRedirect')
      return Promise.resolve()
    })

    render(<LogoutPage />)

    await waitFor(() => {
      expect(callOrder).toEqual(['setActiveAccount', 'logoutRedirect'])
    })
  })
})
