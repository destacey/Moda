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
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
  }),
})

export const { useGlobalSearchQuery, useLazyGlobalSearchQuery } =
  searchApi
