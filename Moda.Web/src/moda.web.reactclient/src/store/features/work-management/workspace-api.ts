import { getWorkspacesClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import { WorkspaceDto, WorkspaceListDto } from '@/src/services/moda-api'

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
          console.log('Data:', data)
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
  }),
})

export const { useGetWorkspacesQuery, useGetWorkspaceQuery } = workspaceApi
