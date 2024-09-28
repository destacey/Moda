import {
  ObjectIdAndKey,
  RoadmapChildrenDto,
  UpdateRoadmapChildOrderRequest,
  UpdateRoadmapChildrenOrderRequest,
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
        if (!arg.parentId) {
          return [{ type: QueryTags.Roadmap, id: 'LIST' }]
        } else {
          return [{ type: QueryTags.RoadmapChildren, id: arg.parentId }]
        }
      },
    }),
    updateRoadmap: builder.mutation<
      void,
      {
        request: UpdateRoadmapRequest
        cacheKey: number
        parentCacheKey?: string | null
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
        const tags: {
          type: QueryTags
          id: any
        }[] = [{ type: QueryTags.Roadmap, id: arg.cacheKey }]

        if (arg.parentCacheKey) {
          tags.push({ type: QueryTags.RoadmapChildren, id: arg.parentCacheKey })
        } else {
          tags.push({ type: QueryTags.Roadmap, id: 'LIST' })
        }

        return tags
      },
    }),
    deleteRoadmap: builder.mutation<
      void,
      {
        id: string
        cacheKey: number
        parentCacheKey?: string | null
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
        const tags: {
          type: QueryTags
          id: any
        }[] = [
          // LIST is needed to handle orphaned children
          { type: QueryTags.Roadmap, id: 'LIST' },
          //{ type: QueryTags.Roadmap, id: arg.cacheKey }, // This triggers a 404 error
        ]

        if (arg.parentCacheKey) {
          tags.push({ type: QueryTags.RoadmapChildren, id: arg.parentCacheKey })
        }

        return tags
      },
    }),
    getRoadmapChildren: builder.query<RoadmapChildrenDto[], string[]>({
      queryFn: async (parentIds: string[]) => {
        try {
          const data = await (await getRoadmapsClient()).getChildren(parentIds)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.RoadmapChildren,
        ...arg.map((parentId) => ({
          type: QueryTags.RoadmapChildren,
          id: parentId,
        })),
      ],
    }),
    updateChildrenOrder: builder.mutation<
      void,
      UpdateRoadmapChildrenOrderRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).updateChildrenOrder(request.roadmapId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        {
          type: QueryTags.RoadmapChildren,
          id: arg.roadmapId,
        },
      ],
    }),
    updateChildOrder: builder.mutation<void, UpdateRoadmapChildOrderRequest>({
      queryFn: async (request) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).updateChildOrder(request.roadmapId, request.childRoadmapId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        {
          type: QueryTags.RoadmapChildren,
          id: arg.roadmapId,
        },
      ],
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
  useGetRoadmapChildrenQuery,
  useUpdateChildrenOrderMutation,
  useUpdateChildOrderMutation,
  useGetVisibilityOptionsQuery,
} = roadmapApi
