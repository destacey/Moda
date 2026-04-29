import { getPlanningIntervalsClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { PlanningIntervalObjectiveHealthCheckDetailsDto } from '@/src/services/wayd-api'
import { QueryTags } from '../query-tags'

export interface GetObjectiveHealthChecksArgs {
  planningIntervalId: string
  objectiveId: string
}

export const healthChecksApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getObjectiveHealthChecks: builder.query<
      PlanningIntervalObjectiveHealthCheckDetailsDto[],
      GetObjectiveHealthChecksArgs
    >({
      queryFn: async ({ planningIntervalId, objectiveId }) => {
        try {
          const data = await getPlanningIntervalsClient().getObjectiveHealthChecks(
            planningIntervalId,
            objectiveId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result = [], error, { objectiveId }) => [
        { type: QueryTags.HealthChecksHealthReport, id: objectiveId },
        ...(result?.map(({ id }: { id: string }) => ({
          type: QueryTags.HealthChecksHealthReport,
          id,
        })) ?? []),
      ],
    }),
  }),
})

export const { useGetObjectiveHealthChecksQuery } = healthChecksApi
