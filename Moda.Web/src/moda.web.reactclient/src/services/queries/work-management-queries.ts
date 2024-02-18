import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import {
  getWorkProcessesClient,
  getWorkStatusesClient,
  getWorkTypesClient,
} from '../clients'

// WORK PROCESSES
export const useGetWorkProcesses = (includeDisabled: boolean = false) => {
  return useQuery({
    queryKey: [QK.WORK_PROCESSES, includeDisabled],
    queryFn: async () =>
      (await getWorkProcessesClient()).getList(includeDisabled),
    staleTime: 60000,
  })
}

export const useGetWorkProcessesByIdOrKey = (idOrKey: string) => {
  return useQuery({
    queryKey: [QK.WORK_PROCESSES, idOrKey],
    queryFn: async () => (await getWorkProcessesClient()).get(idOrKey),
    enabled: !!idOrKey,
  })
}

export interface ChangeWorkProcessIsActiveMutationRequest {
  id: string
  isActive: boolean
}
export const useChangeWorkProcessIsActiveMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({
      id,
      isActive,
    }: ChangeWorkProcessIsActiveMutationRequest) => {
      isActive
        ? (await getWorkProcessesClient()).activate(id)
        : (await getWorkProcessesClient()).deactivate(id)
    },
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.WORK_PROCESSES])
      queryClient.invalidateQueries([QK.WORK_PROCESSES, variables.id])
    },
  })
}

// WORK STATUSES
export const useGetWorkStatuses = (includeDisabled: boolean = false) => {
  return useQuery({
    queryKey: [QK.WORK_STATUSES, includeDisabled],
    queryFn: async () =>
      (await getWorkStatusesClient()).getList(includeDisabled),
    staleTime: 60000,
  })
}

// WORK TYPES
export const useGetWorkTypes = (includeDisabled: boolean = false) => {
  return useQuery({
    queryKey: [QK.WORK_TYPES, includeDisabled],
    queryFn: async () => (await getWorkTypesClient()).getList(includeDisabled),
    staleTime: 60000,
  })
}
