import { QueryClient, useMutation, useQuery } from 'react-query'
import { QK } from './query-keys'
import { getPermissionsClient, getRolesClient } from './clients'
import {
  CreateOrUpdateRoleRequest,
  UpdateRolePermissionsRequest,
} from './moda-api'

export const useGetRoles = () => {
  return useQuery({
    queryKey: [QK.ROLES],
    queryFn: async () => (await getRolesClient()).getList(),
  })
}

export const useGetRoleById = (id: string) => {
  return useQuery({
    queryKey: [QK.ROLES, id],
    queryFn: async () => (await getRolesClient()).getByIdWithPermissions(id),
    enabled: !!id,
  })
}

export const useGetPermissions = () => {
  return useQuery({
    queryKey: [QK.PERMISSIONS],
    queryFn: async () => (await getPermissionsClient()).getList(),
  })
}

export const useCreateRoleMutation = (queryClient: QueryClient) => {
  return useMutation({
    mutationFn: async (params: CreateOrUpdateRoleRequest) =>
      (await getRolesClient()).createOrUpdate(params),
    onSuccess: (data) => {
      queryClient.invalidateQueries(QK.ROLES)
    },
  })
}

export const useUpdatePermissionsMutation = (queryClient: QueryClient) => {
  return useMutation({
    mutationFn: async (params: UpdateRolePermissionsRequest) =>
      (await getRolesClient()).updatePermissions(params.roleId, {
        roleId: params.roleId,
        permissions: params.permissions,
      }),
    onSuccess: (data, context) => {
      queryClient.invalidateQueries([QK.ROLES, context.roleId])
    },
  })
}

export const useDeleteRoleMutation = (queryClient: QueryClient) => {
  return useMutation({
    mutationFn: async (id: string) => (await getRolesClient()).delete(id),
    onSuccess: (data) => {
      queryClient.invalidateQueries(QK.ROLES)
    },
  })
}
