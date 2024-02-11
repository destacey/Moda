import { useQuery } from 'react-query'
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
