import {
  CreateRoadmapItemRequest,
  ObjectIdAndKey,
  RoadmapItemDto,
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
    getRoadmaps: builder.query<RoadmapListDto[], null>({
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
    deleteRoadmap: builder.mutation<
      void,
      {
        id: string
      }
    >({
      queryFn: async (request) => {
        try {
          const data = await (await getRoadmapsClient()).delete(request.id)
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
    getRoadmapItems: builder.query<RoadmapItemDto[], string>({
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
    createRoadmapActivity: builder.mutation<
      ObjectIdAndKey,
      CreateRoadmapItemRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).createActivity(request.roadmapId, request)
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
  useCreateRoadmapActivityMutation,
  // useUpdateChildrenOrderMutation,
  // useUpdateChildOrderMutation,
  useGetVisibilityOptionsQuery,
} = roadmapApi
