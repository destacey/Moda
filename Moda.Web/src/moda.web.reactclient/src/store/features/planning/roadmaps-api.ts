import {
  CreateRoadmapActivityRequest,
  CreateRoadmapMilestoneRequest,
  CreateRoadmapTimeboxRequest,
  ObjectIdAndKey,
  RoadmapActivityListDto,
  RoadmapItemDetailsDto,
  RoadmapItemListDto,
  UpdateRoadmapActivityRequest,
  UpdateRoadmapMilestoneRequest,
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
import { OptionModel } from '@/src/app/components/types'

export const roadmapApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getRoadmaps: builder.query<RoadmapListDto[], void>({
      queryFn: async () => {
        try {
          const data = await (await getRoadmapsClient()).getRoadmaps()
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
          const data = await (await getRoadmapsClient()).getRoadmap(idOrKey)
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
          const data = await (await getRoadmapsClient()).create(request)
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
          const data = await (
            await getRoadmapsClient()
          ).update(mutationRequest.request.id, mutationRequest.request)
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
    deleteRoadmap: builder.mutation<void, string>({
      queryFn: async (roadmapId) => {
        try {
          const data = await (await getRoadmapsClient()).delete(roadmapId)
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
          const data = await (await getRoadmapsClient()).getItems(roadmapId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.RoadmapItems, id: arg },
      ],
    }),
    getRoadmapItem: builder.query<
      RoadmapItemDetailsDto,
      { roadmapId: string; itemId: string }
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).getItem(request.roadmapId, request.itemId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.RoadmapItems, id: arg.itemId },
      ],
    }),
    getRoadmapActivities: builder.query<RoadmapActivityListDto[], string>({
      queryFn: async (roadmapId: string) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).getActivities(roadmapId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.RoadmapItems, id: arg },
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
          const data = await (
            await getRoadmapsClient()
          ).createItem(request.roadmapId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.RoadmapItems, id: arg.roadmapId }] // TODO: add a cache key to invalidate only the specific roadmap by the key instead of id
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
          const data = await (
            await getRoadmapsClient()
          ).updateItem(
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
          { type: QueryTags.RoadmapItems, id: arg.roadmapId },
          { type: QueryTags.RoadmapItems, id: arg.itemId },
        ]
      },
    }),
    // updateChildrenOrder: builder.mutation<
    //   void,
    //   UpdateRoadmapChildrenOrderRequest
    // >({
    //   queryFn: async (request) => {
    //     try {
    //       const data = await (
    //         await getRoadmapsClient()
    //       ).updateChildrenOrder(request.roadmapId, request)
    //       return { data }
    //     } catch (error) {
    //       console.error('API Error:', error)
    //       return { error }
    //     }
    //   },
    //   invalidatesTags: (result, error, arg) => [
    //     {
    //       type: QueryTags.RoadmapItems,
    //       id: arg.roadmapId,
    //     },
    //   ],
    // }),
    // updateChildOrder: builder.mutation<void, UpdateRoadmapChildOrderRequest>({
    //   queryFn: async (request) => {
    //     try {
    //       const data = await (
    //         await getRoadmapsClient()
    //       ).updateChildOrder(request.roadmapId, request.childRoadmapId, request)
    //       return { data }
    //     } catch (error) {
    //       console.error('API Error:', error)
    //       return { error }
    //     }
    //   },
    //   invalidatesTags: (result, error, arg) => [
    //     {
    //       type: QueryTags.RoadmapItems,
    //       id: arg.roadmapId,
    //     },
    //   ],
    // }),
    deleteRoadmapItem: builder.mutation<
      void,
      {
        roadmapId: string
        itemId: string
      }
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).deleteItem(request.roadmapId, request.itemId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.RoadmapItems, id: arg.roadmapId },
          { type: QueryTags.RoadmapItems, id: arg.itemId },
        ]
      },
    }),
    getVisibilityOptions: builder.query<OptionModel<number>[], void>({
      queryFn: async () => {
        try {
          const visibilities = await (
            await getRoadmapsClient()
          ).getVisibilityOptions()
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
  // useUpdateChildrenOrderMutation,
  // useUpdateChildOrderMutation,
  useDeleteRoadmapItemMutation,
  useGetVisibilityOptionsQuery,
} = roadmapApi
