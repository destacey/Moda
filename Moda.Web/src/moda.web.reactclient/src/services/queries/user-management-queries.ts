import { QueryClient, useMutation, useQuery, useQueryClient } from 'react-query'
import {
  getPermissionsClient,
  getRolesClient,
  getUsersClient,
} from '../clients'
import {
  CreateOrUpdateRoleRequest,
  UpdateRolePermissionsRequest,
} from '../moda-api'
import { QK } from './query-keys'

// USERS

export const useGetUsers = () => {
  return useQuery({
    queryKey: [QK.USERS],
    queryFn: async () => (await getUsersClient()).getList(),
    select: (data) => data.sort((a, b) => a.userName.localeCompare(b.userName)),
  })
}

export const useGetUserById = (id: string) => {
  return useQuery({
    queryKey: [QK.USERS, id],
    queryFn: async () => (await getUsersClient()).getById(id),
    enabled: !!id,
  })
}

export const useGetUserRoles = (
  id: string,
  includeUnassigned: boolean = false,
) => {
  return useQuery({
    queryKey: [QK.USER_ROLES, id],
    queryFn: async () =>
      (await getUsersClient()).getRoles(id, includeUnassigned),
    enabled: !!id,
  })
}

// ROLES

export const useGetRoles = () => {
  return useQuery({
    queryKey: [QK.ROLES],
    queryFn: async () => (await getRolesClient()).getList(),
    select: (data) => data.sort((a, b) => a.name.localeCompare(b.name)),
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
