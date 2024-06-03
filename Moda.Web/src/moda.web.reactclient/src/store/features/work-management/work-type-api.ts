import { getWorkTypesClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  UpdateWorkTypeRequest,
  WorkTypeDto,
  WorkTypeLevelDto,
} from '@/src/services/moda-api'

export const workTypeApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getWorkTypes: builder.query<WorkTypeDto[], boolean>({
      queryFn: async (includeInactive) => {
        try {
          const data = await (
            await getWorkTypesClient()
          ).getList(includeInactive)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.WorkType,
        ...result.map(({ id }) => ({ type: QueryTags.WorkType, id })),
      ],
    }),
    getWorkType: builder.query<WorkTypeLevelDto, number>({
      queryFn: async (id: number) => {
        try {
          const data = await (await getWorkTypesClient()).getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.WorkType, id: result.id },
      ],
    }),
    updateWorkType: builder.mutation<void, UpdateWorkTypeRequest>({
      queryFn: async (request) => {
        try {
          const data = await (
            await getWorkTypesClient()
          ).update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.WorkType, id: arg.id },
      ],
    }),
  }),
  overrideExisting: false,
})

export const {
  useGetWorkTypesQuery,
  useGetWorkTypeQuery,
  useUpdateWorkTypeMutation,
} = workTypeApi
