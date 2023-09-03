import { useMutation, useQuery, useQueryClient } from 'react-query'
import { QK } from './query-keys'
import { getAzureDevOpsBoardsConnectionsClient } from '../clients'
import {
  CreateAzureDevOpsBoardConnectionRequest,
  UpdateAzureDevOpsBoardConnectionRequest,
} from '../moda-api'

// AzDO BOARDS CONNECTIONS
export const useGetAzdoBoardsConnections = (
  includeDisabled: boolean = false,
) => {
  return useQuery({
    queryKey: [QK.AZDO_BOARDS_CONNECTIONS, includeDisabled],
    queryFn: async () =>
      (await getAzureDevOpsBoardsConnectionsClient()).getList(includeDisabled),
    staleTime: 60000,
  })
}

export const useGetAzdoBoardsConnectionById = (id: string) => {
  return useQuery({
    queryKey: [QK.AZDO_BOARDS_CONNECTIONS, id],
    queryFn: async () =>
      (await getAzureDevOpsBoardsConnectionsClient()).getById(id),
    staleTime: 60000,
  })
}

export const useCreateAzdoBoardsConnectionMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (connection: CreateAzureDevOpsBoardConnectionRequest) =>
      (await getAzureDevOpsBoardsConnectionsClient()).create(connection),
    onSuccess: () => {
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, false])
    },
  })
}

export const useUpdateAzdoBoardsConnectionMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (connection: UpdateAzureDevOpsBoardConnectionRequest) =>
      (await getAzureDevOpsBoardsConnectionsClient()).update(
        connection.id,
        connection,
      ),
    onSuccess: (data, context) => {
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, false])
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, true])
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, context.id])
    },
  })
}

export const useGetAzdoBoardsConfiguration = (id: string) => {
  return useQuery({
    queryKey: [QK.AZDO_BOARDS_CONNECTION_CONFIGURATIONS, id],
    queryFn: async () =>
      (await getAzureDevOpsBoardsConnectionsClient()).getConfig(id),
    staleTime: 60000,
  })
}
