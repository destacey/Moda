import React from 'react'
import { render, screen, waitFor } from '@testing-library/react'
import { message } from 'antd'
import authorizeForm from './authorize-form'
import { MessageProvider } from '../contexts/messaging'

const DummyForm: React.FC = () => <div>Dummy Form</div>
DummyForm.displayName = 'DummyForm'

const mockHasClaim = jest.fn()
jest.mock('../contexts/auth', () => ({
  __esModule: true,
  default: () => ({
    hasClaim: mockHasClaim,
  }),
}))

jest.mock('antd', () => ({
  ...jest.requireActual('antd'),
  message: {
    error: jest.fn(),
    useMessage: jest.fn(() => [
      {
        error: jest.fn(),
      },
      <div key="contextHolder" />,
    ]),
  },
}))

describe('authorizeForm HOC', () => {
  let onNotAuthorized: jest.Mock

  beforeEach(() => {
    onNotAuthorized = jest.fn()
    mockHasClaim.mockReset()
    jest.clearAllMocks()
  })

  const renderWithMessageProvider = (ui: React.ReactElement) => {
    return render(<MessageProvider>{ui}</MessageProvider>)
  }

  test('calls onNotAuthorized and shows error when unauthorized', async () => {
    mockHasClaim.mockReturnValue(false)

    const AuthorizedDummyForm = authorizeForm(
      DummyForm,
      onNotAuthorized,
      'Permission',
      'test-value',
    )

    renderWithMessageProvider(<AuthorizedDummyForm />)

    expect(screen.queryByText('Dummy Form')).toBeNull()

    await waitFor(() => {
      expect(onNotAuthorized).toHaveBeenCalledTimes(1)

      const [mockMessageApi] = (message.useMessage as jest.Mock).mock.results[0]
        .value
      expect(mockMessageApi.error).toHaveBeenCalledWith(
        'You do not have the correct permissions to access this form.',
      )
    })
  })
})
