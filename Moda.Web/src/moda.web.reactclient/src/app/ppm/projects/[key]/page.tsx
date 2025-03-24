'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { useGetProjectQuery } from '@/src/store/features/ppm/projects-api'
import { Alert, Card, MenuProps, message } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { useCallback, useEffect, useMemo, useState } from 'react'
import ProjectDetailsLoading from './loading'
import {
  ChangeProjectStatusForm,
  DeleteProjectForm,
  EditProjectForm,
  ProjectDetails,
} from '../_components'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import { ProjectStatusAction } from '../_components/change-project-status-form'

enum ProjectTabs {
  Details = 'details',
}

enum ProjectAction {
  Edit = 'Edit',
  Delete = 'Delete',
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const ProjectDetailsPage = ({ params }) => {
  useDocumentTitle('Project Details')
  const [activeTab, setActiveTab] = useState(ProjectTabs.Details)
  const [openEditProjectForm, setOpenEditProjectForm] = useState<boolean>(false)
  const [openActivateProjectForm, setOpenActivateProjectForm] =
    useState<boolean>(false)
  const [openCompleteProjectForm, setOpenCompleteProjectForm] =
    useState<boolean>(false)
  const [openCancelProjectForm, setOpenCancelProjectForm] =
    useState<boolean>(false)
  const [openDeleteProjectForm, setOpenDeleteProjectForm] =
    useState<boolean>(false)

  const [messageApi, contextHolder] = message.useMessage()

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
  } = useGetProjectQuery(params.key)

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

  const tabs = useMemo(() => {
    const pageTabs = [
      {
        key: ProjectTabs.Details,
        label: 'Details',
        content: <ProjectDetails project={projectData} />,
      },
    ]
    return pageTabs
  }, [projectData])

  // doesn't trigger on first render
  const onTabChange = useCallback((tabKey) => {
    setActiveTab(tabKey)
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
    notFound()
  }

  return (
    <>
      {contextHolder}
      <PageTitle
        title={`${projectData?.key} - ${projectData?.name}`}
        subtitle="Project Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />

      {missingDates === true && (
        <>
          <Alert
            message="Project Dates are required before activating."
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
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>

      {openEditProjectForm && (
        <EditProjectForm
          projectKey={projectData?.key}
          showForm={openEditProjectForm}
          onFormComplete={() => onEditProjectFormClosed(true)}
          onFormCancel={() => onEditProjectFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openActivateProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Activate}
          showForm={openActivateProjectForm}
          onFormComplete={() => onActivateProjectFormClosed(true)}
          onFormCancel={() => onActivateProjectFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openCompleteProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Complete}
          showForm={openCompleteProjectForm}
          onFormComplete={() => onCompleteProjectFormClosed(true)}
          onFormCancel={() => onCompleteProjectFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openCancelProjectForm && (
        <ChangeProjectStatusForm
          project={projectData}
          statusAction={ProjectStatusAction.Cancel}
          showForm={openCancelProjectForm}
          onFormComplete={() => onCancelProjectFormClosed(true)}
          onFormCancel={() => onCancelProjectFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openDeleteProjectForm && (
        <DeleteProjectForm
          project={projectData}
          showForm={openDeleteProjectForm}
          onFormComplete={() => onDeleteProjectFormClosed(true)}
          onFormCancel={() => onDeleteProjectFormClosed(false)}
          messageApi={messageApi}
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
