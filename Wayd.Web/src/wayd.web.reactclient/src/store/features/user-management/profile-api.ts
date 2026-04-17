import { getProfileClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { ChangePasswordRequest, UserDetailsDto, UserPermissionsResponse, UserPreferencesDto } from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'

export const profileApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getProfile: builder.query<UserDetailsDto, void>({
      queryFn: async () => {
        try {
          const data = await getProfileClient().get()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
    getUserPermissions: builder.query<UserPermissionsResponse, void>({
      queryFn: async () => {
        try {
          const data = await getProfileClient().getPermissions()
          return { data }
        } catch (error: any) {
          console.error('Error fetching permissions:', error)
          // Extract status code from axios error or API exception
          const status = error?.status || error?.response?.status || 500
          return {
            error: {
              status,
              error:
                error instanceof Error
                  ? error.message
                  : 'Failed to load permissions',
            },
          }
        }
      },
      providesTags: () => [{ type: QueryTags.UserPermission, id: 'USER' }],
      // Cache current user permissions for 1 minute since they don't change often
      keepUnusedDataFor: 60,
    }),
    getUserPreferences: builder.query<UserPreferencesDto, void>({
      queryFn: async () => {
        try {
          const data = await getProfileClient().getPreferences()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.UserPreferences, id: 'USER' }],
    }),
    updateUserPreferences: builder.mutation<void, UserPreferencesDto>({
      queryFn: async (preferences) => {
        try {
          await getProfileClient().updatePreferences(preferences)
          return { data: null }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: [{ type: QueryTags.UserPreferences, id: 'USER' }],
    }),
    changePassword: builder.mutation<void, ChangePasswordRequest>({
      queryFn: async (args) => {
        try {
          await getProfileClient().changePassword(args)
          return { data: null }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
  }),
})

export const {
  useGetProfileQuery,
  useGetUserPermissionsQuery,
  useGetUserPreferencesQuery,
  useUpdateUserPreferencesMutation,
  useChangePasswordMutation,
} = profileApi
