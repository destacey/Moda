import {
  authenticatedFetch,
  getAnalyticsViewsClient,
} from '@/src/services/clients'
import {
  AnalyticsViewDetailsDto,
  AnalyticsViewListDto,
  CreateAnalyticsViewRequest,
  UpdateAnalyticsViewRequest,
} from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'

export interface AnalyticsViewDataQueryParams {
  id: string
  pageNumber?: number
  pageSize?: number
}

export interface AnalyticsViewColumnDto {
  field: string
  displayName: string
}

export interface AnalyticsViewDataResultDto {
  columns: AnalyticsViewColumnDto[]
  rows: Record<string, unknown>[]
  totalCount: number
}

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

    getAnalyticsViewData: builder.query<
      AnalyticsViewDataResultDto,
      AnalyticsViewDataQueryParams
    >({
      queryFn: async (params) => {
        try {
          const queryParts: string[] = []
          if (params.pageNumber != null)
            queryParts.push(`pageNumber=${params.pageNumber}`)
          if (params.pageSize != null)
            queryParts.push(`pageSize=${params.pageSize}`)

          const queryString =
            queryParts.length > 0 ? `?${queryParts.join('&')}` : ''
          const response = await authenticatedFetch(
            `/api/analytics/views/${params.id}/data${queryString}`,
          )

          if (!response.ok) {
            const errorText = await response.text()
            return {
              error: {
                status: response.status,
                data: errorText,
              },
            }
          }

          const data: AnalyticsViewDataResultDto = await response.json()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
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

  }),
})

export const {
  useGetAnalyticsViewsQuery,
  useGetAnalyticsViewQuery,
  useGetAnalyticsViewDataQuery,
  useLazyGetAnalyticsViewDataQuery,
  useCreateAnalyticsViewMutation,
  useUpdateAnalyticsViewMutation,
  useDeleteAnalyticsViewMutation,
} = analyticsViewsApi
