import {
  CreateRoadmapRequest,
  ObjectIdAndKey,
  RoadmapDetailsDto,
  RoadmapListDto,
  UpdateRoadmapRequest,
  VisibilityDto,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { getRoadmapsClient } from '@/src/services/clients'
import { create } from 'lodash'
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
        { type: QueryTags.Roadmap, id: arg }, // typically arg is the key
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
    }),
    updateRoadmap: builder.mutation<void, UpdateRoadmapRequest>({
      queryFn: async (request) => {
        try {
          const data = await (
            await getRoadmapsClient()
          ).update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
    getVisibilityOptions: builder.query<OptionModel<number>[], void>({
      queryFn: async () => {
        try {
          const visibilities = await (
            await getRoadmapsClient()
          ).getVisibilityOptions()
          //const visibilities = _.sortBy(dtos, ['order'])
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
  useGetVisibilityOptionsQuery,
} = roadmapApi
