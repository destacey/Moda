import {
  CreateRoadmapReponse,
  RoadmapLinkDto,
  UpdateRoadmapLinkOrderRequest,
  UpdateRoadmapLinksOrderRequest,
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
        //...result.map(({ key }) => ({ type: QueryTags.Roadmap, key })),
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
      // need to check result for null when 404 or after delete
      providesTags: (result, error, arg) => [
        QueryTags.Roadmap,
        //...result.map(({ key }) => ({ type: QueryTags.Roadmap, key })),
      ],
      // providesTags: (result, error, arg) => {
      //   if (result) {
      //     return [{ type: QueryTags.Roadmap, id: result.key }]
      //   }
      //   return []
      // },
    }),
    createRoadmap: builder.mutation<CreateRoadmapReponse, CreateRoadmapRequest>(
      {
        queryFn: async (request) => {
          try {
            const data = await (await getRoadmapsClient()).create(request)
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        invalidatesTags: (result, error, arg) => [QueryTags.Roadmap],
      },
    ),
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
      invalidatesTags: (result, error, arg) => [QueryTags.Roadmap],
      // invalidatesTags: (result, error, arg) => [
      //   { type: QueryTags.Roadmap, id: arg.cacheKey },
      // ],
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
    getRoadmapLinks: builder.query<RoadmapLinkDto[], string[]>({
      queryFn: async (parentIds: string[]) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).getChildRoadmapLinks(parentIds)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
    updateChildLinksOrder: builder.mutation<
      void,
      UpdateRoadmapLinksOrderRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).updateChildLinksOrder(request.roadmapId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [QueryTags.Roadmap],
    }),
    updateChildLinkOrder: builder.mutation<void, UpdateRoadmapLinkOrderRequest>(
      {
        queryFn: async (request) => {
          try {
            const data = await (
              await getRoadmapsClient()
            ).updateChildLinkOrder(
              request.roadmapId,
              request.roadmapLinkId,
              request,
            )
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        invalidatesTags: (result, error, arg) => [QueryTags.Roadmap],
      },
    ),
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
  useGetRoadmapLinksQuery,
  useUpdateChildLinksOrderMutation,
  useUpdateChildLinkOrderMutation,
  useGetVisibilityOptionsQuery,
} = roadmapApi
