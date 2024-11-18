import { getLinksClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  CreateLinkRequest,
  LinkDto,
  UpdateLinkRequest,
} from '@/src/services/moda-api'

export interface StoreUpdateLinkRequest {
  request: UpdateLinkRequest
  objectId: string
}

export interface StoreDeleteLinkRequest {
  id: string
  objectId: string
}

export const linksApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getLinks: builder.query<any, string>({
      queryFn: async (objectId: string) => {
        try {
          const data = await (await getLinksClient()).getList(objectId)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result = [], error, objectId) => [
        { type: QueryTags.Links, id: objectId },
        ...(result?.map(({ id }: { id: string }) => ({
          type: QueryTags.Links,
          id,
        })) ?? []),
      ],
    }),

    getLink: builder.query<any, string>({
      queryFn: async (id: string) => {
        try {
          const data = await (await getLinksClient()).getById(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, id) => [{ type: QueryTags.Links, id }],
    }),

    createLink: builder.mutation<any, CreateLinkRequest>({
      queryFn: async (request) => {
        try {
          const data = await (await getLinksClient()).create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { objectId }) => [
        { type: QueryTags.Links, id: result?.id },
        { type: QueryTags.Links, id: objectId },
      ],
    }),

    updateLink: builder.mutation<LinkDto, StoreUpdateLinkRequest>({
      queryFn: async ({ request }) => {
        try {
          const data = await (
            await getLinksClient()
          ).update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, request) => [
        { type: QueryTags.Links, id: request.request.id },
        { type: QueryTags.Links, id: request.objectId },
      ],
    }),

    deleteLink: builder.mutation<void, StoreDeleteLinkRequest>({
      queryFn: async ({ id }) => {
        try {
          const data = await (await getLinksClient()).delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { id, objectId }) => [
        { type: QueryTags.Links, id },
        { type: QueryTags.Links, id: objectId },
      ],
    }),
  }),
})

// Export hooks
export const {
  useGetLinksQuery,
  useGetLinkQuery,
  useCreateLinkMutation,
  useUpdateLinkMutation,
  useDeleteLinkMutation,
} = linksApi
