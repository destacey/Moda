import { getProjectsClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import {
  CreateProjectRequest,
  ObjectIdAndKey,
  ProjectListDto,
  ProjectDetailsDto,
  UpdateProjectRequest,
} from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'

export const projectsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getProjects: builder.query<ProjectListDto[], number | undefined>({
      queryFn: async (status = undefined) => {
        try {
          const data = await (await getProjectsClient()).getProjects(status)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.Project, id: 'LIST' }],
    }),
    getProject: builder.query<ProjectDetailsDto, number>({
      queryFn: async (key) => {
        try {
          const data = await (
            await getProjectsClient()
          ).getProject(key.toString())
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.Project, id: arg },
      ],
    }),
    createProject: builder.mutation<ObjectIdAndKey, CreateProjectRequest>({
      queryFn: async (request) => {
        try {
          const data = await (await getProjectsClient()).create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [{ type: QueryTags.Project, id: 'LIST' }]
      },
    }),
    updateProject: builder.mutation<
      void,
      { request: UpdateProjectRequest; cacheKey: number }
    >({
      queryFn: async ({ request, cacheKey }) => {
        try {
          const data = await (
            await getProjectsClient()
          ).update(request.id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.Project, id: 'LIST' },
          { type: QueryTags.Project, id: cacheKey },
        ]
      },
    }),
    activateProject: builder.mutation<void, { id: string; cacheKey: number }>({
      queryFn: async ({ id }) => {
        try {
          const data = await (await getProjectsClient()).activate(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.Project, id: 'LIST' },
          { type: QueryTags.Project, id: cacheKey },
        ]
      },
    }),
    completeProject: builder.mutation<void, { id: string; cacheKey: number }>({
      queryFn: async ({ id }) => {
        try {
          const data = await (await getProjectsClient()).complete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.Project, id: 'LIST' },
          { type: QueryTags.Project, id: cacheKey },
        ]
      },
    }),
    cancelProject: builder.mutation<void, { id: string; cacheKey: number }>({
      queryFn: async ({ id }) => {
        try {
          const data = await (await getProjectsClient()).cancel(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { cacheKey }) => {
        return [
          { type: QueryTags.Project, id: 'LIST' },
          { type: QueryTags.Project, id: cacheKey },
        ]
      },
    }),
    deleteProject: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await (await getProjectsClient()).delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [
          { type: QueryTags.Project, id: 'LIST' },
          { type: QueryTags.PortfolioProjects, id: 'LIST' },
        ]
      },
    }),
  }),
})

export const {
  useGetProjectsQuery,
  useGetProjectQuery,
  useCreateProjectMutation,
  useUpdateProjectMutation,
  useActivateProjectMutation,
  useCompleteProjectMutation,
  useCancelProjectMutation,
  useDeleteProjectMutation,
} = projectsApi
