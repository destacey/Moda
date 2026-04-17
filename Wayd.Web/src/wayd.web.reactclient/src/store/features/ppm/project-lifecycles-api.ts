import { getProjectLifecyclesClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import { QueryTags } from '../query-tags'
import {
  CreateProjectLifecycleRequest,
  ProjectLifecycleDetailsDto,
  ProjectLifecycleListDto,
  ProjectLifecyclePhaseRequest,
  ProjectLifecycleState,
  UpdateProjectLifecycleRequest,
} from '@/src/services/wayd-api'

export const projectLifecyclesApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getProjectLifecycles: builder.query<
      ProjectLifecycleListDto[],
      ProjectLifecycleState | null | undefined
    >({
      queryFn: async (state) => {
        try {
          const data =
            await getProjectLifecyclesClient().getProjectLifecycles(state)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [
        { type: QueryTags.ProjectLifecycle },
      ],
    }),
    getProjectLifecycle: builder.query<ProjectLifecycleDetailsDto, string>({
      queryFn: async (idOrKey) => {
        try {
          const data =
            await getProjectLifecyclesClient().getProjectLifecycle(idOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, arg) => [
        { type: QueryTags.ProjectLifecycle, id: arg },
      ],
    }),
    createProjectLifecycle: builder.mutation<
      string,
      CreateProjectLifecycleRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getProjectLifecyclesClient().create(request)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [{ type: QueryTags.ProjectLifecycle, id: 'LIST' }]
      },
    }),
    updateProjectLifecycle: builder.mutation<
      void,
      { id: string } & UpdateProjectLifecycleRequest
    >({
      queryFn: async (request) => {
        try {
          const data = await getProjectLifecyclesClient().update(
            request.id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.ProjectLifecycle, id: 'LIST' },
          { type: QueryTags.ProjectLifecycle, id: arg.id },
        ]
      },
    }),
    deleteProjectLifecycle: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getProjectLifecyclesClient().delete(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => {
        return [{ type: QueryTags.ProjectLifecycle, id: 'LIST' }]
      },
    }),
    activateProjectLifecycle: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getProjectLifecyclesClient().activate(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.ProjectLifecycle, id: 'LIST' },
          { type: QueryTags.ProjectLifecycle, id: arg },
        ]
      },
    }),
    archiveProjectLifecycle: builder.mutation<void, string>({
      queryFn: async (id) => {
        try {
          const data = await getProjectLifecyclesClient().archive(id)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, arg) => {
        return [
          { type: QueryTags.ProjectLifecycle, id: 'LIST' },
          { type: QueryTags.ProjectLifecycle, id: arg },
        ]
      },
    }),
    addProjectLifecyclePhase: builder.mutation<
      string,
      { lifecycleId: string } & ProjectLifecyclePhaseRequest
    >({
      queryFn: async ({ lifecycleId, ...request }) => {
        try {
          const data = await getProjectLifecyclesClient().addPhase(
            lifecycleId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [
        { type: QueryTags.ProjectLifecycle },
      ],
    }),
    updateProjectLifecyclePhase: builder.mutation<
      void,
      { lifecycleId: string; phaseId: string } & ProjectLifecyclePhaseRequest
    >({
      queryFn: async ({ lifecycleId, phaseId, ...request }) => {
        try {
          const data = await getProjectLifecyclesClient().updatePhase(
            lifecycleId,
            phaseId,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [{ type: QueryTags.ProjectLifecycle }],
    }),
    removeProjectLifecyclePhase: builder.mutation<
      void,
      { lifecycleId: string; phaseId: string }
    >({
      queryFn: async ({ lifecycleId, phaseId }) => {
        try {
          const data = await getProjectLifecyclesClient().removePhase(
            lifecycleId,
            phaseId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [
        { type: QueryTags.ProjectLifecycle },
      ],
    }),
    reorderProjectLifecyclePhases: builder.mutation<
      void,
      { lifecycleId: string; orderedPhaseIds: string[] }
    >({
      queryFn: async ({ lifecycleId, orderedPhaseIds }) => {
        try {
          const data = await getProjectLifecyclesClient().reorderPhases(
            lifecycleId,
            { orderedPhaseIds },
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: () => [
        { type: QueryTags.ProjectLifecycle },
      ],
    }),
  }),
})

export const {
  useGetProjectLifecyclesQuery,
  useGetProjectLifecycleQuery,
  useCreateProjectLifecycleMutation,
  useUpdateProjectLifecycleMutation,
  useDeleteProjectLifecycleMutation,
  useActivateProjectLifecycleMutation,
  useArchiveProjectLifecycleMutation,
  useAddProjectLifecyclePhaseMutation,
  useUpdateProjectLifecyclePhaseMutation,
  useRemoveProjectLifecyclePhaseMutation,
  useReorderProjectLifecyclePhasesMutation,
} = projectLifecyclesApi
