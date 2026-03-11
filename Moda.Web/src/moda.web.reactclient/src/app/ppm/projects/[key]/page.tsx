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
import { Alert, Card, MenuProps, Spin } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import ProjectDetailsLoading from './loading'
import dynamic from 'next/dynamic'
import {
  ChangeProjectStatusForm,
  ChangeProjectProgramForm,
  ChangeProjectKeyForm,
  DeleteProjectForm,
  EditProjectForm,
  ProjectDetails,
} from '../_components'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import { ProjectStatusAction } from '../_components/change-project-status-form'

const ProjectPlan = dynamic(
  () => import('../_components/project-plan'),
  { ssr: false, loading: () => <Spin /> },
)

const WorkItemsGrid = dynamic(
  () => import('@/src/components/common/work/work-items-grid'),
  { ssr: false, loading: () => <Spin /> },
)

enum ProjectTabs {
  Details = 'details',
  Plan = 'tasks',
  WorkItems = 'workItems',
}

const tabs = [
  {
    key: ProjectTabs.Details,
    label: 'Details',
  },
  {
    key: ProjectTabs.Plan,
    label: 'Plan',
  },
  {
    key: ProjectTabs.WorkItems,
    label: 'Work Items',
  },
]

enum ProjectAction {
  Edit = 'Edit',
  ChangeProgram = 'Change Program',
  ChangeKey = 'Change Key',
  Delete = 'Delete',
  Approve = 'Approve',
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const ProjectDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key: projectKey } = use(props.params)

  const [activeTab, setActiveTab] = useState(ProjectTabs.Details)
  const [openEditProjectForm, setOpenEditProjectForm] = useState<boolean>(false)
  const [openChangeProgramForm, setOpenChangeProgramForm] =
    useState<boolean>(false)
  const [openChangeKeyForm, setOpenChangeKeyForm] = useState<boolean>(false)
  const [openApproveProjectForm, setOpenApproveProjectForm] =
    useState<boolean>(false)
  const [openActivateProjectForm, setOpenActivateProjectForm] =
    useState<boolean>(false)
  const [openCompleteProjectForm, setOpenCompleteProjectForm] =
    useState<boolean>(false)
  const [openCancelProjectForm, setOpenCancelProjectForm] =
    useState<boolean>(false)
  const [openDeleteProjectForm, setOpenDeleteProjectForm] =
    useState<boolean>(false)

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdateProject = hasPermissionClaim('Permissions.Projects.Update')
  const canDeleteProject = hasPermissionClaim('Permissions.Projects.Delete')

  const {
    data: projectData,
    isLoading,
    refetch: refetchProject,
  } = useGetProjectQuery(projectKey)

  useDocumentTitle(`${projectData?.name ?? projectKey} - Project Details`)

  const {
    data: workItemsData,
    isLoading: workItemsDataIsLoading,
    refetch: refetchWorkItemsData,
  } = useGetProjectWorkItemsQuery(projectData?.id, { skip: !projectData?.id })

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

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case ProjectTabs.Details:
        return <ProjectDetails project={projectData} />
      case ProjectTabs.Plan:
        return (
          <ProjectPlan
            project={projectData}
            canManageTasks={canUpdateProject}
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
    canUpdateProject,
    projectData,
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
              ProjectAction.Approve,
              ProjectAction.Activate,
              ProjectAction.Cancel,
            ]
          : [
              ProjectAction.Edit,
              ProjectAction.Delete,
              ProjectAction.Approve,
              ProjectAction.Cancel,
            ]
        : currentStatus === 'Approved'
          ? !missingDates
            ? [
                ProjectAction.Edit,
                ProjectAction.Activate,
                ProjectAction.Cancel,
              ]
            : [ProjectAction.Edit, ProjectAction.Cancel]
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
      items.push({
        key: 'change-key',
        label: ProjectAction.ChangeKey,
        onClick: () => setOpenChangeKeyForm(true),
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
      (availableActions.includes(ProjectAction.Approve) ||
        availableActions.includes(ProjectAction.Activate) ||
        availableActions.includes(ProjectAction.Complete) ||
        availableActions.includes(ProjectAction.Cancel))
    ) {
      items.push({
        key: 'manage-divider',
        type: 'divider',
      })
    }

    if (canUpdateProject && availableActions.includes(ProjectAction.Approve)) {
      items.push({
        key: 'approve',
        label: ProjectAction.Approve,
        onClick: () => setOpenApproveProjectForm(true),
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

  const onChangeKeyFormClosed = useCallback(
    (wasSaved: boolean, newKey?: string) => {
      setOpenChangeKeyForm(false)
      if (wasSaved) {
        if (newKey && newKey !== projectData?.key) {
          router.push(`/ppm/projects/${newKey}`)
          return
        }
        refetchProject()
      }
    },
    [projectData?.key, refetchProject, router],
  )

  const onApproveProjectFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenApproveProjectForm(false)
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

      {missingDates === true &&
        (projectData?.status.name === 'Proposed' ||
          projectData?.status.name === 'Approved') && (
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
          onFormComplete={() => onEditProjectFormClosed(true)}
          onFormCancel={() => onEditProjectFormClosed(false)}
        />
      )}
      {openChangeProgramForm && (
        <ChangeProjectProgramForm
          project={projectData}
          onFormComplete={() => onChangeProgramFormClosed(true)}
          onFormCancel={() => onChangeProgramFormClosed(false)}
        />
      )}
      {openChangeKeyForm && (
        <ChangeProjectKeyForm
          projectKey={projectData.key}
          onFormComplete={(newKey) => onChangeKeyFormClosed(true, newKey)}
          onFormCancel={() => onChangeKeyFormClosed(false)}
        />
      )}
      {openApproveProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Approve}
          onFormComplete={() => onApproveProjectFormClosed(true)}
          onFormCancel={() => onApproveProjectFormClosed(false)}
        />
      )}
      {openActivateProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Activate}
          onFormComplete={() => onActivateProjectFormClosed(true)}
          onFormCancel={() => onActivateProjectFormClosed(false)}
        />
      )}
      {openCompleteProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Complete}
          onFormComplete={() => onCompleteProjectFormClosed(true)}
          onFormCancel={() => onCompleteProjectFormClosed(false)}
        />
      )}
      {openCancelProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Cancel}
          onFormComplete={() => onCancelProjectFormClosed(true)}
          onFormCancel={() => onCancelProjectFormClosed(false)}
        />
      )}
      {openDeleteProjectForm && (
        <DeleteProjectForm
          project={projectData}
          onFormComplete={() => onDeleteProjectFormClosed(true)}
          onFormCancel={() => onDeleteProjectFormClosed(false)}
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
