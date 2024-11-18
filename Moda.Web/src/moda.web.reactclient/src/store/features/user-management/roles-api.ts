import { apiSlice } from '../apiSlice'
import {
  CreateOrUpdateRoleRequest,
  RoleDto,
  RoleListDto,
  UpdateRolePermissionsRequest,
} from '@/src/services/moda-api'
import { getRolesClient } from '@/src/services/clients'
import { QueryTags } from '../query-tags'

export const rolesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getRoles: builder.query<RoleListDto[], void>({
      queryFn: async () => {
        try {
          const data = await (await getRolesClient()).getList()
          return {
            data: data.sort((a, b) => a.name.localeCompare(b.name)),
          }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result = []) => [
        { type: QueryTags.Roles, id: 'LIST' },
        ...result.map(({ id }) => ({ type: QueryTags.Roles, id })),
      ],
    }),

    getRole: builder.query<RoleDto, string>({
      queryFn: async (id: string) => {
        try {
          const data = await (await getRolesClient()).getByIdWithPermissions(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, id) => [{ type: QueryTags.Roles, id }],
    }),

    upsertRole: builder.mutation<string, CreateOrUpdateRoleRequest>({
      queryFn: async (request) => {
        try {
          const data = await (await getRolesClient()).createOrUpdate(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { id }) => [
        { type: QueryTags.Roles, id: 'LIST' },
        { type: QueryTags.Roles, id: id ?? 'NEW' },
      ],
    }),

    updatePermissions: builder.mutation<void, UpdateRolePermissionsRequest>({
      queryFn: async (request) => {
        try {
          const data = await (
            await getRolesClient()
          ).updatePermissions(request.roleId, {
            roleId: request.roleId,
            permissions: request.permissions,
          })
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { roleId }) => [
        { type: QueryTags.Roles, id: roleId },
      ],
    }),

    deleteRole: builder.mutation<void, string>({
      queryFn: async (id: string) => {
        try {
          const data = await (await getRolesClient()).delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, id) => [
        { type: QueryTags.Roles, id: 'LIST' },
      ],
    }),
  }),
})

// Export hooks
export const {
  useGetRolesQuery,
  useGetRoleQuery,
  useUpsertRoleMutation,
  useUpdatePermissionsMutation,
  useDeleteRoleMutation,
} = rolesApi
