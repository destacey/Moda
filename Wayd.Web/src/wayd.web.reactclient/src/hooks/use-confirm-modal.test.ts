import { renderHook, act } from '@testing-library/react'
import useConfirmModal from './use-confirm-modal'

// Mock dependencies
const mockMessageSuccess = jest.fn()
const mockMessageError = jest.fn()
jest.mock('../components/contexts/messaging', () => ({
  useMessage: () => ({
    success: mockMessageSuccess,
    error: mockMessageError,
  }),
}))

const mockHasPermissionClaim = jest.fn()
jest.mock('../components/contexts/auth', () => ({
  __esModule: true,
  default: () => ({
    hasPermissionClaim: mockHasPermissionClaim,
  }),
}))

const createDefaultOptions = (overrides = {}) => ({
  onSubmit: jest.fn().mockResolvedValue(true),
  onComplete: jest.fn(),
  onCancel: jest.fn(),
  ...overrides,
})

describe('useConfirmModal', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockHasPermissionClaim.mockReturnValue(true)
  })

  describe('initial state', () => {
    it('starts with isSaving false', () => {
      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions()),
      )

      expect(result.current.isSaving).toBe(false)
    })

    it('starts with isOpen true when no permission is provided', () => {
      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions()),
      )

      expect(result.current.isOpen).toBe(true)
    })

    it('returns handler functions', () => {
      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions()),
      )

      expect(typeof result.current.handleOk).toBe('function')
      expect(typeof result.current.handleCancel).toBe('function')
    })
  })

  describe('permission gate', () => {
    it('sets isOpen true when permission claim is granted', () => {
      mockHasPermissionClaim.mockReturnValue(true)

      const { result } = renderHook(() =>
        useConfirmModal(
          createDefaultOptions({ permission: 'Permissions.Test.Delete' }),
        ),
      )

      expect(result.current.isOpen).toBe(true)
      expect(mockHasPermissionClaim).toHaveBeenCalledWith(
        'Permissions.Test.Delete',
      )
    })

    it('sets isOpen false when permission claim is denied', () => {
      mockHasPermissionClaim.mockReturnValue(false)

      const { result } = renderHook(() =>
        useConfirmModal(
          createDefaultOptions({ permission: 'Permissions.Test.Delete' }),
        ),
      )

      expect(result.current.isOpen).toBe(false)
    })

    it('calls onCancel when permission is denied', () => {
      mockHasPermissionClaim.mockReturnValue(false)
      const onCancel = jest.fn()

      renderHook(() =>
        useConfirmModal(
          createDefaultOptions({
            permission: 'Permissions.Test.Delete',
            onCancel,
          }),
        ),
      )

      expect(onCancel).toHaveBeenCalled()
    })

    it('shows error toast when permission is denied', () => {
      mockHasPermissionClaim.mockReturnValue(false)

      renderHook(() =>
        useConfirmModal(
          createDefaultOptions({ permission: 'Permissions.Test.Delete' }),
        ),
      )

      expect(mockMessageError).toHaveBeenCalledWith(
        'You do not have permission to perform this action.',
      )
    })

    it('only shows permission error once on re-renders', () => {
      mockHasPermissionClaim.mockReturnValue(false)
      const onCancel = jest.fn()

      const { rerender } = renderHook(() =>
        useConfirmModal(
          createDefaultOptions({
            permission: 'Permissions.Test.Delete',
            onCancel,
          }),
        ),
      )

      rerender()
      rerender()

      expect(mockMessageError).toHaveBeenCalledTimes(1)
      expect(onCancel).toHaveBeenCalledTimes(1)
    })

    it('does not check permission when no permission string is provided', () => {
      const onCancel = jest.fn()

      renderHook(() =>
        useConfirmModal(createDefaultOptions({ onCancel })),
      )

      expect(onCancel).not.toHaveBeenCalled()
      expect(mockMessageError).not.toHaveBeenCalled()
    })
  })

  describe('handleOk', () => {
    it('calls onSubmit', async () => {
      const onSubmit = jest.fn().mockResolvedValue(true)

      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions({ onSubmit })),
      )

      await act(async () => {
        await result.current.handleOk()
      })

      expect(onSubmit).toHaveBeenCalled()
    })

    it('calls onComplete when onSubmit returns true', async () => {
      const onSubmit = jest.fn().mockResolvedValue(true)
      const onComplete = jest.fn()

      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions({ onSubmit, onComplete })),
      )

      await act(async () => {
        await result.current.handleOk()
      })

      expect(onComplete).toHaveBeenCalled()
    })

    it('does not call onComplete when onSubmit returns false', async () => {
      const onSubmit = jest.fn().mockResolvedValue(false)
      const onComplete = jest.fn()

      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions({ onSubmit, onComplete })),
      )

      await act(async () => {
        await result.current.handleOk()
      })

      expect(onComplete).not.toHaveBeenCalled()
    })

    it('sets isSaving to true while submitting', async () => {
      let resolveFn: (value: boolean) => void
      const submitPromise = new Promise<boolean>((resolve) => {
        resolveFn = resolve
      })
      const onSubmit = jest.fn().mockReturnValue(submitPromise)

      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions({ onSubmit })),
      )

      let okPromise: Promise<void>
      act(() => {
        okPromise = result.current.handleOk()
      })

      expect(result.current.isSaving).toBe(true)

      await act(async () => {
        resolveFn!(true)
        await okPromise
      })

      expect(result.current.isSaving).toBe(false)
    })

    it('sets isSaving back to false after submit failure', async () => {
      jest.spyOn(console, 'error').mockImplementation()
      const onSubmit = jest.fn().mockRejectedValue(new Error('API error'))

      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions({ onSubmit })),
      )

      await act(async () => {
        await result.current.handleOk()
      })

      expect(result.current.isSaving).toBe(false)
    })

    it('shows default error message when onSubmit throws', async () => {
      jest.spyOn(console, 'error').mockImplementation()
      const onSubmit = jest.fn().mockRejectedValue(new Error('API error'))

      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions({ onSubmit })),
      )

      await act(async () => {
        await result.current.handleOk()
      })

      expect(mockMessageError).toHaveBeenCalledWith(
        'An unexpected error occurred. Please try again.',
      )
    })

    it('shows custom error message when provided and onSubmit throws', async () => {
      jest.spyOn(console, 'error').mockImplementation()
      const onSubmit = jest.fn().mockRejectedValue(new Error('API error'))

      const { result } = renderHook(() =>
        useConfirmModal(
          createDefaultOptions({
            onSubmit,
            errorMessage: 'Failed to delete the item.',
          }),
        ),
      )

      await act(async () => {
        await result.current.handleOk()
      })

      expect(mockMessageError).toHaveBeenCalledWith(
        'Failed to delete the item.',
      )
    })
  })

  describe('handleCancel', () => {
    it('calls onCancel callback', () => {
      const onCancel = jest.fn()

      const { result } = renderHook(() =>
        useConfirmModal(createDefaultOptions({ onCancel })),
      )

      act(() => {
        result.current.handleCancel()
      })

      expect(onCancel).toHaveBeenCalled()
    })
  })
})
