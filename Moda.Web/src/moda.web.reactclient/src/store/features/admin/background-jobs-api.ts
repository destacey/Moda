import {
  BackgroundJobDto,
  BackgroundJobTypeDto,
  CreateRecurringJobRequest,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getBackgroundJobsClient } from '@/src/services/clients'

export const backgroundJobsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getRunningJobs: builder.query<BackgroundJobDto[], null>({
      queryFn: async () => {
        try {
          const data = await (await getBackgroundJobsClient()).getRunningJobs()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.BackgroundJob,
        ...result.map(({ id }) => ({ type: QueryTags.BackgroundJob, id })),
      ],
    }),
    getJobTypes: builder.query<BackgroundJobTypeDto[], void>({
      queryFn: async () => {
        try {
          const data = await (await getBackgroundJobsClient()).getJobTypes()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.BackgroundJobType,
        ...result.map(({ id }) => ({ type: QueryTags.BackgroundJobType, id })),
      ],
    }),
    runJob: builder.mutation<void, number>({
      queryFn: async (jobTypeId) => {
        try {
          await (await getBackgroundJobsClient()).run(jobTypeId)
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
    createRecurringJob: builder.mutation<void, CreateRecurringJobRequest>({
      queryFn: async (request) => {
        try {
          await (await getBackgroundJobsClient()).create(request)
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
  }),
})

export const {
  useGetRunningJobsQuery,
  useGetJobTypesQuery,
  useRunJobMutation,
  useCreateRecurringJobMutation,
} = backgroundJobsApi
