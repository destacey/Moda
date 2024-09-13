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
import _ from 'lodash'
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
      providesTags: (result, error, arg) => [
        QueryTags.Roadmap,
        ...result.map(({ key }) => ({ type: QueryTags.Roadmap, key })),
      ],
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
      providesTags: (result, error, arg) => [
        { type: QueryTags.Roadmap, id: result?.key },
      ],
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
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.Roadmap },
        { type: QueryTags.RoadmapChildren },
      ],
    }),
    updateRoadmap: builder.mutation<
      void,
      { request: UpdateRoadmapRequest; cacheKey: number }
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
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.Roadmap, id: arg.cacheKey },
        { type: QueryTags.RoadmapChildren, id: arg.cacheKey }, // TODO: this is not working
      ],
    }),
    deleteRoadmap: builder.mutation<void, { id: string; cacheKey: number }>({
      queryFn: async (request) => {
        try {
          const data = await (await getRoadmapsClient()).delete(request.id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        // TODO: this is triggering a refetch on removed roadmap which is causing a 404
        QueryTags.Roadmap,
        //{ type: QueryTags.Roadmap, id: arg.cacheKey },
      ],
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
        ...result.map(({ key }) => ({ type: QueryTags.RoadmapChildren, key })),
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
      invalidatesTags: (result, error, arg) => [QueryTags.RoadmapChildren],
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
      invalidatesTags: (result, error, arg) => [QueryTags.RoadmapChildren],
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
      providesTags: (result, error, arg) => [
        QueryTags.RoadmapVisibility,
        ...result.map(({ value }) => ({
          type: QueryTags.RoadmapVisibility,
          value,
        })),
      ],
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
