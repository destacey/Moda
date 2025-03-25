import { apiSlice } from '../apiSlice'
import {
  AssignUserRolesRequest,
  UserDetailsDto,
  UserRoleDto,
} from '@/src/services/moda-api'
import { getUsersClient } from '@/src/services/clients'
import { QueryTags } from '../query-tags'

export const usersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getUsers: builder.query<UserDetailsDto[], void>({
      queryFn: async () => {
        try {
          const data = await getUsersClient().getList()
          data.sort((a, b) => a.userName.localeCompare(b.userName))
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.User, id: 'LIST' }],
    }),
    getUser: builder.query<UserDetailsDto, string>({
      queryFn: async (id: string) => {
        try {
          const data = await getUsersClient().getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [{ type: QueryTags.User, id: result?.id }],
    }),
    getUserRoles: builder.query<
      UserRoleDto[],
      { id: string; includeUnassigned?: boolean }
    >({
      queryFn: async (request) => {
        try {
          const data = await getUsersClient().getRoles(
            request.id,
            request.includeUnassigned ?? false,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        // TODO: should we include includeUnassigned in the tag?
        { type: QueryTags.UserRole, id: arg.id },
      ],
    }),
    manageUserRoles: builder.mutation<void, AssignUserRolesRequest>({
      queryFn: async (request) => {
        try {
          const data = await getUsersClient().manageRoles(
            request.userId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.UserRole, id: arg.userId }]
      },
    }),
  }),
})

export const {
  useGetUsersQuery,
  useGetUserQuery,
  useGetUserRolesQuery,
  useManageUserRolesMutation,
} = usersApi
