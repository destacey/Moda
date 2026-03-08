import { useGetClientFeatureFlagsQuery } from '@/src/store/features/feature-flags-api'

/**
 * Hook to check if a feature flag is enabled.
 *
 * @param featureName - The kebab-case name of the feature flag (e.g., "new-dashboard-layout")
 * @returns boolean - Whether the feature is enabled. Returns false while loading or on error.
 *
 * @example
 * const isEnabled = useFeatureFlag('new-dashboard-layout')
 * if (isEnabled) {
 *   return <NewDashboard />
 * }
 * return <LegacyDashboard />
 */
export function useFeatureFlag(featureName: string): boolean {
  const { data: features } = useGetClientFeatureFlagsQuery(undefined, {
    pollingInterval: 60_000, // Refresh every 60 seconds
  })

  return (
    features?.some((f) => f.name === featureName && f.isEnabled) ?? false
  )
}
