import { getSearchClient } from '@/src/services/clients'
import { GlobalSearchResultDto } from '@/src/services/moda-api'
import { apiSlice } from '../apiSlice'

export interface GlobalSearchRequest {
  query: string
  maxResultsPerCategory?: number
}

export const searchApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    globalSearch: builder.query<GlobalSearchResultDto, GlobalSearchRequest>({
      queryFn: async ({ query, maxResultsPerCategory }) => {
        try {
          const data = await getSearchClient().search(
            query,
            maxResultsPerCategory,
          )
          return { data }
        } catch (error) {
          const message =
            error instanceof Error ? error.message : 'Unknown error'
          console.error('API Error:', message)
          return { error: { status: 'CUSTOM_ERROR', error: message } }
        }
      },
    }),
  }),
})

export const { useGlobalSearchQuery, useLazyGlobalSearchQuery } =
  searchApi
