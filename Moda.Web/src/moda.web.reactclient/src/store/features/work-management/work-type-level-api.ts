import {
  getWorkTypeLevelsClient,
  getWorkTypeTiersClient,
} from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  CreateWorkTypeLevelRequest,
  UpdateWorkTypeLevelRequest,
  WorkTypeLevelDto,
} from '@/src/services/moda-api'
import { BaseOptionType, OptionProps } from 'antd/es/select'

export const workTypeLevelApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getWorkTypeLevels: builder.query<WorkTypeLevelDto[], null>({
      queryFn: async () => {
        try {
          const data = await (await getWorkTypeLevelsClient()).getList()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.WorkTypeLevel,
        ...result.map(({ id }) => ({ type: QueryTags.WorkTypeLevel, id })),
      ],
    }),
    getWorkTypeLevelOptions: builder.query<BaseOptionType[], null>({
      queryFn: async () => {
        try {
          const tiers = await (await getWorkTypeTiersClient()).getList()
          const levels = await (await getWorkTypeLevelsClient()).getList()

          const data: BaseOptionType[] = tiers
            .sort((a, b) => a.order - b.order)
            .map((tier) => ({
              label: tier.name,
              options: levels
                .filter((level) => level.tier.id === tier.id)
                .sort((a, b) => a.order - b.order)
                .map((level) => ({
                  label: level.name,
                  value: level.id,
                })),
            }))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.WorkTypeLevelOption,
        ...result.map(({ id }) => ({
          type: QueryTags.WorkTypeLevelOption,
          id,
        })),
      ],
    }),
    getWorkTypeLevel: builder.query<WorkTypeLevelDto, number>({
      queryFn: async (id: number) => {
        try {
          const data = await (await getWorkTypeLevelsClient()).getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.WorkTypeLevel, id: result.id },
      ],
    }),
    createWorkTypeLevel: builder.mutation<number, CreateWorkTypeLevelRequest>({
      queryFn: async (request) => {
        try {
          const data = await (await getWorkTypeLevelsClient()).create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        QueryTags.WorkTypeLevel,
        QueryTags.WorkTypeLevelOption,
      ],
    }),
    updateWorkTypeLevel: builder.mutation<null, UpdateWorkTypeLevelRequest>({
      queryFn: async (request) => {
        try {
          await (await getWorkTypeLevelsClient()).update(request.id, request)
          return { data: null }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.WorkTypeLevel, id: arg.id },
        { type: QueryTags.WorkTypeLevelOption, id: arg.id },
      ],
    }),
  }),
  overrideExisting: false,
})

export const {
  useGetWorkTypeLevelsQuery,
  useGetWorkTypeLevelOptionsQuery,
  useGetWorkTypeLevelQuery,
  useCreateWorkTypeLevelMutation,
  useUpdateWorkTypeLevelMutation,
} = workTypeLevelApi
