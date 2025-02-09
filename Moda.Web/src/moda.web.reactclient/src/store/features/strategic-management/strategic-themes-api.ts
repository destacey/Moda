import { get } from 'lodash'
import { apiSlice } from '../apiSlice'
import {
  CreateStrategicThemeRequest,
  ObjectIdAndKey,
  StrategicThemeListDto,
  UpdateStrategicThemeRequest,
} from '@/src/services/moda-api'
import { getStrategicThemesClient } from '@/src/services/clients'
import { QueryTags } from '../query-tags'
import { OptionModel } from '@/src/components/types'

export const strategicThemesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getStrategicThemes: builder.query<
      StrategicThemeListDto[],
      number | undefined
    >({
      queryFn: async (strategicThemeState = undefined) => {
        try {
          const data = await (
            await getStrategicThemesClient()
          ).getStrategicThemes(strategicThemeState)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.StrategicTheme, id: 'LIST' }],
    }),
    getStrategicTheme: builder.query<StrategicThemeListDto, string>({
      queryFn: async (idOrKey) => {
        try {
          const data = await (
            await getStrategicThemesClient()
          ).getStrategicTheme(idOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        { type: QueryTags.StrategicTheme, id: result?.id },
      ],
    }),
    createStrategicTheme: builder.mutation<
      ObjectIdAndKey,
      CreateStrategicThemeRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (await getStrategicThemesClient()).create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.StrategicTheme, id: 'LIST' }]
      },
    }),
    updateStrategicTheme: builder.mutation<
      void,
      { request: UpdateStrategicThemeRequest; cacheKey: number }
    >({
      queryFn: async ({ request, cacheKey }) => {
        try {
          const data = await (
            await getStrategicThemesClient()
          ).update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicTheme, id: 'LIST' },
          { type: QueryTags.StrategicTheme, id: cacheKey },
        ]
      },
    }),
    deleteStrategicTheme: builder.mutation<
      void,
      { strategicThemeId: string; cacheKey: number }
    >({
      queryFn: async ({ strategicThemeId }) => {
        try {
          const data = await (
            await getStrategicThemesClient()
          ).delete(strategicThemeId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.StrategicTheme, id: 'LIST' },
          { type: QueryTags.StrategicTheme, id: cacheKey },
        ]
      },
    }),
    getStateOptions: builder.query<OptionModel<number>[], void>({
      queryFn: async () => {
        try {
          const states = await (
            await getStrategicThemesClient()
          ).getStateOptions()

          const data: OptionModel<number>[] = states
            .sort((a, b) => a.order - b.order)
            .map((s) => ({
              value: s.id,
              label: s.name,
            }))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [QueryTags.StrategicThemState],
    }),
  }),
})

export const {
  useGetStrategicThemesQuery,
  useGetStrategicThemeQuery,
  useCreateStrategicThemeMutation,
  useUpdateStrategicThemeMutation,
  useDeleteStrategicThemeMutation,
  useGetStateOptionsQuery,
} = strategicThemesApi
