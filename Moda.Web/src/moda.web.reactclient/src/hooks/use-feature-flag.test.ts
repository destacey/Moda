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

  it('returns isEnabled true when feature flag is found and enabled', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [
        { name: 'my-feature', isEnabled: true },
        { name: 'other-feature', isEnabled: true },
      ],
      isLoading: false,
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current.isEnabled).toBe(true)
    expect(result.current.isLoading).toBe(false)
  })

  it('returns isEnabled false when feature flag is found but disabled', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [{ name: 'my-feature', isEnabled: false }],
      isLoading: false,
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current.isEnabled).toBe(false)
    expect(result.current.isLoading).toBe(false)
  })

  it('returns isEnabled false when feature flag is not in the list', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [{ name: 'other-feature', isEnabled: true }],
      isLoading: false,
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current.isEnabled).toBe(false)
  })

  it('returns isLoading true when data is loading', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: undefined,
      isLoading: true,
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current.isEnabled).toBe(false)
    expect(result.current.isLoading).toBe(true)
  })

  it('returns isEnabled false when data is undefined and not loading', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: undefined,
      isLoading: false,
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current.isEnabled).toBe(false)
    expect(result.current.isLoading).toBe(false)
  })

  it('returns isEnabled false when feature flags list is empty', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [],
      isLoading: false,
    })

    const { result } = renderHook(() => useFeatureFlag('my-feature'))
    expect(result.current.isEnabled).toBe(false)
  })

  it('is case-sensitive for feature flag names', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [{ name: 'my-feature', isEnabled: true }],
      isLoading: false,
    })

    const { result } = renderHook(() => useFeatureFlag('My-Feature'))
    expect(result.current.isEnabled).toBe(false)
  })

  it('passes polling interval to the query hook', () => {
    mockUseGetClientFeatureFlagsQuery.mockReturnValue({
      data: [],
      isLoading: false,
    })

    renderHook(() => useFeatureFlag('my-feature'))

    expect(mockUseGetClientFeatureFlagsQuery).toHaveBeenCalledWith(undefined, {
      pollingInterval: 60_000,
    })
  })
})
