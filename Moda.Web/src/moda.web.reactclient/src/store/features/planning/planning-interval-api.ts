import {
  ManagePlanningIntervalObjectiveWorkItemsRequest,
  PlanningIntervalDetailsDto,
  PlanningIntervalListDto,
  PlanningIntervalObjectiveDetailsDto,
  PlanningIntervalObjectiveListDto,
  UpdatePlanningIntervalObjectivesOrderRequest,
  WorkItemProgressDailyRollupDto,
  WorkItemsSummaryDto,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { getPlanningIntervalsClient } from '@/src/services/clients'
import { QueryTags } from '../query-tags'

export interface GetPlanningIntervalObjectivesRequest {
  planningIntervalId: string
  teamId?: string
}

export interface GetPlanningIntervalObjectiveRequest {
  planningIntervalId: string
  objectiveId: string
}

export interface GetObjectiveWorkItemsRequest {
  planningIntervalId: string
  objectiveId: string
}

export const planningIntervalApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getPlanningIntervals: builder.query<PlanningIntervalListDto[], null>({
      queryFn: async () => {
        try {
          const data = await getPlanningIntervalsClient().getList()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.PlanningInterval,
        ...result.map(({ key }) => ({ type: QueryTags.PlanningInterval, key })),
      ],
    }),
    getPlanningInterval: builder.query<PlanningIntervalDetailsDto, string>({
      queryFn: async (idOrKey: string) => {
        try {
          const data =
            await getPlanningIntervalsClient().getPlanningInterval(idOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.PlanningInterval, id: arg }, // typically arg is the key
      ],
    }),
    getPlanningIntervalObjectives: builder.query<
      PlanningIntervalObjectiveListDto[],
      GetPlanningIntervalObjectivesRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getPlanningIntervalsClient().getObjectives(
            request.planningIntervalId,
            request.teamId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.PlanningIntervalObjective,
        ...result.map(({ id }) => ({
          type: QueryTags.PlanningIntervalObjective,
          id,
        })),
      ],
    }),
    getPlanningIntervalObjective: builder.query<
      PlanningIntervalObjectiveDetailsDto,
      GetPlanningIntervalObjectiveRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getPlanningIntervalsClient().getObjectiveById(
            request.planningIntervalId,
            request.objectiveId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.PlanningIntervalObjective, id: arg.objectiveId },
      ],
    }),
    getObjectiveWorkItems: builder.query<
      WorkItemsSummaryDto,
      GetObjectiveWorkItemsRequest
    >({
      queryFn: async (request: GetObjectiveWorkItemsRequest) => {
        try {
          const data = await getPlanningIntervalsClient().getObjectiveWorkItems(
            request.planningIntervalId,
            request.objectiveId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        {
          type: QueryTags.PlanningIntervalObjectiveWorkItemsSummary,
          id: arg.objectiveId,
        },
      ],
    }),
    getObjectiveWorkItemMetrics: builder.query<
      WorkItemProgressDailyRollupDto[],
      GetObjectiveWorkItemsRequest
    >({
      queryFn: async (request: GetObjectiveWorkItemsRequest) => {
        try {
          const data =
            await getPlanningIntervalsClient().getObjectiveWorkItemMetrics(
              request.planningIntervalId,
              request.objectiveId,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        {
          type: QueryTags.PlanningIntervalObjectiveWorkItemMetric,
          id: arg.objectiveId,
        },
      ],
    }),
    updateObjectivesOrder: builder.mutation<
      void,
      UpdatePlanningIntervalObjectivesOrderRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getPlanningIntervalsClient().updateObjectivesOrder(
            request.planningIntervalId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [{ type: QueryTags.PlanningIntervalObjective }]
      },
    }),
    manageObjectiveWorkItems: builder.mutation<
      void,
      ManagePlanningIntervalObjectiveWorkItemsRequest
    >({
      queryFn: async (request) => {
        try {
          const data =
            await getPlanningIntervalsClient().manageObjectiveWorkItems(
              request.planningIntervalId,
              request.objectiveId,
              request,
            )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          {
            type: QueryTags.PlanningIntervalObjectiveWorkItemsSummary,
            id: arg.objectiveId,
          },
        ]
      },
    }),
  }),
})

export const {
  useGetPlanningIntervalsQuery,
  useGetPlanningIntervalQuery,
  useGetPlanningIntervalObjectivesQuery,
  useGetPlanningIntervalObjectiveQuery,
  useGetObjectiveWorkItemsQuery,
  useGetObjectiveWorkItemMetricsQuery,
  useUpdateObjectivesOrderMutation,
  useManageObjectiveWorkItemsMutation,
} = planningIntervalApi
