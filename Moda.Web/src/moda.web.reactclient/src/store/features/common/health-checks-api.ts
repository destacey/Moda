import { getHealthChecksClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { HealthCheckDto } from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'

export const healthChecksApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getHealthReport: builder.query<HealthCheckDto[], string>({
      queryFn: async (objectId: string) => {
        try {
          const data = await (
            await getHealthChecksClient()
          ).getHealthReport(objectId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result = [], error, objectId) => [
        { type: QueryTags.HealthChecksHealthReport, id: objectId },
        ...(result?.map(({ id }: { id: string }) => ({
          type: QueryTags.HealthChecksHealthReport,
          id,
        })) ?? []),
      ],
    }),
  }),
})

export const { useGetHealthReportQuery } = healthChecksApi
