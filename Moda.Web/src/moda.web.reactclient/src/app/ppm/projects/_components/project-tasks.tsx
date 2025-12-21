'use client'

import { ProjectDetailsDto } from '@/src/services/moda-api'
import { FC, useCallback, useState } from 'react'
import {
  CreateProjectTaskForm,
  DeleteProjectTaskForm,
  EditProjectTaskForm,
  ProjectTasksTable,
} from '.'
import {
  useGetProjectTaskTreeQuery,
  useGetTaskPriorityOptionsQuery,
  useGetTaskStatusOptionsQuery,
  useGetTaskTypeOptionsQuery,
  useUpdateProjectTaskMutation,
} from '@/src/store/features/ppm/project-tasks-api'

export interface ProjectTasksProps {
  project: ProjectDetailsDto
  canManageTasks: boolean
}

const ProjectTasks: FC<ProjectTasksProps> = ({
  project,
  canManageTasks,
}: ProjectTasksProps) => {
  const [openCreateTaskForm, setOpenCreateTaskForm] = useState<boolean>(false)
  const [openEditTaskForm, setOpenEditTaskForm] = useState<boolean>(false)
  const [openDeleteTaskForm, setOpenDeleteTaskForm] = useState<boolean>(false)
  const [selectedTaskId, setSelectedTaskId] = useState<string | undefined>(
    undefined,
  )

  const {
    data: tasksData,
    isLoading: tasksDataIsLoading,
    refetch: refetchTasksData,
  } = useGetProjectTaskTreeQuery(
    { projectIdOrKey: project?.key || '' },
    { skip: !project?.key },
  )
  const { data: taskStatusOptions = [] } = useGetTaskStatusOptionsQuery(
    { projectIdOrKey: project?.key || '' },
    { skip: !project?.key },
  )

  const { data: taskPriorityOptions = [] } = useGetTaskPriorityOptionsQuery(
    { projectIdOrKey: project?.key || '' },
    { skip: !project?.key },
  )

  const { data: taskTypeOptions = [] } = useGetTaskTypeOptionsQuery(
    { projectIdOrKey: project?.key || '' },
    { skip: !project?.key },
  )

  const [updateProjectTask] = useUpdateProjectTaskMutation()

  // Task form handlers
  const handleCreateTask = useCallback(() => {
    setOpenCreateTaskForm(true)
  }, [])

  const handleEditTask = useCallback((task: any) => {
    setSelectedTaskId(task.id)
    setOpenEditTaskForm(true)
  }, [])

  const handleDeleteTask = useCallback((task: any) => {
    setSelectedTaskId(task.id)
    setOpenDeleteTaskForm(true)
  }, [])

  const handleUpdateTaskField = useCallback(
    // eslint-disable-next-line react-hooks/preserve-manual-memoization
    async (taskId: string, updates: Partial<any>) => {
      if (!project?.key) return

      // Find the task in the tree
      const findTask = (tasks: any[], id: string): any => {
        for (const task of tasks) {
          if (task.id === id) return task
          if (task.children?.length) {
            const found = findTask(task.children, id)
            if (found) return found
          }
        }
        return null
      }

      const task = findTask(tasksData || [], taskId)
      if (!task) return

      try {
        await updateProjectTask({
          projectIdOrKey: project?.key,
          cacheKey: taskId,
          request: {
            id: task.id,
            name: updates.name ?? task.name,
            description: updates.description ?? task.description,
            statusId: updates.statusId ?? task.status?.id,
            priorityId: updates.priorityId ?? task.priority?.id,
            parentId: task.parentId,
            teamId: task.team?.id,
            plannedStart:
              updates.plannedStart !== undefined
                ? updates.plannedStart
                : task.plannedStart,
            plannedEnd:
              updates.plannedEnd !== undefined
                ? updates.plannedEnd
                : task.plannedEnd,
            plannedDate: updates.plannedDate ?? task.plannedDate,
            actualDate: task.actualDate,
            estimatedEffortHours:
              updates.estimatedEffortHours !== undefined
                ? updates.estimatedEffortHours
                : task.estimatedEffortHours,
            assignments: [],
          },
        }).unwrap()
      } catch (error) {
        console.error('Failed to update task:', error)
      }
    },
    [project?.key, tasksData, updateProjectTask],
  )

  const onCreateTaskFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCreateTaskForm(false)
      if (wasSaved) {
        refetchTasksData()
      }
    },
    [refetchTasksData],
  )

  const onEditTaskFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditTaskForm(false)
      setSelectedTaskId(undefined)
      if (wasSaved) {
        refetchTasksData()
      }
    },
    [refetchTasksData],
  )

  const onDeleteTaskFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteTaskForm(false)
      setSelectedTaskId(undefined)
      if (wasDeleted) {
        refetchTasksData()
      }
    },
    [refetchTasksData],
  )

  if (!project) return null

  return (
    <>
      <ProjectTasksTable
        tasks={tasksData}
        isLoading={tasksDataIsLoading}
        onCreateTask={handleCreateTask}
        onEditTask={handleEditTask}
        onDeleteTask={handleDeleteTask}
        onRefresh={refetchTasksData}
        onUpdateTask={handleUpdateTaskField}
        taskStatusOptions={taskStatusOptions}
        taskPriorityOptions={taskPriorityOptions}
        taskTypeOptions={taskTypeOptions}
      />

      {openCreateTaskForm && (
        <CreateProjectTaskForm
          projectIdOrKey={project?.key}
          showForm={openCreateTaskForm}
          onFormComplete={() => onCreateTaskFormClosed(true)}
          onFormCancel={() => onCreateTaskFormClosed(false)}
        />
      )}
      {openEditTaskForm && selectedTaskId && (
        <EditProjectTaskForm
          projectIdOrKey={project?.key}
          taskIdOrKey={selectedTaskId}
          showForm={openEditTaskForm}
          onFormComplete={() => onEditTaskFormClosed(true)}
          onFormCancel={() => onEditTaskFormClosed(false)}
        />
      )}
      {openDeleteTaskForm && selectedTaskId && (
        <DeleteProjectTaskForm
          projectIdOrKey={project?.key}
          taskIdOrKey={selectedTaskId}
          showForm={openDeleteTaskForm}
          onFormComplete={() => onDeleteTaskFormClosed(true)}
          onFormCancel={() => onDeleteTaskFormClosed(false)}
        />
      )}
    </>
  )
}

export default ProjectTasks
