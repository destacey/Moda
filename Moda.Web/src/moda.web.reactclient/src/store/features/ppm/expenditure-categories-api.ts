import { getExpenditureCategoriesClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  CreateExpenditureCategoryRequest,
  ExpenditureCategoryDetailsDto,
  ExpenditureCategoryListDto,
  UpdateExpenditureCategoryRequest,
} from '@/src/services/moda-api'

export const expenditureCategoriesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getExpenditureCategories: builder.query<ExpenditureCategoryListDto[], void>(
      {
        queryFn: async () => {
          try {
            const data = await (
              await getExpenditureCategoriesClient()
            ).getExpenditureCategories()
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        providesTags: () => [
          { type: QueryTags.ExpenditureCategory, id: 'LIST' },
        ],
      },
    ),
    getExpenditureCategory: builder.query<
      ExpenditureCategoryDetailsDto,
      number
    >({
      queryFn: async (id) => {
        try {
          const data = await (
            await getExpenditureCategoriesClient()
          ).getExpenditureCategory(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.ExpenditureCategory, id: arg },
      ],
    }),
    createExpenditureCategory: builder.mutation<
      number,
      CreateExpenditureCategoryRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getExpenditureCategoriesClient()
          ).create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [{ type: QueryTags.ExpenditureCategory, id: 'LIST' }]
      },
    }),
    updateExpenditureCategory: builder.mutation<
      void,
      UpdateExpenditureCategoryRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await (
            await getExpenditureCategoriesClient()
          ).update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.ExpenditureCategory, id: 'LIST' },
          { type: QueryTags.ExpenditureCategory, id: arg.id },
        ]
      },
    }),
    activateExpenditureCategory: builder.mutation<void, number>({
      queryFn: async (id) => {
        try {
          const data = await (
            await getExpenditureCategoriesClient()
          ).activate(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.ExpenditureCategory, id: 'LIST' },
          { type: QueryTags.ExpenditureCategory, id: arg },
        ]
      },
    }),
    archiveExpenditureCategory: builder.mutation<void, number>({
      queryFn: async (id) => {
        try {
          const data = await (
            await getExpenditureCategoriesClient()
          ).archive(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.ExpenditureCategory, id: 'LIST' },
          { type: QueryTags.ExpenditureCategory, id: arg },
        ]
      },
    }),
    deleteExpenditureCategory: builder.mutation<void, number>({
      queryFn: async (id) => {
        try {
          const data = await (await getExpenditureCategoriesClient()).delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [{ type: QueryTags.ExpenditureCategory, id: 'LIST' }]
      },
    }),
  }),
})

export const {
  useGetExpenditureCategoriesQuery,
  useGetExpenditureCategoryQuery,
  useCreateExpenditureCategoryMutation,
  useUpdateExpenditureCategoryMutation,
  useActivateExpenditureCategoryMutation,
  useArchiveExpenditureCategoryMutation,
  useDeleteExpenditureCategoryMutation,
} = expenditureCategoriesApi
