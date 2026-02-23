import { getAnalyticsViewsClient } from '@/src/services/clients'
import {
  AnalyticsViewDetailsDto,
  AnalyticsViewListDto,
  AnalyticsViewResultDto,
  CreateAnalyticsViewRequest,
  RunAnalyticsViewRequest,
  UpdateAnalyticsViewRequest,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'

export const analyticsViewsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getAnalyticsViews: builder.query<AnalyticsViewListDto[], boolean>({
      queryFn: async (includeInactive: boolean = false) => {
        try {
          const data = await getAnalyticsViewsClient().getList(includeInactive)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.AnalyticsView,
        ...(result?.map(({ id }) => ({
          type: QueryTags.AnalyticsView as const,
          id,
        })) ?? []),
      ],
    }),

    getAnalyticsView: builder.query<AnalyticsViewDetailsDto, string>({
      queryFn: async (id: string) => {
        try {
          const data = await getAnalyticsViewsClient().getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.AnalyticsViewDetail, id: arg },
        { type: QueryTags.AnalyticsView, id: arg },
      ],
    }),

    createAnalyticsView: builder.mutation<string, CreateAnalyticsViewRequest>({
      queryFn: async (request) => {
        try {
          const data = await getAnalyticsViewsClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: [{ type: QueryTags.AnalyticsView }],
    }),

    updateAnalyticsView: builder.mutation<
      AnalyticsViewDetailsDto,
      UpdateAnalyticsViewRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getAnalyticsViewsClient().update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => [
        { type: QueryTags.AnalyticsView, id: arg.id },
        { type: QueryTags.AnalyticsViewDetail, id: arg.id },
      ],
    }),

    deleteAnalyticsView: builder.mutation<void, string>({
      queryFn: async (id: string) => {
        try {
          const data = await getAnalyticsViewsClient().delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: [{ type: QueryTags.AnalyticsView }],
    }),

    runAnalyticsView: builder.mutation<AnalyticsViewResultDto, RunAnalyticsViewRequest>(
      {
        queryFn: async (request) => {
          try {
            const data = await getAnalyticsViewsClient().run(request.id, request)
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
      },
    ),
  }),
})

export const {
  useGetAnalyticsViewsQuery,
  useGetAnalyticsViewQuery,
  useCreateAnalyticsViewMutation,
  useUpdateAnalyticsViewMutation,
  useDeleteAnalyticsViewMutation,
  useRunAnalyticsViewMutation,
} = analyticsViewsApi
