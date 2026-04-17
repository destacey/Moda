import { apiSlice } from './apiSlice'
import { QueryTags } from './query-tags'
import { ClientFeatureFlagDto } from '@/src/services/moda-api'
import { getFeatureFlagsClient } from '@/src/services/clients'

export const clientFeatureFlagsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getClientFeatureFlags: builder.query<ClientFeatureFlagDto[], void>({
      queryFn: async () => {
        try {
          const data =
            await getFeatureFlagsClient().getEnabledFeatureFlags()
          return { data }
        } catch (error) {
          const message =
            error instanceof Error ? error.message : 'Unknown error'
          console.error('API Error:', message)
          return { error: { status: 'CUSTOM_ERROR', error: message } }
        }
      },
      providesTags: [QueryTags.ClientFeatureFlag],
    }),
  }),
})

export const { useGetClientFeatureFlagsQuery } = clientFeatureFlagsApi
