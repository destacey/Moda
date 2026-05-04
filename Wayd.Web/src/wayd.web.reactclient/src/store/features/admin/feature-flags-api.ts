import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  FeatureFlagDto,
  FeatureFlagListDto,
  ToggleFeatureFlagRequest,
  UpdateFeatureFlagRequest,
} from '@/src/services/wayd-api'
import { getFeatureFlagsClient } from '@/src/services/clients'

export const featureFlagsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getFeatureFlags: builder.query<
      FeatureFlagListDto[],
      { includeArchived?: boolean } | void
    >({
      queryFn: async (args) => {
        try {
          const data = await getFeatureFlagsClient().featureFlags(
            args ? args.includeArchived : undefined,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) =>
        result
          ? [
              QueryTags.FeatureFlag,
              ...result.map(({ id }) => ({
                type: QueryTags.FeatureFlag as const,
                id,
              })),
            ]
          : [QueryTags.FeatureFlag],
    }),
    getFeatureFlag: builder.query<FeatureFlagDto, number>({
      queryFn: async (id) => {
        try {
          const data = await getFeatureFlagsClient().featureFlag(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (_result, _error, id) => [
        { type: QueryTags.FeatureFlag, id },
      ],
    }),
    updateFeatureFlag: builder.mutation<void, UpdateFeatureFlagRequest>({
      queryFn: async (request) => {
        try {
          await getFeatureFlagsClient().update(request.id, request)
          return { data: undefined as void }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (_result, _error, { id }) => [
        { type: QueryTags.FeatureFlag, id },
        QueryTags.FeatureFlag,
      ],
    }),
    toggleFeatureFlag: builder.mutation<void, ToggleFeatureFlagRequest>({
      queryFn: async (request) => {
        try {
          await getFeatureFlagsClient().toggle(request.id, request)
          return { data: undefined as void }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (_result, _error, { id }) => [
        { type: QueryTags.FeatureFlag, id },
        QueryTags.FeatureFlag,
        QueryTags.ClientFeatureFlag,
      ],
    }),
    archiveFeatureFlag: builder.mutation<void, number>({
      queryFn: async (id) => {
        try {
          await getFeatureFlagsClient().archive(id)
          return { data: undefined as void }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: [QueryTags.FeatureFlag, QueryTags.ClientFeatureFlag],
    }),
  }),
})

export const {
  useGetFeatureFlagsQuery,
  useGetFeatureFlagQuery,
  useUpdateFeatureFlagMutation,
  useToggleFeatureFlagMutation,
  useArchiveFeatureFlagMutation,
} = featureFlagsApi
