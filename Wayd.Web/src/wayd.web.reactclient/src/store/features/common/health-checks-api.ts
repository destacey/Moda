import { getHealthChecksClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { HealthStatusDto } from '@/src/services/wayd-api'
import { QueryTags } from '../query-tags'

export const healthChecksApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getHealthStatuses: builder.query<HealthStatusDto[], void>({
      queryFn: async () => {
        try {
          const data = await getHealthChecksClient().getStatuses()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [
        { type: QueryTags.HealthChecksStatusOptions, id: 'LIST' },
      ],
    }),
  }),
})

export const { useGetHealthStatusesQuery } = healthChecksApi
