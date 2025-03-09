import { getWorkTypeTiersClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { WorkTypeTierDto } from '@/src/services/moda-api'

export const workTypeTierApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getWorkTypeTiers: builder.query<WorkTypeTierDto[], null>({
      queryFn: async () => {
        try {
          const data = await getWorkTypeTiersClient().getList()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.WorkTypeTier,
        ...result.map(({ id }) => ({ type: QueryTags.WorkTypeTier, id })),
      ],
    }),
  }),
  overrideExisting: false,
})

export const { useGetWorkTypeTiersQuery } = workTypeTierApi
