import { apiSlice } from './../apiSlice'
import { QueryTags } from '../query-tags'
import { getStrategicInitiativesClient } from '@/src/services/clients'
import {
  CreateStrategicInitiativeRequest,
  ObjectIdAndKey,
  StrategicInitiativeListDto,
  UpdateStrategicInitiativeRequest,
} from '@/src/services/moda-api'

export const strategicInitiativesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getStrategicInitiatives: builder.query<
      StrategicInitiativeListDto[],
      number | undefined
    >({
      queryFn: async (status = undefined) => {
        try {
          const data =
            await getStrategicInitiativesClient().getStrategicInitiatives(
              status,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.StrategicInitiative, id: 'LIST' }],
    }),
    getStrategicInitiative: builder.query<StrategicInitiativeListDto, number>({
      queryFn: async (key) => {
        try {
          const data =
            await getStrategicInitiativesClient().getStrategicInitiative(
              key.toString(),
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.StrategicInitiative, id: arg },
      ],
    }),
    createStrategicInitiative: builder.mutation<
      ObjectIdAndKey,
      CreateStrategicInitiativeRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getStrategicInitiativesClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.StrategicInitiative, id: 'LIST' }]
      },
    }),
    updateStrategicInitiative: builder.mutation<
      void,
      { request: UpdateStrategicInitiativeRequest; cacheKey: number }
    >({
      queryFn: async ({ request, cacheKey }) => {
        try {
          const data = await getStrategicInitiativesClient().update(
            request.id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
        ]
      },
    }),
    approveStrategicInitiative: builder.mutation<
      void,
      { id: string; cacheKey: number }
    >({
      queryFn: async ({ id }) => {
        try {
          const data = await getStrategicInitiativesClient().approve(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
        ]
      },
    }),
    activateStrategicInitiative: builder.mutation<
      void,
      { id: string; cacheKey: number }
    >({
      queryFn: async ({ id }) => {
        try {
          const data = await getStrategicInitiativesClient().activate(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
        ]
      },
    }),
    completeStrategicInitiative: builder.mutation<
      void,
      { id: string; cacheKey: number }
    >({
      queryFn: async ({ id }) => {
        try {
          const data = await getStrategicInitiativesClient().complete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
        ]
      },
    }),
    cancelStrategicInitiative: builder.mutation<
      void,
      { id: string; cacheKey: number }
    >({
      queryFn: async ({ id }) => {
        try {
          const data = await getStrategicInitiativesClient().cancel(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicInitiative, id: 'LIST' },
          { type: QueryTags.StrategicInitiative, id: cacheKey },
        ]
      },
    }),
    deleteStrategicInitiative: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getStrategicInitiativesClient().delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [{ type: QueryTags.StrategicInitiative, id: 'LIST' }]
      },
    }),
  }),
})

export const {
  useGetStrategicInitiativesQuery,
  useGetStrategicInitiativeQuery,
  useCreateStrategicInitiativeMutation,
  useUpdateStrategicInitiativeMutation,
  useApproveStrategicInitiativeMutation,
  useActivateStrategicInitiativeMutation,
  useCompleteStrategicInitiativeMutation,
  useCancelStrategicInitiativeMutation,
  useDeleteStrategicInitiativeMutation,
} = strategicInitiativesApi
