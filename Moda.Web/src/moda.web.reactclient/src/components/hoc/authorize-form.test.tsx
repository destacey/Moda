// authorizeForm.test.tsx
import React from 'react'
import { render, screen, waitFor } from '@testing-library/react'
import { MessageInstance } from 'antd/es/message/interface'
import authorizeForm from './authorize-form'

// A dummy component to be wrapped by the HOC.
const DummyForm: React.FC = () => <div>Dummy Form</div>
DummyForm.displayName = 'DummyForm'

// We'll mock the useAuth hook to control the behavior of hasClaim.
const mockHasClaim = jest.fn()
jest.mock('../contexts/auth', () => ({
  __esModule: true,
  default: () => ({
    hasClaim: mockHasClaim,
  }),
}))

describe('authorizeForm HOC', () => {
  let messageApi: MessageInstance
  let onNotAuthorized: jest.Mock

  beforeEach(() => {
    // Create a stubbed message API
    messageApi = { error: jest.fn() } as unknown as MessageInstance
    onNotAuthorized = jest.fn()
    mockHasClaim.mockReset()
  })

  test('renders wrapped form when authorized', () => {
    // Simulate that the user is authorized.
    mockHasClaim.mockReturnValue(true)

    const AuthorizedDummyForm = authorizeForm(
      DummyForm,
      onNotAuthorized,
      messageApi,
      'Permission',
      'test-value',
    )

    render(<AuthorizedDummyForm />)

    // Verify that the dummy form is rendered.
    expect(screen.getByText('Dummy Form')).toBeInTheDocument()
    expect(onNotAuthorized).not.toHaveBeenCalled()
    expect(messageApi.error).not.toHaveBeenCalled()
  })

  test('calls onNotAuthorized and shows error when unauthorized', async () => {
    // Simulate that the user is not authorized.
    mockHasClaim.mockReturnValue(false)

    const AuthorizedDummyForm = authorizeForm(
      DummyForm,
      onNotAuthorized,
      messageApi,
      'Permission',
      'test-value',
    )

    render(<AuthorizedDummyForm />)

    // The dummy form should not render.
    expect(screen.queryByText('Dummy Form')).toBeNull()

    // Wait for the effect to run, then verify that the unauthorized callback
    // and error message have been triggered.
    await waitFor(() => {
      expect(onNotAuthorized).toHaveBeenCalledTimes(1)
      expect(messageApi.error).toHaveBeenCalledWith(
        'You do not have the correct permissions to access this form.',
      )
    })
  })

  test('sets the displayName correctly', () => {
    const AuthorizedDummyForm = authorizeForm(
      DummyForm,
      onNotAuthorized,
      messageApi,
      'Permission',
      'test-value',
    )

    expect(AuthorizedDummyForm.displayName).toBe('authorizedForm(DummyForm)')
  })
})
