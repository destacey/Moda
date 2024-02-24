import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import { getWorkProcessesClient } from '../clients'

// WORK PROCESSES
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
