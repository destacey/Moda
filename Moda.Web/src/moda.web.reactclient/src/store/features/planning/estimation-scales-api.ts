import { getEstimationScalesClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  CreateEstimationScaleRequest,
  EstimationScaleDetailsDto,
  EstimationScaleListDto,
  UpdateEstimationScaleRequest,
} from '@/src/services/moda-api'

export const estimationScalesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getEstimationScales: builder.query<EstimationScaleListDto[], void>({
      queryFn: async () => {
        try {
          const data = await getEstimationScalesClient().getList()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.EstimationScale, id: 'LIST' }],
    }),

    getEstimationScale: builder.query<EstimationScaleDetailsDto, number>({
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
  useDeleteEstimationScaleMutation,
} = estimationScalesApi
