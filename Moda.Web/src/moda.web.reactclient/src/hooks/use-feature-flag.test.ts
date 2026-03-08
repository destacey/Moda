import { renderHook } from '@testing-library/react'
import { useFeatureFlag } from './use-feature-flag'
import { useGetClientFeatureFlagsQuery } from '../store/features/feature-flags-api'

jest.mock('../store/features/feature-flags-api', () => ({
  useGetClientFeatureFlagsQuery: jest.fn(),
}))

const mockUseGetClientFeatureFlagsQuery =
  useGetClientFeatureFlagsQuery as jest.Mock

describe('useFeatureFlag', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('returns true when feature flag is found and enabled', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [
        { name: 'my-feature', isEnabled: true },
        { name: 'other-feature', isEnabled: true },
      ],
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current).toBe(true)
  })

  it('returns false when feature flag is found but disabled', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [{ name: 'my-feature', isEnabled: false }],
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current).toBe(false)
  })

  it('returns false when feature flag is not in the list', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [{ name: 'other-feature', isEnabled: true }],
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current).toBe(false)
  })

  it('returns false when data is undefined (loading)', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: undefined,
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current).toBe(false)
  })

  it('returns false when feature flags list is empty', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [],
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current).toBe(false)
  })

  it('is case-sensitive for feature flag names', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [{ name: 'my-feature', isEnabled: true }],
    })

    const { result } = renderHook(() => useFeatureFlag('My-Feature'))
    expect(result.current).toBe(false)
  })

  it('passes polling interval to the query hook', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({ data: [] })

    renderHook(() => useFeatureFlag('my-feature'))

    expect(mockUseGetClientFeatureFlagsQuery).toHaveBeenCalledWith(undefined, {
      pollingInterval: 60_000,
    })
  })
})
