import { ConnectionListDto, ConnectorListDto } from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getConnectionsClient } from '@/src/services/clients'

export interface GetAzdoConnectionTeamsRequest {
  connectionId: string
  workspaceId: string | null
}

export const connectionsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getConnections: builder.query<ConnectionListDto[], boolean>({
      queryFn: async (includeDisabled: boolean = false) => {
        try {
          const data =
            await getConnectionsClient().getConnections(includeDisabled)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.Connection, id: 'LIST' }],
    }),

    getConnectors: builder.query<ConnectorListDto[], void>({
      queryFn: async () => {
        try {
          const data = await getConnectionsClient().getConnectors()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.Connectors, id: 'LIST' }],
    }),
  }),
})

export const { useGetConnectionsQuery, useGetConnectorsQuery } = connectionsApi
