import { QueryClient, useMutation, useQuery, useQueryClient } from 'react-query'
import { getPermissionsClient, getRolesClient } from '../clients'
import {
  CreateOrUpdateRoleRequest,
  UpdateRolePermissionsRequest,
} from '../moda-api'
import { QK } from './query-keys'

// ROLES

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

export const useCreateRoleMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (params: CreateOrUpdateRoleRequest) =>
      (await getRolesClient()).createOrUpdate(params),
    onSuccess: (data) => {
      queryClient.invalidateQueries(QK.ROLES)
    },
  })
}

export const useUpdatePermissionsMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (params: UpdateRolePermissionsRequest) =>
      (await getRolesClient()).updatePermissions(params.roleId, {
        roleId: params.roleId,
        permissions: params.permissions,
      }),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.ROLES, variables.roleId])
    },
  })
}

export const useDeleteRoleMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => (await getRolesClient()).delete(id),
    onSuccess: (data) => {
      queryClient.invalidateQueries(QK.ROLES)
    },
  })
}

// PERMISSIONS

export const useGetPermissions = () => {
  return useQuery({
    queryKey: [QK.PERMISSIONS],
    queryFn: async () => (await getPermissionsClient()).getList(),
  })
}
