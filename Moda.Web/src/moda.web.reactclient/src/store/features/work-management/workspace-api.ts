import { getWorkspacesClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  WorkItemDetailsDto,
  WorkItemListDto,
  WorkspaceDto,
  WorkspaceListDto,
} from '@/src/services/moda-api'

export interface GetWorkItemRequest {
  idOrKey: string
  workItemKey: string
}

export const workspaceApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getWorkspaces: builder.query<WorkspaceListDto[], boolean>({
      queryFn: async (includeInactive) => {
        try {
          const data = await (
            await getWorkspacesClient()
          ).getList(includeInactive)
          return { data }
        } catch (error) {
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.Workspace,
        ...result.map(({ key }) => ({ type: QueryTags.Workspace, key })),
      ],
    }),
    getWorkspace: builder.query<WorkspaceDto, string>({
      queryFn: async (idOrKey: string) => {
        try {
          const data = await (await getWorkspacesClient()).get(idOrKey)
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.Workspace, id: arg }, // typically arg is the key
      ],
    }),
    getWorkItems: builder.query<WorkItemListDto[], string>({
      queryFn: async (idOrKey: string) => {
        try {
          const data = await (await getWorkspacesClient()).getWorkItems(idOrKey)
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.WorkItem,
        ...result.map(({ key }) => ({ type: QueryTags.WorkItem, key })),
      ],
    }),
    getWorkItem: builder.query<WorkItemDetailsDto, GetWorkItemRequest>({
      queryFn: async (request: GetWorkItemRequest) => {
        try {
          const data = await (
            await getWorkspacesClient()
          ).getWorkItem(request.idOrKey, request.workItemKey)
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.WorkItem, id: arg.workItemKey }, // typically arg is the key
      ],
    }),
    searchWorkItems: builder.query<WorkItemListDto[], string>({
      queryFn: async (searchTerm: string) => {
        try {
          const data = await (
            await getWorkspacesClient()
          ).searchWorkItems(searchTerm, 50)
          return { data }
        } catch (error) {
          console.error('Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        QueryTags.WorkItemSearch,
        ...result.map(({ key }) => ({ type: QueryTags.WorkItemSearch, key })),
      ],
    }),
  }),
})

export const {
  useGetWorkspacesQuery,
  useGetWorkspaceQuery,
  useGetWorkItemsQuery,
  useGetWorkItemQuery,
  useSearchWorkItemsQuery,
} = workspaceApi
