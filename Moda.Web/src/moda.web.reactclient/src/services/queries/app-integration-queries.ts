import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import { getAzureDevOpsBoardsConnectionsClient } from '../clients'
import {
  CreateAzureDevOpsBoardConnectionRequest,
  UpdateAzureDevOpsBoardConnectionRequest,
} from '../moda-api'

// CONNECTIONS
export const useGetConnections = (includeDisabled: boolean = false) => {
  return useQuery({
    queryKey: [QK.CONNECTIONS, includeDisabled],
    queryFn: async () =>
      (await getAzureDevOpsBoardsConnectionsClient()).getList(includeDisabled),
    staleTime: 60000,
  })
}

export const useGetConnectionById = (id: string) => {
  return useQuery({
    queryKey: [QK.CONNECTIONS, id],
    queryFn: async () =>
      (await getAzureDevOpsBoardsConnectionsClient()).getById(id),
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

export const useUpdateConnectionMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (connection: UpdateAzureDevOpsBoardConnectionRequest) =>
      (await getAzureDevOpsBoardsConnectionsClient()).update(
        connection.id,
        connection,
      ),
    onSuccess: (data, context) => {
      queryClient.invalidateQueries([QK.CONNECTIONS, false])
      queryClient.invalidateQueries([QK.CONNECTIONS, true])
      queryClient.invalidateQueries([QK.CONNECTIONS, context.id])
    },
  })
}
