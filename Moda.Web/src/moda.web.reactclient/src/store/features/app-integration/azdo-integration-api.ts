import {
  AzureDevOpsBoardsConnectionDetailsDto,
  AzureDevOpsBoardsTeamConfigurationDto,
  ConnectionListDto,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { getAzureDevOpsBoardsConnectionsClient } from '@/src/services/clients'
import { QueryTags } from '../query-tags'

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
      AzureDevOpsBoardsTeamConfigurationDto,
      string
    >({
      queryFn: async (connectionId: string) => {
        try {
          const data = await (
            await getAzureDevOpsBoardsConnectionsClient()
          ).getTeams(connectionId)
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.AzdoConnectionTeams, id: arg }, // typically arg is the key
      ],
    }),
  }),
})

export const {
  useGetAzdoConnectionsQuery,
  useGetAzdoConnectionByIdQuery,
  useGetAzdoConnectionTeamsQuery,
} = azdoIntegrationApi
