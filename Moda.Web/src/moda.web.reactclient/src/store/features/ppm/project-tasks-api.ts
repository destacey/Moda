import { getProjectTasksClient } from '@/src/services/clients'
import { apiSlice } from '../apiSlice'
import {
  ProjectTaskListDto,
  ProjectTaskDto,
  ProjectTaskTreeDto,
  CreateProjectTaskRequest,
  UpdateProjectTaskRequest,
  ObjectIdAndTaskKey,
  UpdateProjectTaskOrderRequest,
  AddTaskDependencyRequest,
} from '@/src/services/moda-api'
import { QueryTags } from '../query-tags'
import { OptionModel } from '@/src/components/types'

interface GetProjectTasksParams {
  projectIdOrKey: string
  status?: number
  parentId?: string
}

export const projectTasksApi = apiSlice.injectEndpoints({
  endpoints: (builder) => ({
    getProjectTasks: builder.query<ProjectTaskListDto[], GetProjectTasksParams>(
      {
        queryFn: async ({ projectIdOrKey, status, parentId }) => {
          try {
            const data = await getProjectTasksClient().getProjectTasks(
              projectIdOrKey,
              status,
              parentId,
            )
            return { data }
          } catch (error) {
            console.error('API Error:', error)
            return { error }
          }
        },
        providesTags: (result, error, { projectIdOrKey }) => [
          { type: QueryTags.ProjectTask, id: `LIST-${projectIdOrKey}` },
        ],
      },
    ),
    getProjectTaskTree: builder.query<
      ProjectTaskTreeDto[],
      { projectIdOrKey: string }
    >({
      queryFn: async ({ projectIdOrKey }) => {
        try {
          const data =
            await getProjectTasksClient().getProjectTaskTree(projectIdOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, { projectIdOrKey }) => [
        { type: QueryTags.ProjectTaskTree, id: `TREE-${projectIdOrKey}` },
      ],
    }),
    getProjectTask: builder.query<
      ProjectTaskDto,
      { projectIdOrKey: string; idOrTaskKey: string }
    >({
      queryFn: async ({ projectIdOrKey, idOrTaskKey }) => {
        try {
          const data = await getProjectTasksClient().getProjectTask(
            projectIdOrKey,
            idOrTaskKey,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, { idOrTaskKey }) => [
        { type: QueryTags.ProjectTask, id: idOrTaskKey },
      ],
    }),
    createProjectTask: builder.mutation<
      ObjectIdAndTaskKey,
      { projectIdOrKey: string; request: CreateProjectTaskRequest }
    >({
      queryFn: async ({ projectIdOrKey, request }) => {
        try {
          const data = await getProjectTasksClient().createProjectTask(
            projectIdOrKey,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { projectIdOrKey }) => [
        { type: QueryTags.ProjectTask, id: `LIST-${projectIdOrKey}` },
        { type: QueryTags.ProjectTaskTree, id: `TREE-${projectIdOrKey}` },
      ],
    }),
    updateProjectTask: builder.mutation<
      void,
      {
        projectIdOrKey: string
        request: UpdateProjectTaskRequest
        cacheKey: string
      }
    >({
      queryFn: async ({ projectIdOrKey, request }) => {
        try {
          const data = await getProjectTasksClient().updateProjectTask(
            projectIdOrKey,
            request.id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { projectIdOrKey, cacheKey }) => [
        { type: QueryTags.ProjectTask, id: `LIST-${projectIdOrKey}` },
        { type: QueryTags.ProjectTask, id: cacheKey },
        { type: QueryTags.ProjectTaskTree, id: `TREE-${projectIdOrKey}` },
      ],
    }),
    deleteProjectTask: builder.mutation<
      void,
      { projectIdOrKey: string; id: string }
    >({
      queryFn: async ({ projectIdOrKey, id }) => {
        try {
          const data = await getProjectTasksClient().deleteProjectTask(
            projectIdOrKey,
            id,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { projectIdOrKey }) => [
        { type: QueryTags.ProjectTask, id: `LIST-${projectIdOrKey}` },
        { type: QueryTags.ProjectTaskTree, id: `TREE-${projectIdOrKey}` },
      ],
    }),
    updateProjectTaskOrder: builder.mutation<
      void,
      {
        projectIdOrKey: string
        id: string
        request: UpdateProjectTaskOrderRequest
      }
    >({
      queryFn: async ({ projectIdOrKey, id, request }) => {
        try {
          const data = await getProjectTasksClient().updateProjectTaskOrder(
            projectIdOrKey,
            id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { projectIdOrKey }) => [
        { type: QueryTags.ProjectTask, id: `LIST-${projectIdOrKey}` },
        { type: QueryTags.ProjectTaskTree, id: `TREE-${projectIdOrKey}` },
      ],
    }),
    getCriticalPath: builder.query<string[], { projectIdOrKey: string }>({
      queryFn: async ({ projectIdOrKey }) => {
        try {
          const data =
            await getProjectTasksClient().getCriticalPath(projectIdOrKey)
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, { projectIdOrKey }) => [
        { type: QueryTags.ProjectCriticalPath, id: projectIdOrKey },
      ],
    }),
    addTaskDependency: builder.mutation<
      void,
      {
        projectIdOrKey: string
        id: string
        request: AddTaskDependencyRequest
      }
    >({
      queryFn: async ({ projectIdOrKey, id, request }) => {
        try {
          const data = await getProjectTasksClient().addTaskDependency(
            projectIdOrKey,
            id,
            request,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { projectIdOrKey, request }) => [
        { type: QueryTags.ProjectTask, id: `LIST-${projectIdOrKey}` },
        { type: QueryTags.ProjectTask, id: request.predecessorId },
        { type: QueryTags.ProjectTask, id: request.successorId },
        { type: QueryTags.ProjectTaskDependency, id: projectIdOrKey },
        { type: QueryTags.ProjectCriticalPath, id: projectIdOrKey },
      ],
    }),
    removeTaskDependency: builder.mutation<
      void,
      {
        projectIdOrKey: string
        id: string
        successorId: string
      }
    >({
      queryFn: async ({ projectIdOrKey, id, successorId }) => {
        try {
          const data = await getProjectTasksClient().removeTaskDependency(
            projectIdOrKey,
            id,
            successorId,
          )
          return { data }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      invalidatesTags: (result, error, { projectIdOrKey, id, successorId }) => [
        { type: QueryTags.ProjectTask, id: `LIST-${projectIdOrKey}` },
        { type: QueryTags.ProjectTask, id: id },
        { type: QueryTags.ProjectTask, id: successorId },
        { type: QueryTags.ProjectTaskDependency, id: projectIdOrKey },
        { type: QueryTags.ProjectCriticalPath, id: projectIdOrKey },
      ],
    }),
    getTaskStatusOptions: builder.query<
      OptionModel<number>[],
      { projectIdOrKey: string }
    >({
      queryFn: async ({ projectIdOrKey }) => {
        try {
          const data =
            await getProjectTasksClient().getTaskStatuses(projectIdOrKey)
          return {
            data: data
              .sort((a, b) => a.order - b.order)
              .map((s) => ({
                value: s.id,
                label: s.name,
              })),
          }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.TaskStatusOptions, id: 'LIST' }],
    }),
    getTaskPriorityOptions: builder.query<
      OptionModel<number>[],
      { projectIdOrKey: string }
    >({
      queryFn: async ({ projectIdOrKey }) => {
        try {
          const data =
            await getProjectTasksClient().getTaskPriorities(projectIdOrKey)
          return {
            data: data
              .sort((a, b) => a.order - b.order)
              .map((p) => ({
                value: p.id,
                label: p.name,
              })),
          }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.TaskPriorityOptions, id: 'LIST' }],
    }),
    getTaskTypeOptions: builder.query<
      OptionModel<number>[],
      { projectIdOrKey: string }
    >({
      queryFn: async ({ projectIdOrKey }) => {
        try {
          const data =
            await getProjectTasksClient().getProjectTaskTypes(projectIdOrKey)
          return {
            data: data
              .sort((a, b) => a.order - b.order)
              .map((t) => ({
                value: t.id,
                label: t.name,
              })),
          }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: () => [{ type: QueryTags.TaskTypeOptions, id: 'LIST' }],
    }),
    getParentTaskOptions: builder.query<
      Array<{
        value: string
        title: string
        children?: Array<{
          value: string
          title: string
          children?: any[]
        }>
      }>,
      { projectIdOrKey: string; excludeTaskId?: string }
    >({
      queryFn: async ({ projectIdOrKey, excludeTaskId }) => {
        try {
          const treeData =
            await getProjectTasksClient().getProjectTaskTree(projectIdOrKey)

          // Recursively convert tree to TreeSelect format, excluding a specific task
          const convertToTreeSelect = (
            tasks: ProjectTaskTreeDto[],
          ): Array<{
            value: string
            title: string
            children?: any[]
          }> => {
            return tasks
              .filter(
                (t) => t.id !== excludeTaskId && t.type.name !== 'Milestone',
              )
              .map((t) => ({
                value: t.id,
                title: `${t.taskKey} - ${t.name}`,
                children:
                  t.children && t.children.length > 0
                    ? convertToTreeSelect(t.children)
                    : undefined,
              }))
          }

          return {
            data: convertToTreeSelect(treeData),
          }
        } catch (error) {
          console.error('API Error:', error)
          return { error }
        }
      },
      providesTags: (result, error, { projectIdOrKey }) => [
        { type: QueryTags.ProjectTaskTree, id: `TREE-${projectIdOrKey}` },
      ],
    }),
  }),
})

export const {
  useGetProjectTasksQuery,
  useGetProjectTaskTreeQuery,
  useGetProjectTaskQuery,
  useCreateProjectTaskMutation,
  useUpdateProjectTaskMutation,
  useDeleteProjectTaskMutation,
  useUpdateProjectTaskOrderMutation,
  useGetCriticalPathQuery,
  useAddTaskDependencyMutation,
  useRemoveTaskDependencyMutation,
  useGetTaskStatusOptionsQuery,
  useGetTaskPriorityOptionsQuery,
  useGetTaskTypeOptionsQuery,
  useGetParentTaskOptionsQuery,
} = projectTasksApi
