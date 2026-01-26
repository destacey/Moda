import { getSprintsClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  SprintBacklogItemDto,
  SprintDetailsDto,
  SprintListDto,
  SprintWorkItemMetricsDto,
} from '@/src/services/moda-api'

export const sprintsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getSprints: builder.query<SprintListDto[], void>({
      queryFn: async () => {
        try {
          const data = await getSprintsClient().getSprints()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.Sprint, id: 'LIST' }],
    }),

    getSprint: builder.query<SprintDetailsDto, number>({
      queryFn: async (key: number) => {
        try {
          const data = await getSprintsClient().getSprint(key.toString())
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [{ type: QueryTags.Sprint, id: result?.key }],
    }),

    getSprintBacklog: builder.query<SprintBacklogItemDto[], number>({
      queryFn: async (sprintKey: number) => {
        try {
          const data = await getSprintsClient().getSprintBacklog(
            sprintKey.toString(),
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, sprintKey) => [
        { type: QueryTags.SprintBacklog, id: sprintKey },
      ],
    }),

    getSprintMetrics: builder.query<SprintWorkItemMetricsDto, number>({
      queryFn: async (sprintKey: number) => {
        try {
          const data = await getSprintsClient().getSprintMetrics(
            sprintKey.toString(),
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, sprintKey) => [
        { type: QueryTags.SprintMetrics, id: sprintKey },
      ],
    }),
  }),
})

export const {
  useGetSprintsQuery,
  useGetSprintQuery,
  useGetSprintBacklogQuery,
  useGetSprintMetricsQuery,
} = sprintsApi
