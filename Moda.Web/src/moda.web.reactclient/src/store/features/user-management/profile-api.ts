import { getProfileClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { UserDetailsDto } from '@/src/services/moda-api'
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
    getUserPermissions: builder.query<string[], void>({
      queryFn: async () => {
        try {
          const data = await getProfileClient().getPermissions()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.UserPermission, id: 'USER' }],
      // Cache current user permissions for 1 minute since they don't change often
      keepUnusedDataFor: 60,
    }),
    getInternalEmployeeId: builder.query<string, void>({
      queryFn: async () => {
        try {
          const data = await getProfileClient().getInternalEmployeeId()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        { type: QueryTags.InternalEmployeeId, id: 'EMPLOYEEID' },
      ],
      keepUnusedDataFor: 300, // Cache internal employee ID for 5 minutes since it doesn't change often
    }),
  }),
})

export const {
  useGetProfileQuery,
  useGetUserPermissionsQuery,
  useGetInternalEmployeeIdQuery,
} = profileApi
