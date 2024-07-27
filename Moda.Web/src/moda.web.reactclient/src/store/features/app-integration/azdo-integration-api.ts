import {
  AzureDevOpsBoardsConnectionDetailsDto,
  AzureDevOpsBoardsWorkspaceTeamDto,
  ConnectionListDto,
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
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.Connections,
        ...result.map(({ id }) => ({ type: QueryTags.Connections, id })),
      ],
    }),
    getAzdoConnectionById: builder.query<
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
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.Connections, id: arg }, // typically arg is the key
      ],
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
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.AzdoConnectionTeams,
        ...result.map(({ teamId }) => ({
          type: QueryTags.AzdoConnectionTeams,
          teamId,
        })),
      ],
    }),
  }),
})

export const {
  useGetAzdoConnectionsQuery,
  useGetAzdoConnectionByIdQuery,
  useGetAzdoConnectionTeamsQuery,
} = azdoIntegrationApi
