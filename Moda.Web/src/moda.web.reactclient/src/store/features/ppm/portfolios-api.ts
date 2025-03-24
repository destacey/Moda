import { getPortfoliosClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import {
  CreatePortfolioRequest,
  ObjectIdAndKey,
  ProjectListDto,
  ProjectPortfolioDetailsDto,
  ProjectPortfolioListDto,
  StrategicInitiativeListDto,
  UpdatePortfolioRequest,
} from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'
import { BaseOptionType } from 'antd/es/select'

export const portfoliosApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getPortfolios: builder.query<ProjectPortfolioListDto[], number | undefined>(
      {
        queryFn: async (portfolioState = undefined) => {
          try {
            const data =
              await getPortfoliosClient().getPortfolios(portfolioState)
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        providesTags: () => [{ type: QueryTags.Portfolio, id: 'LIST' }],
      },
    ),
    getPortfolio: builder.query<ProjectPortfolioDetailsDto, number>({
      queryFn: async (key) => {
        try {
          const data = await getPortfoliosClient().getPortfolio(key.toString())
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.Portfolio, id: arg },
      ],
    }),
    createPortfolio: builder.mutation<ObjectIdAndKey, CreatePortfolioRequest>({
      queryFn: async (request) => {
        try {
          const data = await getPortfoliosClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.Portfolio, id: 'LIST' }]
      },
    }),
    updatePortfolio: builder.mutation<
      void,
      { request: UpdatePortfolioRequest; cacheKey: number }
    >({
      queryFn: async ({ request, cacheKey }) => {
        try {
          const data = await getPortfoliosClient().update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.Portfolio, id: 'LIST' },
          { type: QueryTags.Portfolio, id: cacheKey },
        ]
      },
    }),
    activatePortfolio: builder.mutation<void, { id: string; cacheKey: number }>(
      {
        queryFn: async ({ id }) => {
          try {
            const data = await getPortfoliosClient().activate(id)
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        invalidatesTags: (result, error, { cacheKey }) => {
          return [
            { type: QueryTags.Portfolio, id: 'LIST' },
            { type: QueryTags.Portfolio, id: cacheKey },
          ]
        },
      },
    ),
    closePortfolio: builder.mutation<void, { id: string; cacheKey: number }>({
      queryFn: async ({ id }) => {
        try {
          const data = await getPortfoliosClient().close(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.Portfolio, id: 'LIST' },
          { type: QueryTags.Portfolio, id: cacheKey },
        ]
      },
    }),
    archivePortfolio: builder.mutation<void, { id: string; cacheKey: number }>({
      queryFn: async ({ id }) => {
        try {
          const data = await getPortfoliosClient().archive(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.Portfolio, id: 'LIST' },
          { type: QueryTags.Portfolio, id: cacheKey },
        ]
      },
    }),
    deletePortfolio: builder.mutation<
      void,
      { portfolioId: string; cacheKey: number }
    >({
      queryFn: async ({ portfolioId }) => {
        try {
          const data = await getPortfoliosClient().delete(portfolioId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [{ type: QueryTags.Portfolio, id: 'LIST' }]
      },
    }),
    getPortfolioProjects: builder.query<ProjectListDto[], string>({
      queryFn: async (portfolioIdOrKey) => {
        try {
          const data = await getPortfoliosClient().getProjects(portfolioIdOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.PortfolioProjects, id: 'LIST' },
        { type: QueryTags.PortfolioProjects, id: arg },
      ],
    }),
    getPortfolioStrategicInitiatives: builder.query<
      StrategicInitiativeListDto[],
      string
    >({
      queryFn: async (portfolioIdOrKey) => {
        try {
          const data = await getPortfoliosClient().getStrategicInitiatives(
            portfolioIdOrKey,
            null,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.PortfolioStrategicInitiatives, id: 'LIST' },
        { type: QueryTags.PortfolioStrategicInitiatives, id: arg },
      ],
    }),
    getPortfolioOptions: builder.query<BaseOptionType[], void>({
      queryFn: async () => {
        try {
          const portfolios = await getPortfoliosClient().getPortfolioOptions()

          const data: BaseOptionType[] = portfolios
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((category) => ({
              label: category.name,
              value: category.id,
            }))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
  }),
})

export const {
  useGetPortfoliosQuery,
  useGetPortfolioQuery,
  useCreatePortfolioMutation,
  useUpdatePortfolioMutation,
  useActivatePortfolioMutation,
  useClosePortfolioMutation,
  useArchivePortfolioMutation,
  useDeletePortfolioMutation,
  useGetPortfolioProjectsQuery,
  useGetPortfolioStrategicInitiativesQuery,
  useGetPortfolioOptionsQuery,
} = portfoliosApi
