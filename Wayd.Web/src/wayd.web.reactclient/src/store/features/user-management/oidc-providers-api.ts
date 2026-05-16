import { getOidcProvidersClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  CreateOidcProviderRequest,
  OidcProviderDto,
  OidcProviderListItemDto,
  TestOidcProviderDiscoveryResult,
  UpdateOidcProviderRequest,
} from '@/src/services/wayd-api'

export const oidcProvidersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getOidcProviders: builder.query<OidcProviderListItemDto[], void>({
      queryFn: async () => {
        try {
          const data = await getOidcProvidersClient().getList()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.OidcProvider, id: 'LIST' }],
    }),

    getOidcProvider: builder.query<OidcProviderDto, string>({
      queryFn: async (id) => {
        try {
          const data = await getOidcProvidersClient().getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        { type: QueryTags.OidcProvider, id: result?.id },
      ],
    }),

    createOidcProvider: builder.mutation<OidcProviderDto, CreateOidcProviderRequest>({
      queryFn: async (request) => {
        try {
          const data = await getOidcProvidersClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [{ type: QueryTags.OidcProvider, id: 'LIST' }],
    }),

    updateOidcProvider: builder.mutation<void, UpdateOidcProviderRequest>({
      queryFn: async (request) => {
        try {
          const data = await getOidcProvidersClient().update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.OidcProvider, id: 'LIST' },
        { type: QueryTags.OidcProvider, id: arg.id },
      ],
    }),

    deleteOidcProvider: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getOidcProvidersClient().delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [{ type: QueryTags.OidcProvider, id: 'LIST' }],
    }),

    testOidcProviderDiscovery: builder.mutation<TestOidcProviderDiscoveryResult, string>({
      queryFn: async (id) => {
        try {
          const data = await getOidcProvidersClient().testDiscovery(id)
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
  useGetOidcProvidersQuery,
  useGetOidcProviderQuery,
  useCreateOidcProviderMutation,
  useUpdateOidcProviderMutation,
  useDeleteOidcProviderMutation,
  useTestOidcProviderDiscoveryMutation,
} = oidcProvidersApi
