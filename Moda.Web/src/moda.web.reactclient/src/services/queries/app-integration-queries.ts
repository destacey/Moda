import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import { getAzureDevOpsBoardsConnectionsClient } from '../clients'
import { CreateAzureDevOpsBoardConnectionRequest } from '../moda-api'

// CONNECTIONS
export const useGetConnections = (includeDisabled: boolean = false) => {
  return useQuery({
    queryKey: [QK.CONNECTIONS, includeDisabled],
    queryFn: async () =>
      (await getAzureDevOpsBoardsConnectionsClient()).getList(includeDisabled),
    staleTime: 60000,
  })
}

export const useCreateConnectionMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (connection: CreateAzureDevOpsBoardConnectionRequest) =>
      (await getAzureDevOpsBoardsConnectionsClient()).create(connection),
    onSuccess: () => {
      queryClient.invalidateQueries([QK.CONNECTIONS, false])
    },
  })
}
