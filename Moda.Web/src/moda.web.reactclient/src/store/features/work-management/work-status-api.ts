import { getWorkStatusesClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { WorkStatusDto } from '@/src/services/moda-api'

export const workStatusApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getWorkStatuses: builder.query<WorkStatusDto[], boolean>({
      queryFn: async (includeInactive) => {
        try {
          const data = await (
            await getWorkStatusesClient()
          ).getList(includeInactive)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.WorkStatus,
        ...result.map(({ id }) => ({ type: QueryTags.WorkType, id })),
      ],
    }),
    getWorkStatus: builder.query<WorkStatusDto, number>({
      queryFn: async (id: number) => {
        try {
          const data = await (await getWorkStatusesClient()).getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [{ type: QueryTags.WorkStatus, id: result.id }],
    }),
  }),
  overrideExisting: false,
})

export const { useGetWorkStatusesQuery, useGetWorkStatusQuery } = workStatusApi
