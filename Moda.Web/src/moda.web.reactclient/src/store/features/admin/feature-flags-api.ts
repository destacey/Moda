import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  CreateFeatureFlagRequest,
  FeatureFlagDto,
  FeatureFlagListDto,
  ToggleFeatureFlagRequest,
  UpdateFeatureFlagRequest,
} from '@/src/services/moda-api'
import { getFeatureFlagsClient } from '@/src/services/clients'

export const featureFlagsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getFeatureFlags: builder.query<
      FeatureFlagListDto[],
      { includeArchived?: boolean } | void
    >({
      queryFn: async (args) => {
        try {
          const data = await getFeatureFlagsClient().getFeatureFlags(
            args?.includeArchived,
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
          const data = await getFeatureFlagsClient().getFeatureFlag(id)
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
    createFeatureFlag: builder.mutation<number, CreateFeatureFlagRequest>({
      queryFn: async (request) => {
        try {
          const data = await getFeatureFlagsClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: [QueryTags.FeatureFlag, QueryTags.ClientFeatureFlag],
    }),
    updateFeatureFlag: builder.mutation<void, UpdateFeatureFlagRequest>({
      queryFn: async (request) => {
        try {
          await getFeatureFlagsClient().update(request.id, request)
          return { data: null }
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
          return { data: null }
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
          return { data: null }
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
  useCreateFeatureFlagMutation,
  useUpdateFeatureFlagMutation,
  useToggleFeatureFlagMutation,
  useArchiveFeatureFlagMutation,
} = featureFlagsApi
