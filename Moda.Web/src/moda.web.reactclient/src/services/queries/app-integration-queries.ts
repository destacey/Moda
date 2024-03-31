import {
  InitWorkProcessIntegrationRequest,
  InitWorkspaceIntegrationRequest,
  TestAzureDevOpsBoardConnectionRequest,
} from './../moda-api'
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
    enabled: !!id,
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
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, false])
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, true])
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, variables.id])
    },
  })
}

export const useDeleteAzdoBoardsConnectionMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (connectionId: string) =>
      (await getAzureDevOpsBoardsConnectionsClient()).delete(connectionId),
    onSuccess: (data, connectionId) => {
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, connectionId])
    },
  })
}

export interface UpdateAzdoBoardsConnectionSyncStateMutationRequest {
  connectionId: string
  isSyncEnabled: boolean
}
export const useUpdateAzdoBoardsConnectionSyncStateMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (
      request: UpdateAzdoBoardsConnectionSyncStateMutationRequest,
    ) =>
      (await getAzureDevOpsBoardsConnectionsClient()).updateSyncState(
        request.connectionId,
        request.isSyncEnabled,
      ),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, false])
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, true])
      queryClient.invalidateQueries([
        QK.AZDO_BOARDS_CONNECTIONS,
        variables.connectionId,
      ])
    },
  })
}

export const useSyncAzdoBoardsConnectionOrganizationMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (connectionId: string) =>
      (
        await getAzureDevOpsBoardsConnectionsClient()
      ).syncOrganizationConfiguration(connectionId),
    onSuccess: (data, connectionId) => {
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, connectionId])
    },
  })
}

export const useInitAzdoBoardsConnectionWorkProcessMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (initRequest: InitWorkProcessIntegrationRequest) =>
      (
        await getAzureDevOpsBoardsConnectionsClient()
      ).initWorkProcesssIntegration(initRequest.id, initRequest),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, variables.id])
    },
  })
}

export const useInitAzdoBoardsConnectionWorkspaceMutation = () => {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (initRequest: InitWorkspaceIntegrationRequest) =>
      (await getAzureDevOpsBoardsConnectionsClient()).initWorkspaceIntegration(
        initRequest.id,
        initRequest,
      ),
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries([QK.AZDO_BOARDS_CONNECTIONS, variables.id])
    },
  })
}

export const testAzdoBoardsConfiguration = async (
  configuration: TestAzureDevOpsBoardConnectionRequest,
) => {
  try {
    var result = await (
      await getAzureDevOpsBoardsConnectionsClient()
    ).testConfig(configuration)
    return 'success'
  } catch (error) {
    return error.supportMessage
  }
}
