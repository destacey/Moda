import {
  AzdoConnectionTeamMappingsRequest,
  AzureDevOpsBoardsConnectionDetailsDto,
  AzureDevOpsBoardsWorkspaceTeamDto,
  ConnectionListDto,
  CreateAzureDevOpsBoardConnectionRequest,
  InitWorkProcessIntegrationRequest,
  InitWorkspaceIntegrationRequest,
  TestAzureDevOpsBoardConnectionRequest,
  UpdateAzureDevOpsBoardConnectionRequest,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { getAzureDevOpsBoardsConnectionsClient } from '@/src/services/clients'
import { QueryTags } from '../query-tags'

export interface GetAzdoConnectionTeamsRequest {
  connectionId: string
  workspaceId: string | null
}

export const azdoIntegrationApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getAzdoConnections: builder.query<ConnectionListDto[], boolean>({
      queryFn: async (includeDisabled: boolean = false) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).getList(includeDisabled)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.Connections,
        ...result.map(({ id }) => ({ type: QueryTags.Connections, id })),
      ],
    }),
    getAzdoConnection: builder.query<
      AzureDevOpsBoardsConnectionDetailsDto,
      string
    >({
      queryFn: async (id: string) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.ConnectionDetails, id: arg }, // typically arg is the key
        { type: QueryTags.Connections, id: arg }, // typically arg is the key
      ],
    }),
    createAzdoConnection: builder.mutation<
      string,
      CreateAzureDevOpsBoardConnectionRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.Connections }]
      },
    }),
    updateAzdoConnection: builder.mutation<
      void,
      UpdateAzureDevOpsBoardConnectionRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.Connections, id: arg.id },
          { type: QueryTags.ConnectionDetails, id: arg.id },
        ]
      },
    }),
    deleteAzdoConnection: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.Connections }]
      },
    }),
    updateAzdoConnectionSyncState: builder.mutation<
      void,
      {
        connectionId: string
        isSyncEnabled: boolean
      }
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).updateSyncState(request.connectionId, request.isSyncEnabled)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.Connections, id: arg.connectionId },
          { type: QueryTags.ConnectionDetails, id: arg.connectionId },
        ]
      },
    }),
    syncAzdoConnectionOrganization: builder.mutation<void, string>({
      queryFn: async (connectionId) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).syncOrganizationConfiguration(connectionId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.ConnectionDetails, id: arg }]
      },
    }),
    initAzdoConnectionWorkProcess: builder.mutation<
      void,
      InitWorkProcessIntegrationRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).initWorkProcesssIntegration(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.ConnectionDetails, id: arg.id }]
      },
    }),
    initAzdoConnectionWorkspace: builder.mutation<
      void,
      InitWorkspaceIntegrationRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).initWorkspaceIntegration(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.ConnectionDetails, id: arg.id }]
      },
    }),

    getAzdoConnectionTeams: builder.query<
      AzureDevOpsBoardsWorkspaceTeamDto[],
      GetAzdoConnectionTeamsRequest
    >({
      queryFn: async (request: GetAzdoConnectionTeamsRequest) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).getConnectionTeams(request.connectionId, request.workspaceId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.AzdoConnectionTeams,
        ...result.map(() => ({
          type: QueryTags.AzdoConnectionTeams,
          id: arg.connectionId,
        })),
      ],
    }),
    mapAzdoConnectionTeams: builder.mutation<
      void,
      AzdoConnectionTeamMappingsRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).mapConnectionTeams(request.connectionId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.AzdoConnectionTeams, id: arg.connectionId }]
      },
    }),
    testAzdoConfiguration: builder.mutation<
      void,
      TestAzureDevOpsBoardConnectionRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).testConfig(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
  }),
})

export const {
  useGetAzdoConnectionsQuery,
  useGetAzdoConnectionQuery,
  useCreateAzdoConnectionMutation,
  useUpdateAzdoConnectionMutation,
  useDeleteAzdoConnectionMutation,
  useUpdateAzdoConnectionSyncStateMutation,
  useSyncAzdoConnectionOrganizationMutation,
  useInitAzdoConnectionWorkProcessMutation,
  useInitAzdoConnectionWorkspaceMutation,
  useGetAzdoConnectionTeamsQuery,
  useMapAzdoConnectionTeamsMutation,
  useTestAzdoConfigurationMutation,
} = azdoIntegrationApi
