import { getRisksClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import {
  CreateRiskRequest,
  ObjectIdAndKey,
  RiskDetailsDto,
  RiskListDto,
  UpdateRiskRequest,
} from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'
import { BaseOptionType } from 'antd/es/select'
import _ from 'lodash'
import { OptionModel } from '@/src/components/types'

export const risksApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getRisk: builder.query<RiskDetailsDto, number>({
      queryFn: async (key) => {
        try {
          const data = await getRisksClient().getRisk(key.toString())
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [{ type: QueryTags.Risk, id: arg }],
    }),
    getMyRisks: builder.query<RiskListDto[], string>({
      queryFn: async (username) => {
        try {
          const data = await getRisksClient().getMyRisks()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.MyRisk, id: arg },
      ],
    }),
    createRisk: builder.mutation<ObjectIdAndKey, CreateRiskRequest>({
      queryFn: async (request) => {
        try {
          const data = await getRisksClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [
          { type: QueryTags.Risk, id: 'LIST' },
          { type: QueryTags.MyRisk },
          { type: QueryTags.PlanningIntervalRisk },
          // team risks, team of team risks
        ]
      },
    }),
    updateRisk: builder.mutation<
      void,
      { request: UpdateRiskRequest; cacheKey: number }
    >({
      queryFn: async ({ request }) => {
        try {
          const data = await getRisksClient().update(request.riskId, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.Risk, id: cacheKey },
          { type: QueryTags.Risk, id: 'LIST' },
          { type: QueryTags.MyRisk },
          { type: QueryTags.PlanningIntervalRisk },
          // team risks, team of team risks
        ]
      },
    }),
    getRiskStatusOptions: builder.query<OptionModel<number>[], void>({
      queryFn: async () => {
        try {
          const portfolios = await getRisksClient().getStatuses()

          const statuses = _.sortBy(portfolios, ['order'])
          const data: OptionModel<number>[] = statuses.map((category) => ({
            label: category.name,
            value: category.id,
          }))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.RiskStatusOptions, id: 'LIST' }],
    }),
    getRiskCategoryOptions: builder.query<OptionModel<number>[], void>({
      queryFn: async () => {
        try {
          const portfolios = await getRisksClient().getCategories()

          const categories = _.sortBy(portfolios, ['order'])
          const data: OptionModel<number>[] = categories.map((category) => ({
            label: category.name,
            value: category.id,
          }))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.RiskCategoryOptions, id: 'LIST' }],
    }),
    getRiskGradeOptions: builder.query<OptionModel<number>[], void>({
      queryFn: async () => {
        try {
          const portfolios = await getRisksClient().getGrades()

          const grades = _.sortBy(portfolios, ['order'])
          const data: OptionModel<number>[] = grades.map((category) => ({
            label: category.name,
            value: category.id,
          }))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.RiskGradeOptions, id: 'LIST' }],
    }),
  }),
})

export const {
  useGetRiskQuery,
  useGetMyRisksQuery,
  useCreateRiskMutation,
  useUpdateRiskMutation,
  useGetRiskStatusOptionsQuery,
  useGetRiskCategoryOptionsQuery,
  useGetRiskGradeOptionsQuery,
} = risksApi
