'use client'

import { useCallback } from 'react'
import { UserPreferencesDto } from '@/src/services/moda-api'
import {
  useGetUserPreferencesQuery,
  useUpdateUserPreferencesMutation,
} from '@/src/store/features/user-management/profile-api'

const DEFAULT_PREFERENCES: UserPreferencesDto = { tours: {} }

export const useUserPreferences = () => {
  const { data: preferences, isLoading } = useGetUserPreferencesQuery()
  const [updatePreferences] = useUpdateUserPreferencesMutation()

  const current = preferences ?? DEFAULT_PREFERENCES

  const setPreferences = useCallback(
    (updater: (prev: UserPreferencesDto) => UserPreferencesDto) => {
      const updated = updater(current)
      updatePreferences(updated)
    },
    [current, updatePreferences],
  )

  return { preferences: current, isLoading, setPreferences }
}

export const useTourCompleted = (tourKey: string) => {
  const { preferences, isLoading, setPreferences } = useUserPreferences()

  const isCompleted = preferences.tours[tourKey] ?? false

  const markCompleted = useCallback(() => {
    setPreferences((prev) => ({
      ...prev,
      tours: { ...prev.tours, [tourKey]: true },
    }))
  }, [tourKey, setPreferences])

  const resetTour = useCallback(() => {
    setPreferences((prev) => ({
      ...prev,
      tours: { ...prev.tours, [tourKey]: false },
    }))
  }, [tourKey, setPreferences])

  return { isCompleted, isLoading, markCompleted, resetTour }
}
