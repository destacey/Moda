'use client'

import {
  LifecycleStatusTag,
  PageActions,
  PageTitle,
} from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import {
  useGetProjectQuery,
  useGetProjectWorkItemsQuery,
} from '@/src/store/features/ppm/projects-api'
import {
  useGetProjectTaskTreeQuery,
  useUpdateProjectTaskMutation,
} from '@/src/store/features/ppm/project-tasks-api'
import { Alert, Card, MenuProps, theme } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import ProjectDetailsLoading from './loading'
import {
  ChangeProjectStatusForm,
  ChangeProjectProgramForm,
  DeleteProjectForm,
  EditProjectForm,
  ProjectDetails,
  ProjectTasksTable,
  CreateProjectTaskForm,
  EditProjectTaskForm,
  DeleteProjectTaskForm,
} from '../_components'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import { ProjectStatusAction } from '../_components/change-project-status-form'
import { WorkItemsGrid } from '@/src/components/common/work'

enum ProjectTabs {
  Details = 'details',
  Tasks = 'tasks',
  WorkItems = 'workItems',
}

const tabs = [
  {
    key: ProjectTabs.Details,
    label: 'Details',
  },
  {
    key: ProjectTabs.Tasks,
    label: 'Tasks',
  },
  {
    key: ProjectTabs.WorkItems,
    label: 'Work Items',
  },
]

enum ProjectAction {
  Edit = 'Edit',
  ChangeProgram = 'Change Program',
  Delete = 'Delete',
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const ProjectDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key: projectKey } = use(props.params)

  useDocumentTitle('Project Details')

  const [activeTab, setActiveTab] = useState(ProjectTabs.Details)
  const [openEditProjectForm, setOpenEditProjectForm] = useState<boolean>(false)
  const [openChangeProgramForm, setOpenChangeProgramForm] =
    useState<boolean>(false)
  const [openActivateProjectForm, setOpenActivateProjectForm] =
    useState<boolean>(false)
  const [openCompleteProjectForm, setOpenCompleteProjectForm] =
    useState<boolean>(false)
  const [openCancelProjectForm, setOpenCancelProjectForm] =
    useState<boolean>(false)
  const [openDeleteProjectForm, setOpenDeleteProjectForm] =
    useState<boolean>(false)

  // Task form state
  const [openCreateTaskForm, setOpenCreateTaskForm] = useState<boolean>(false)
  const [openEditTaskForm, setOpenEditTaskForm] = useState<boolean>(false)
  const [openDeleteTaskForm, setOpenDeleteTaskForm] = useState<boolean>(false)
  const [selectedTaskId, setSelectedTaskId] = useState<string | undefined>(
    undefined,
  )

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdateProject = hasPermissionClaim('Permissions.Projects.Update')
  const canDeleteProject = hasPermissionClaim('Permissions.Projects.Delete')

  const {
    data: projectData,
    isLoading,
    error,
    refetch: refetchProject,
  } = useGetProjectQuery(projectKey)

  const {
    data: workItemsData,
    isLoading: workItemsDataIsLoading,
    error: workItemsDataError,
    refetch: refetchWorkItemsData,
  } = useGetProjectWorkItemsQuery(projectData?.id, { skip: !projectData?.id })

  const {
    data: tasksData,
    isLoading: tasksDataIsLoading,
    refetch: refetchTasksData,
  } = useGetProjectTaskTreeQuery(
    { projectIdOrKey: projectData?.key || '' },
    { skip: !projectData?.key },
  )

  const [updateProjectTask] = useUpdateProjectTaskMutation()

  useEffect(() => {
    if (!projectData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'PPM',
      },
      {
        href: `/ppm/projects`,
        title: 'Projects',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, projectData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

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
    async (taskId: string, updates: Partial<any>) => {
      if (!projectData?.key) return

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
          projectIdOrKey: projectData.key,
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
            actualStart: task.actualStart,
            actualEnd: task.actualEnd,
            actualDate: task.actualDate,
            estimatedEffortHours: task.estimatedEffortHours,
            actualEffortHours: task.actualEffortHours,
            assignments: [],
          },
        }).unwrap()
      } catch (error) {
        console.error('Failed to update task:', error)
      }
    },
    [projectData?.key, tasksData, updateProjectTask],
  )

  const handleUpdateStatus = useCallback(
    async (taskId: string, statusId: number) => {
      await handleUpdateTaskField(taskId, { statusId })
    },
    [handleUpdateTaskField],
  )

  const handleUpdatePriority = useCallback(
    async (taskId: string, priorityId: number) => {
      await handleUpdateTaskField(taskId, { priorityId })
    },
    [handleUpdateTaskField],
  )

  const handleUpdateName = useCallback(
    async (taskId: string, name: string) => {
      await handleUpdateTaskField(taskId, { name })
    },
    [handleUpdateTaskField],
  )

  const handleUpdateType = useCallback(
    async (taskId: string, typeId: number) => {
      await handleUpdateTaskField(taskId, { typeId })
    },
    [handleUpdateTaskField],
  )

  const handleUpdatePlannedStart = useCallback(
    async (taskId: string, date: string | null) => {
      await handleUpdateTaskField(taskId, { plannedStart: date })
    },
    [handleUpdateTaskField],
  )

  const handleUpdatePlannedEnd = useCallback(
    async (taskId: string, date: string | null) => {
      await handleUpdateTaskField(taskId, { plannedEnd: date })
    },
    [handleUpdateTaskField],
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

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case ProjectTabs.Details:
        return <ProjectDetails project={projectData} />
      case ProjectTabs.Tasks:
        return (
          <ProjectTasksTable
            tasks={tasksData || []}
            isLoading={tasksDataIsLoading}
            onCreateTask={handleCreateTask}
            onEditTask={handleEditTask}
            onDeleteTask={handleDeleteTask}
            onRefresh={refetchTasksData}
            onUpdateStatus={handleUpdateStatus}
            onUpdatePriority={handleUpdatePriority}
            onUpdateName={handleUpdateName}
            onUpdateType={handleUpdateType}
            onUpdatePlannedStart={handleUpdatePlannedStart}
            onUpdatePlannedEnd={handleUpdatePlannedEnd}
          />
        )
      case ProjectTabs.WorkItems:
        return (
          <WorkItemsGrid
            workItems={workItemsData}
            isLoading={workItemsDataIsLoading}
            refetch={refetchWorkItemsData}
            hideProjectColumn={true}
          />
        )
      default:
        return null
    }
  }, [
    activeTab,
    projectData,
    tasksData,
    tasksDataIsLoading,
    handleCreateTask,
    handleEditTask,
    handleDeleteTask,
    refetchWorkItemsData,
    workItemsData,
    workItemsDataIsLoading,
  ])

  // doesn't trigger on first render
  const onTabChange = useCallback((tabKey: string) => {
    setActiveTab(tabKey as ProjectTabs)
  }, [])

  const missingDates = projectData?.start === null || projectData?.end === null

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const currentStatus = projectData?.status.name
    const availableActions =
      currentStatus === 'Proposed'
        ? !missingDates
          ? [
              ProjectAction.Edit,
              ProjectAction.Delete,
              ProjectAction.Activate,
              ProjectAction.Cancel,
            ]
          : [ProjectAction.Edit, ProjectAction.Delete, ProjectAction.Cancel]
        : currentStatus === 'Active'
          ? [ProjectAction.Edit, ProjectAction.Complete, ProjectAction.Cancel]
          : []

    // TODO: Implement On Hold status

    const items: ItemType[] = []
    if (canUpdateProject && availableActions.includes(ProjectAction.Edit)) {
      items.push({
        key: 'edit',
        label: ProjectAction.Edit,
        onClick: () => setOpenEditProjectForm(true),
      })
    }
    if (canUpdateProject) {
      items.push({
        key: 'change-program',
        label: ProjectAction.ChangeProgram,
        onClick: () => setOpenChangeProgramForm(true),
      })
    }
    if (canDeleteProject && availableActions.includes(ProjectAction.Delete)) {
      items.push({
        key: 'delete',
        label: ProjectAction.Delete,
        onClick: () => setOpenDeleteProjectForm(true),
      })
    }

    if (
      canUpdateProject &&
      (availableActions.includes(ProjectAction.Activate) ||
        availableActions.includes(ProjectAction.Complete) ||
        availableActions.includes(ProjectAction.Cancel))
    ) {
      items.push({
        key: 'manage-divider',
        type: 'divider',
      })
    }

    if (canUpdateProject && availableActions.includes(ProjectAction.Activate)) {
      items.push({
        key: 'activate',
        label: ProjectAction.Activate,
        onClick: () => setOpenActivateProjectForm(true),
      })
    }

    if (canUpdateProject && availableActions.includes(ProjectAction.Complete)) {
      items.push({
        key: 'complete',
        label: ProjectAction.Complete,
        onClick: () => setOpenCompleteProjectForm(true),
      })
    }

    if (canUpdateProject && availableActions.includes(ProjectAction.Cancel)) {
      items.push({
        key: 'cancel',
        label: ProjectAction.Cancel,
        onClick: () => setOpenCancelProjectForm(true),
      })
    }

    return items
  }, [
    canDeleteProject,
    canUpdateProject,
    missingDates,
    projectData?.status.name,
  ])

  const onEditProjectFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditProjectForm(false)
      if (wasSaved) {
        refetchProject()
      }
    },
    [refetchProject],
  )

  const onChangeProgramFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenChangeProgramForm(false)
      if (wasSaved) {
        refetchProject()
      }
    },
    [refetchProject],
  )

  const onActivateProjectFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenActivateProjectForm(false)
      if (wasSaved) {
        refetchProject()
      }
    },
    [refetchProject],
  )

  const onCompleteProjectFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCompleteProjectForm(false)
      if (wasSaved) {
        refetchProject()
      }
    },
    [refetchProject],
  )

  const onCancelProjectFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCancelProjectForm(false)
      if (wasSaved) {
        refetchProject()
      }
    },
    [refetchProject],
  )

  const onDeleteProjectFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteProjectForm(false)
      if (wasDeleted) {
        router.push('/ppm/projects')
      }
    },
    [router],
  )

  if (isLoading) {
    return <ProjectDetailsLoading />
  }

  if (!projectData) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={`${projectData?.key} - ${projectData?.name}`}
        subtitle="Project Details"
        tags={<LifecycleStatusTag status={projectData?.status} />}
        actions={<PageActions actionItems={actionsMenuItems} />}
      />

      {missingDates === true && (
        <>
          <Alert
            title="Project Dates are required before activating."
            type="warning"
            showIcon
          />
          <br />
        </>
      )}
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {renderTabContent()}
      </Card>

      {openEditProjectForm && (
        <EditProjectForm
          projectKey={projectData?.key}
          showForm={openEditProjectForm}
          onFormComplete={() => onEditProjectFormClosed(true)}
          onFormCancel={() => onEditProjectFormClosed(false)}
        />
      )}
      {openChangeProgramForm && (
        <ChangeProjectProgramForm
          project={projectData}
          showForm={openChangeProgramForm}
          onFormComplete={() => onChangeProgramFormClosed(true)}
          onFormCancel={() => onChangeProgramFormClosed(false)}
        />
      )}
      {openActivateProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Activate}
          showForm={openActivateProjectForm}
          onFormComplete={() => onActivateProjectFormClosed(true)}
          onFormCancel={() => onActivateProjectFormClosed(false)}
        />
      )}
      {openCompleteProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Complete}
          showForm={openCompleteProjectForm}
          onFormComplete={() => onCompleteProjectFormClosed(true)}
          onFormCancel={() => onCompleteProjectFormClosed(false)}
        />
      )}
      {openCancelProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Cancel}
          showForm={openCancelProjectForm}
          onFormComplete={() => onCancelProjectFormClosed(true)}
          onFormCancel={() => onCancelProjectFormClosed(false)}
        />
      )}
      {openDeleteProjectForm && (
        <DeleteProjectForm
          project={projectData}
          showForm={openDeleteProjectForm}
          onFormComplete={() => onDeleteProjectFormClosed(true)}
          onFormCancel={() => onDeleteProjectFormClosed(false)}
        />
      )}

      {openCreateTaskForm && (
        <CreateProjectTaskForm
          projectIdOrKey={projectData?.key || ''}
          showForm={openCreateTaskForm}
          onFormComplete={() => onCreateTaskFormClosed(true)}
          onFormCancel={() => onCreateTaskFormClosed(false)}
        />
      )}
      {openEditTaskForm && selectedTaskId && (
        <EditProjectTaskForm
          projectIdOrKey={projectData?.key || ''}
          taskIdOrKey={selectedTaskId}
          showForm={openEditTaskForm}
          onFormComplete={() => onEditTaskFormClosed(true)}
          onFormCancel={() => onEditTaskFormClosed(false)}
        />
      )}
      {openDeleteTaskForm && selectedTaskId && (
        <DeleteProjectTaskForm
          projectIdOrKey={projectData?.key || ''}
          taskIdOrKey={selectedTaskId}
          showForm={openDeleteTaskForm}
          onFormComplete={() => onDeleteTaskFormClosed(true)}
          onFormCancel={() => onDeleteTaskFormClosed(false)}
        />
      )}
    </>
  )
}

const ProjectDetailsPageWithAuthorization = authorizePage(
  ProjectDetailsPage,
  'Permission',
  'Permissions.Projects.View',
)

export default ProjectDetailsPageWithAuthorization
