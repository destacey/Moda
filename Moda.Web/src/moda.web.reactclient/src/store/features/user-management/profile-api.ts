import { getProfileClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'

export const profileApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getInternalEmployeeId: builder.query<string, void>({
      queryFn: async () => {
        try {
          const data = await getProfileClient().getInternalEmployeeId()
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
  }),
})

export const { useGetInternalEmployeeIdQuery } = profileApi
