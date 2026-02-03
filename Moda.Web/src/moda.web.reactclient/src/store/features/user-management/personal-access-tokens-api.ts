import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  PersonalAccessTokensClient,
  PersonalAccessTokenDto,
  CreatePersonalAccessTokenResult,
  CreatePersonalAccessTokenRequest,
  UpdatePersonalAccessTokenRequest,
} from '@/src/services/moda-api'
import axios from 'axios'
import { tokenRequest } from '@/auth-config'
import { InteractionRequiredAuthError } from '@azure/msal-browser'
import { msalInstance } from '@/src/components/contexts/auth'

const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL

const axiosClient = axios.create({
  baseURL: apiUrl,
  transformResponse: (data) => data,
})

// Use the shared MSAL instance to acquire tokens for outgoing requests.
axiosClient.interceptors.request.use(
  async (config) => {
    let token: string | null = null
    try {
      // MSAL v5 requires account parameter for silent token acquisition
      const accounts = msalInstance.getAllAccounts()
      if (accounts.length > 0) {
        const response = await msalInstance.acquireTokenSilent({
          ...tokenRequest,
          account: accounts[0],
        })
        token = response.accessToken
      }
    } catch (error: any) {
      if (error instanceof InteractionRequiredAuthError) {
        try {
          const response = await msalInstance.acquireTokenPopup(tokenRequest)
          token = response.accessToken
        } catch (popupError) {
          console.error('Token popup failed:', popupError)
        }
      } else {
        console.error('Token acquisition error:', error)
      }
    }

    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    // If no token, let request proceed - API will return 401 which is handled gracefully
    // This handles edge cases like MSAL transitional states (block_iframe_reload, timed_out)
    return config
  },
  (error) => Promise.reject(error),
)

const getPersonalAccessTokensClient = () =>
  new PersonalAccessTokensClient('', axiosClient)

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
