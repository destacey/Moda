import { getWorkTypeLevelsClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { WorkTypeLevelDto } from '@/src/services/moda-api'

export const workTypeLevelApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getWorkTypeLevels: builder.query<WorkTypeLevelDto[], null>({
      queryFn: async () => {
        try {
          const data = await (await getWorkTypeLevelsClient()).getList()
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.WorkTypeLevel,
        ...result.map(({ id }) => ({ type: QueryTags.WorkTypeLevel, id })),
      ],
    }),
    getWorkTypeLevel: builder.query<WorkTypeLevelDto, number>({
      queryFn: async (id: number) => {
        try {
          const data = await (await getWorkTypeLevelsClient()).getById(id)
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.WorkTypeLevel, id: result.id },
      ],
    }),
  }),
  overrideExisting: false,
})

export const { useGetWorkTypeLevelsQuery, useGetWorkTypeLevelQuery } =
  workTypeLevelApi
