import {
  AzdoConnectionTeamMappingsRequest,
  AzureDevOpsConnectionDetailsDto,
  AzureDevOpsWorkspaceTeamDto,
  ConnectionListDto,
  CreateAzureDevOpsConnectionRequest,
  InitWorkProcessIntegrationRequest,
  InitWorkspaceIntegrationRequest,
  TestAzureDevOpsConnectionRequest,
  UpdateAzureDevOpsConnectionRequest,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { getAzureDevOpsConnectionsClient } from '@/src/services/clients'
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
          const data =
            await getAzureDevOpsConnectionsClient().getList(includeDisabled)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.AzdoConnection,
        ...result.map(({ id }) => ({ type: QueryTags.AzdoConnection, id })),
      ],
    }),

    getAzdoConnection: builder.query<AzureDevOpsConnectionDetailsDto, string>({
      queryFn: async (id: string) => {
        try {
          const data = await getAzureDevOpsConnectionsClient().getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.AzdoConnectionDetail, id: arg }, // typically arg is the key
        { type: QueryTags.AzdoConnection, id: arg }, // typically arg is the key
      ],
    }),

    createAzdoConnection: builder.mutation<
      string,
      CreateAzureDevOpsConnectionRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getAzureDevOpsConnectionsClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.AzdoConnection }]
      },
    }),

    updateAzdoConnection: builder.mutation<
      void,
      UpdateAzureDevOpsConnectionRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getAzureDevOpsConnectionsClient().update(
            request.id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.AzdoConnection, id: arg.id },
          { type: QueryTags.AzdoConnectionDetail, id: arg.id },
        ]
      },
    }),

    deleteAzdoConnection: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getAzureDevOpsConnectionsClient().delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.AzdoConnection }]
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
          const data = await getAzureDevOpsConnectionsClient().updateSyncState(
            request.connectionId,
            request.isSyncEnabled,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.AzdoConnection, id: arg.connectionId },
          { type: QueryTags.AzdoConnectionDetail, id: arg.connectionId },
        ]
      },
    }),

    syncAzdoConnectionOrganization: builder.mutation<void, string>({
      queryFn: async (connectionId) => {
        try {
          const data =
            await getAzureDevOpsConnectionsClient().syncOrganizationConfiguration(
              connectionId,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.AzdoConnectionDetail, id: arg }]
      },
    }),

    initAzdoConnectionWorkProcess: builder.mutation<
      void,
      InitWorkProcessIntegrationRequest
    >({
      queryFn: async (request) => {
        try {
          const data =
            await getAzureDevOpsConnectionsClient().initWorkProcesssIntegration(
              request.id,
              request,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.AzdoConnectionDetail, id: arg.id }]
      },
    }),

    initAzdoConnectionWorkspace: builder.mutation<
      void,
      InitWorkspaceIntegrationRequest
    >({
      queryFn: async (request) => {
        try {
          const data =
            await getAzureDevOpsConnectionsClient().initWorkspaceIntegration(
              request.id,
              request,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.AzdoConnectionDetail, id: arg.id }]
      },
    }),

    getAzdoConnectionTeams: builder.query<
      AzureDevOpsWorkspaceTeamDto[],
      GetAzdoConnectionTeamsRequest
    >({
      queryFn: async (request: GetAzdoConnectionTeamsRequest) => {
        try {
          const data =
            await getAzureDevOpsConnectionsClient().getConnectionTeams(
              request.connectionId,
              request.workspaceId,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.AzdoConnectionTeam,
        ...result.map(() => ({
          type: QueryTags.AzdoConnectionTeam,
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
          const data =
            await getAzureDevOpsConnectionsClient().mapConnectionTeams(
              request.connectionId,
              request,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.AzdoConnectionTeam, id: arg.connectionId }]
      },
    }),

    testAzdoConfiguration: builder.mutation<
      void,
      TestAzureDevOpsConnectionRequest
    >({
      queryFn: async (request) => {
        try {
          const data =
            await getAzureDevOpsConnectionsClient().testConfig(request)
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
