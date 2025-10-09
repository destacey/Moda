import {
  CreateRoadmapActivityRequest,
  CreateRoadmapMilestoneRequest,
  CreateRoadmapTimeboxRequest,
  ObjectIdAndKey,
  ReorganizeRoadmapActivityRequest,
  RoadmapActivityListDto,
  RoadmapItemDetailsDto,
  RoadmapItemListDto,
  UpdateRoadmapActivityDatesRequest,
  UpdateRoadmapActivityRequest,
  UpdateRoadmapMilestoneDatesRequest,
  UpdateRoadmapMilestoneRequest,
  UpdateRoadmapTimeboxDatesRequest,
  UpdateRoadmapTimeboxRequest,
} from './../../../services/moda-api'
import {
  CreateRoadmapRequest,
  RoadmapDetailsDto,
  RoadmapListDto,
  UpdateRoadmapRequest,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getRoadmapsClient } from '@/src/services/clients'
import { OptionModel } from '@/src/components/types'

export const roadmapApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getRoadmaps: builder.query<RoadmapListDto[], void>({
      queryFn: async () => {
        try {
          const data = await getRoadmapsClient().getRoadmaps()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.Roadmap, id: 'LIST' }],
    }),
    getRoadmap: builder.query<RoadmapDetailsDto, string>({
      queryFn: async (idOrKey: string) => {
        try {
          const data = await getRoadmapsClient().getRoadmap(idOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [{ type: QueryTags.Roadmap, id: result?.key }],
    }),
    createRoadmap: builder.mutation<ObjectIdAndKey, CreateRoadmapRequest>({
      queryFn: async (request) => {
        try {
          const data = await getRoadmapsClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.Roadmap, id: 'LIST' }]
      },
    }),
    updateRoadmap: builder.mutation<
      void,
      {
        request: UpdateRoadmapRequest
        cacheKey: number
      }
    >({
      queryFn: async (mutationRequest) => {
        try {
          const data = await getRoadmapsClient().update(
            mutationRequest.request.id,
            mutationRequest.request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.Roadmap, id: 'LIST' },
          { type: QueryTags.Roadmap, id: arg.cacheKey },
        ]
      },
    }),
    deleteRoadmap: builder.mutation<void, string>({
      queryFn: async (roadmapId) => {
        try {
          const data = await getRoadmapsClient().delete(roadmapId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.Roadmap, id: 'LIST' }]
      },
    }),
    getRoadmapItems: builder.query<RoadmapItemListDto[], string>({
      queryFn: async (roadmapId: string) => {
        try {
          const data = await getRoadmapsClient().getItems(roadmapId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.RoadmapItem, id: arg },
      ],
    }),
    getRoadmapItem: builder.query<
      RoadmapItemDetailsDto,
      { roadmapId: string; itemId: string }
    >({
      queryFn: async (request) => {
        try {
          const data = await getRoadmapsClient().getItem(
            request.roadmapId,
            request.itemId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.RoadmapItem, id: arg.itemId },
      ],
    }),
    getRoadmapActivities: builder.query<RoadmapActivityListDto[], string>({
      queryFn: async (roadmapId: string) => {
        try {
          const data = await getRoadmapsClient().getActivities(roadmapId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.RoadmapItem, id: arg },
      ],
    }),
    createRoadmapItem: builder.mutation<
      ObjectIdAndKey,
      | CreateRoadmapActivityRequest
      | CreateRoadmapMilestoneRequest
      | CreateRoadmapTimeboxRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getRoadmapsClient().createItem(
            request.roadmapId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.RoadmapItem, id: arg.roadmapId }]
      },
    }),
    updateRoadmapItem: builder.mutation<
      void,
      | UpdateRoadmapActivityRequest
      | UpdateRoadmapMilestoneRequest
      | UpdateRoadmapTimeboxRequest
    >({
      queryFn: async (mutationRequest) => {
        try {
          const data = await getRoadmapsClient().updateItem(
            mutationRequest.roadmapId,
            mutationRequest.itemId,
            mutationRequest,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.RoadmapItem, id: arg.roadmapId },
          { type: QueryTags.RoadmapItem, id: arg.itemId },
        ]
      },
    }),
    updateRoadmapItemDates: builder.mutation<
      void,
      | UpdateRoadmapActivityDatesRequest
      | UpdateRoadmapMilestoneDatesRequest
      | UpdateRoadmapTimeboxDatesRequest
    >({
      queryFn: async (mutationRequest) => {
        try {
          const data = await getRoadmapsClient().updateItemDates(
            mutationRequest.roadmapId,
            mutationRequest.itemId,
            mutationRequest,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.RoadmapItem, id: arg.roadmapId },
          { type: QueryTags.RoadmapItem, id: arg.itemId },
        ]
      },
    }),
    reorganizeRoadmapActivity: builder.mutation<
      void,
      {
        request: ReorganizeRoadmapActivityRequest
      }
    >({
      queryFn: async (mutationRequest) => {
        try {
          const data = await getRoadmapsClient().reorganizeActivity(
            mutationRequest.request.roadmapId,
            mutationRequest.request.activityId,
            mutationRequest.request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.RoadmapItem, id: arg.request.roadmapId },
          { type: QueryTags.RoadmapItem, id: arg.request.activityId },
        ]
      },
    }),
    deleteRoadmapItem: builder.mutation<
      void,
      {
        roadmapId: string
        itemId: string
      }
    >({
      queryFn: async (request) => {
        try {
          const data = await getRoadmapsClient().deleteItem(
            request.roadmapId,
            request.itemId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.RoadmapItem, id: arg.roadmapId },
          { type: QueryTags.RoadmapItem, id: arg.itemId },
        ]
      },
    }),
    getVisibilityOptions: builder.query<OptionModel<number>[], void>({
      queryFn: async () => {
        try {
          const visibilities = await getRoadmapsClient().getVisibilityOptions()
          const data: OptionModel<number>[] = visibilities
            .sort((a, b) => a.order - b.order)
            .map((s) => ({
              value: s.id,
              label: s.name,
            }))
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [QueryTags.RoadmapVisibility],
    }),
  }),
})

export const {
  useGetRoadmapsQuery,
  useGetRoadmapQuery,
  useCreateRoadmapMutation,
  useUpdateRoadmapMutation,
  useDeleteRoadmapMutation,
  useGetRoadmapItemsQuery,
  useGetRoadmapItemQuery,
  useGetRoadmapActivitiesQuery,
  useCreateRoadmapItemMutation,
  useUpdateRoadmapItemMutation,
  useUpdateRoadmapItemDatesMutation,
  useReorganizeRoadmapActivityMutation,
  useDeleteRoadmapItemMutation,
  useGetVisibilityOptionsQuery,
} = roadmapApi
