import { getSprintsClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { SprintDetailsDto, SprintListDto } from '@/src/services/moda-api'

export const sprintsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getSprints: builder.query<SprintListDto[], void>({
      queryFn: async () => {
        try {
          const data = await getSprintsClient().getSprints(undefined)
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
  }),
})

export const { useGetSprintsQuery, useGetSprintQuery } = sprintsApi
