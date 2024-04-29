import {
  ManagePlanningIntervalObjectiveWorkItemsRequest,
  PlanningIntervalDetailsDto,
  PlanningIntervalListDto,
  WorkItemListDto,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { getPlanningIntervalsClient } from '@/src/services/clients'
import { QueryTags } from '../query-tags'

export interface GetObjectiveWorkItemsRequest {
  planningIntervalId: string
  objectiveId: string
}

export const planningIntervalApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getPlanningIntervals: builder.query<PlanningIntervalListDto[], null>({
      queryFn: async () => {
        try {
          const data = await (await getPlanningIntervalsClient()).getList()
          return { data }
        } catch (error) {
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.PlanningInterval,
        ...result.map(({ id }) => ({ type: QueryTags.PlanningInterval, id })),
      ],
    }),
    getPlanningInterval: builder.query<PlanningIntervalDetailsDto, string>({
      queryFn: async (id: string) => {
        try {
          const data = await (await getPlanningIntervalsClient()).getById(id)
          return { data }
        } catch (error) {
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.PlanningInterval, id: arg }, // typically arg is the key
      ],
    }),
    getObjectiveWorkItems: builder.query<
      WorkItemListDto[],
      GetObjectiveWorkItemsRequest
    >({
      queryFn: async (request: GetObjectiveWorkItemsRequest) => {
        try {
          const data = await (
            await getPlanningIntervalsClient()
          ).getObjectiveWorkItems(
            request.planningIntervalId,
            request.objectiveId,
          )
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.PlanningIntervalObjectiveWorkItem,
        ...result.map(({ id }) => ({
          type: QueryTags.PlanningIntervalObjectiveWorkItem,
          id,
        })),
      ],
    }),
    manageObjectiveWorkItems: builder.mutation<
      void,
      ManagePlanningIntervalObjectiveWorkItemsRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getPlanningIntervalsClient()
          ).manageObjectiveWorkItems(
            request.planningIntervalId,
            request.objectiveId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          {
            type: QueryTags.PlanningIntervalObjectiveWorkItem,
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
  useGetObjectiveWorkItemsQuery,
  useManageObjectiveWorkItemsMutation,
} = planningIntervalApi
