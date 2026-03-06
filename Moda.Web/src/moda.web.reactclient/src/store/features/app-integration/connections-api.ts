import {
  ConnectionDetailsDto,
  ConnectionListDto,
  ConnectorListDto,
  CreateConnectionRequest,
  UpdateConnectionRequest,
} from '@/src/services/moda-api'
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
      providesTags: (result) => [
        QueryTags.Connection,
        ...(result?.map(({ id }) => ({ type: QueryTags.Connection, id })) ??
          []),
      ],
    }),

    getConnection: builder.query<ConnectionDetailsDto, string>({
      queryFn: async (id: string) => {
        try {
          const data = await getConnectionsClient().getConnection(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.ConnectionDetail, id: arg },
        { type: QueryTags.Connection, id: arg },
      ],
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

    createConnection: builder.mutation<string, CreateConnectionRequest>({
      queryFn: async (request) => {
        try {
          const data = await getConnectionsClient().createConnection(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: [{ type: QueryTags.Connection }],
    }),

    updateConnection: builder.mutation<void, UpdateConnectionRequest>({
      queryFn: async (request) => {
        try {
          const data = await getConnectionsClient().updateConnection(
            request.id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.Connection, id: arg.id },
        { type: QueryTags.ConnectionDetail, id: arg.id },
      ],
    }),

    deleteConnection: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getConnectionsClient().deleteConnection(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: [{ type: QueryTags.Connection }],
    }),
  }),
})

export const {
  useGetConnectionsQuery,
  useGetConnectionQuery,
  useGetConnectorsQuery,
  useCreateConnectionMutation,
  useUpdateConnectionMutation,
  useDeleteConnectionMutation,
} = connectionsApi
