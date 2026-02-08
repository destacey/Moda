import { apiSlice } from '../apiSlice'
import {
  AssignUserRolesRequest,
  ManageRoleUsersRequest,
  UserDetailsDto,
  UserRoleDto,
} from '@/src/services/moda-api'
import { getUsersClient } from '@/src/services/clients'
import { QueryTags } from '../query-tags'
import { BaseOptionType } from 'antd/es/select'

export const usersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getUsers: builder.query<UserDetailsDto[], void>({
      queryFn: async () => {
        try {
          const data = await getUsersClient().getUsers()
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
          const data = await getUsersClient().getUser(id)
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
          const data = await getUsersClient().manageUserRoles(
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
        return [
          { type: QueryTags.UserRole, id: arg.userId },
          { type: QueryTags.User, id: arg.userId },
          { type: QueryTags.User, id: 'LIST' },
          { type: QueryTags.RoleUsers, id: 'LIST' },
        ]
      },
    }),

    manageRoleUsers: builder.mutation<void, ManageRoleUsersRequest>({
      queryFn: async (request) => {
        try {
          const data = await getUsersClient().manageRoleUsers(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [
          { type: QueryTags.RoleUsers, id: 'LIST' },
          { type: QueryTags.User, id: 'LIST' },
          { type: QueryTags.UserRole, id: 'LIST' },
        ]
      },
    }),

    getUserOptions: builder.query<BaseOptionType[], void>({
      queryFn: async () => {
        try {
          const users = await getUsersClient().getUsers()
          const data: BaseOptionType[] = users
            .map((user) => ({
              label: user.isActive
                ? `${user.firstName} ${user.lastName}`
                : `${user.firstName} ${user.lastName} (Inactive)`,
              value: user.id,
            }))
            .sort((a, b) => a.label.localeCompare(b.label))
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.UserOption, id: 'LIST' }],
    }),
  }),
})

export const {
  useGetUsersQuery,
  useGetUserQuery,
  useGetUserRolesQuery,
  useManageUserRolesMutation,
  useManageRoleUsersMutation,
  useGetUserOptionsQuery,
} = usersApi
