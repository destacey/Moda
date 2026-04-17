import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  PersonalAccessTokenDto,
  CreatePersonalAccessTokenResult,
  CreatePersonalAccessTokenRequest,
  UpdatePersonalAccessTokenRequest,
} from '@/src/services/moda-api'
import { getPersonalAccessTokensClient } from '@/src/services/clients'

export const personalAccessTokensApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getMyPersonalAccessTokens: builder.query<PersonalAccessTokenDto[], void>({
      queryFn: async () => {
        try {
          const data = await getPersonalAccessTokensClient().getMyTokens()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) =>
        result
          ? [
              ...result.map(({ id }) => ({
                type: QueryTags.PersonalAccessTokens as const,
                id,
              })),
              { type: QueryTags.PersonalAccessTokens, id: 'LIST' },
            ]
          : [{ type: QueryTags.PersonalAccessTokens, id: 'LIST' }],
    }),
    createPersonalAccessToken: builder.mutation<
      CreatePersonalAccessTokenResult,
      CreatePersonalAccessTokenRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getPersonalAccessTokensClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: [{ type: QueryTags.PersonalAccessTokens, id: 'LIST' }],
    }),
    updatePersonalAccessToken: builder.mutation<
      void,
      { id: string; request: UpdatePersonalAccessTokenRequest }
    >({
      queryFn: async ({ id, request }) => {
        try {
          const data = await getPersonalAccessTokensClient().update(id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { id }) => [
        { type: QueryTags.PersonalAccessTokens, id },
        { type: QueryTags.PersonalAccessTokens, id: 'LIST' },
      ],
    }),
    revokePersonalAccessToken: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getPersonalAccessTokensClient().revoke(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, id) => [
        { type: QueryTags.PersonalAccessTokens, id },
        { type: QueryTags.PersonalAccessTokens, id: 'LIST' },
      ],
    }),
    deletePersonalAccessToken: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getPersonalAccessTokensClient().delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [
        { type: QueryTags.PersonalAccessTokens, id: 'LIST' },
      ],
    }),
  }),
})

export const {
  useGetMyPersonalAccessTokensQuery,
  useCreatePersonalAccessTokenMutation,
  useUpdatePersonalAccessTokenMutation,
  useRevokePersonalAccessTokenMutation,
  useDeletePersonalAccessTokenMutation,
} = personalAccessTokensApi
