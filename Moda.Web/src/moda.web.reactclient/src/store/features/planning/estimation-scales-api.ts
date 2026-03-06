import { getEstimationScalesClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  CreateEstimationScaleRequest,
  EstimationScaleDto,
  UpdateEstimationScaleRequest,
} from '@/src/services/moda-api'

export const estimationScalesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getEstimationScales: builder.query<
      EstimationScaleDto[],
      boolean | void
    >({
      queryFn: async (includeInactive) => {
        try {
          const data = await getEstimationScalesClient().getScales(
            includeInactive || undefined,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.EstimationScale, id: 'LIST' }],
    }),

    getEstimationScale: builder.query<EstimationScaleDto, number>({
      queryFn: async (id: number) => {
        try {
          const data = await getEstimationScalesClient().getScale(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        { type: QueryTags.EstimationScale, id: result?.id },
      ],
    }),

    createEstimationScale: builder.mutation<
      number,
      CreateEstimationScaleRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getEstimationScalesClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [{ type: QueryTags.EstimationScale, id: 'LIST' }],
    }),

    updateEstimationScale: builder.mutation<void, UpdateEstimationScaleRequest>(
      {
        queryFn: async (request) => {
          try {
            const data = await getEstimationScalesClient().update(
              request.estimationScaleId,
              request,
            )
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        invalidatesTags: (result, error, arg) => [
          { type: QueryTags.EstimationScale, id: 'LIST' },
          { type: QueryTags.EstimationScale, id: arg.estimationScaleId },
        ],
      },
    ),

    setEstimationScaleActiveStatus: builder.mutation<
      void,
      { id: number; isActive: boolean }
    >({
      queryFn: async (request) => {
        try {
          const data = await getEstimationScalesClient().setActiveStatus(
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
        { type: QueryTags.EstimationScale, id: 'LIST' },
        { type: QueryTags.EstimationScale, id: arg.id },
      ],
    }),

    deleteEstimationScale: builder.mutation<void, number>({
      queryFn: async (id) => {
        try {
          const data = await getEstimationScalesClient().delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [{ type: QueryTags.EstimationScale, id: 'LIST' }],
    }),
  }),
})

export const {
  useGetEstimationScalesQuery,
  useGetEstimationScaleQuery,
  useCreateEstimationScaleMutation,
  useUpdateEstimationScaleMutation,
  useSetEstimationScaleActiveStatusMutation,
  useDeleteEstimationScaleMutation,
} = estimationScalesApi
