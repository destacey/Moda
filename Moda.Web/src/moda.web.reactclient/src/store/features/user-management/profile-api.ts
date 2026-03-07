import { authenticatedFetch, getProfileClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { UserDetailsDto, UserPermissionsResponse } from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL

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
    changePassword: builder.mutation<
      void,
      { currentPassword: string; newPassword: string }
    >({
      queryFn: async (args) => {
        try {
          const response = await authenticatedFetch(
            `${apiUrl}/api/user-management/profiles/change-password`,
            {
              method: 'PUT',
              headers: { 'Content-Type': 'application/json' },
              body: JSON.stringify(args),
            },
          )
          if (!response.ok) {
            const errorData = await response.json()
            return {
              error: {
                status: response.status,
                data: errorData,
              },
            }
          }
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
  useChangePasswordMutation,
} = profileApi
