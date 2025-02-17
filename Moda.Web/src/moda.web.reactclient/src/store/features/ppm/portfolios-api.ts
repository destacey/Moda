import { getPortfoliosClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import {
  CreatePortfolioRequest,
  ObjectIdAndKey,
  ProjectPortfolioListDto,
  UpdatePortfolioRequest,
} from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'

export const portfoliosApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getPortfolios: builder.query<ProjectPortfolioListDto[], number | undefined>(
      {
        queryFn: async (portfolioState = undefined) => {
          try {
            const data = await (
              await getPortfoliosClient()
            ).getPortfolios(portfolioState)
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        providesTags: () => [{ type: QueryTags.Portfolio, id: 'LIST' }],
      },
    ),
    getPortfolio: builder.query<ProjectPortfolioListDto, number>({
      queryFn: async (key) => {
        try {
          const data = await (
            await getPortfoliosClient()
          ).getPortfolio(key.toString())
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
          const data = await (await getPortfoliosClient()).create(request)
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
          const data = await (
            await getPortfoliosClient()
          ).update(request.id, request)
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
            const data = await (await getPortfoliosClient()).activate(id)
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
          const data = await (await getPortfoliosClient()).close(id)
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
          const data = await (await getPortfoliosClient()).archive(id)
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
          const data = await (await getPortfoliosClient()).delete(portfolioId)
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
} = portfoliosApi
