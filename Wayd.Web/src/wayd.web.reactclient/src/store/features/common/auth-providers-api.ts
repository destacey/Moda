import { getAuthClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { AuthProvidersResponse } from '@/src/services/wayd-api'

/**
 * Advertises which authentication providers are enabled on this deployment.
 * Used by the login page to render the right set of login options — e.g., a
 * local-only deployment hides the Microsoft login tab.
 *
 * Cheap + anonymous on the backend. Cached indefinitely here because provider
 * availability doesn't change without a redeploy.
 */
export const authProvidersApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getAuthProviders: builder.query<AuthProvidersResponse, void>({
      queryFn: async () => {
        try {
          const data = await getAuthClient().getProviders()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
  }),
})

export const { useGetAuthProvidersQuery } = authProvidersApi
