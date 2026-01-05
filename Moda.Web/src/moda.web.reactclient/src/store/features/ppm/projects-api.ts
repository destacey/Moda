import { getProjectsClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import {
  CreateProjectRequest,
  ObjectIdAndKey,
  ProjectListDto,
  ProjectDetailsDto,
  UpdateProjectRequest,
  WorkItemListDto,
  ChangeProjectProgramRequest,
  ChangeProjectKeyRequest,
} from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'
import { BaseOptionType } from 'antd/es/select'

export const projectsApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getProjects: builder.query<ProjectListDto[], number | undefined>({
      queryFn: async (status = undefined) => {
        try {
          const data = await getProjectsClient().getProjects(status)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.Project, id: 'LIST' }],
    }),

    getProject: builder.query<ProjectDetailsDto, string>({
      queryFn: async (idOrKey) => {
        try {
          const data = await getProjectsClient().getProject(idOrKey)
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
          const data = await getProjectsClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.Project, id: 'LIST' },
          { type: QueryTags.PortfolioProjects, id: 'LIST' },
        ]
      },
    }),

    updateProject: builder.mutation<
      void,
      { request: UpdateProjectRequest; cacheKey: string }
    >({
      queryFn: async ({ request }) => {
        try {
          const data = await getProjectsClient().update(request.id, request)
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
          { type: QueryTags.PortfolioProjects, id: 'LIST' },
        ]
      },
    }),

    changeProjectProgram: builder.mutation<
      void,
      { id: string; request: ChangeProjectProgramRequest; cacheKey: string }
    >({
      queryFn: async ({ id, request }) => {
        try {
          const data = await getProjectsClient().changeProgram(id, request)
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
          { type: QueryTags.PortfolioProjects, id: 'LIST' },
          { type: QueryTags.ProgramProjects, id: 'LIST' },
        ]
      },
    }),

    changeProjectKey: builder.mutation<
      void,
      { id: string; request: ChangeProjectKeyRequest }
    >({
      queryFn: async ({ id, request }) => {
        try {
          const data = await getProjectsClient().changeKey(id, request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { request }) => {
        if (error) return []
        return [
          { type: QueryTags.Project, id: 'LIST' },
          // If any screens already cached the *new* key, ensure it's refreshed.
          { type: QueryTags.Project, id: request.key },
          { type: QueryTags.PortfolioProjects, id: 'LIST' },
          { type: QueryTags.ProgramProjects, id: 'LIST' },
        ]
      },
    }),

    activateProject: builder.mutation<void, { id: string; cacheKey: string }>({
      queryFn: async ({ id }) => {
        try {
          const data = await getProjectsClient().activate(id)
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
          { type: QueryTags.PortfolioProjects, id: 'LIST' },
        ]
      },
    }),

    completeProject: builder.mutation<void, { id: string; cacheKey: string }>({
      queryFn: async ({ id }) => {
        try {
          const data = await getProjectsClient().complete(id)
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
          { type: QueryTags.PortfolioProjects, id: 'LIST' },
        ]
      },
    }),

    cancelProject: builder.mutation<void, { id: string; cacheKey: string }>({
      queryFn: async ({ id }) => {
        try {
          const data = await getProjectsClient().cancel(id)
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
          { type: QueryTags.PortfolioProjects, id: 'LIST' },
        ]
      },
    }),

    deleteProject: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getProjectsClient().delete(id)
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

    getProjectWorkItems: builder.query<WorkItemListDto[], string>({
      queryFn: async (id) => {
        try {
          const data = await getProjectsClient().getProjectWorkItems(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result) => [
        QueryTags.WorkItem,
        ...result.map(({ key }) => ({ type: QueryTags.ProjectWorkItems, key })),
      ],
    }),

    getProjectOptions: builder.query<BaseOptionType[], void>({
      queryFn: async () => {
        try {
          const portfolios = await getProjectsClient().getProjects(null)

          const data: BaseOptionType[] = portfolios
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((category) => ({
              label: `${category.name} (${category.key})`,
              value: category.id,
            }))

          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
    }),
  }),
})

export const {
  useGetProjectsQuery,
  useGetProjectQuery,
  useCreateProjectMutation,
  useUpdateProjectMutation,
  useChangeProjectProgramMutation,
  useChangeProjectKeyMutation,
  useActivateProjectMutation,
  useCompleteProjectMutation,
  useCancelProjectMutation,
  useDeleteProjectMutation,
  useGetProjectWorkItemsQuery,
  useGetProjectOptionsQuery,
} = projectsApi
