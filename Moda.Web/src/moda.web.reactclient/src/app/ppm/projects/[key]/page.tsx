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
import { Alert, MenuProps, Tabs } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { use, useEffect, useState } from 'react'
import ProjectDetailsLoading from './loading'
import dynamic from 'next/dynamic'
import {
  ChangeProjectStatusForm,
  ChangeProjectProgramForm,
  ChangeProjectKeyForm,
  DeleteProjectForm,
  EditProjectForm,
  ProjectDetailsTab,
} from '../_components'
import AssignProjectLifecycleForm from '../_components/assign-project-lifecycle-form'
import ChangeProjectLifecycleForm from '../_components/change-project-lifecycle-form'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import { ProjectStatusAction } from '../_components/change-project-status-form'

const ProjectPlan = dynamic(() => import('../_components/project-plan'), {
  ssr: false,
})

const ProjectTeamGrid = dynamic(
  () => import('../_components/project-team-grid'),
  { ssr: false },
)

const ProjectWorkItemsViewManager = dynamic(
  () => import('../_components/project-work-items-view-manager'),
  { ssr: false },
)

enum ProjectTabs {
  Details = 'details',
  Team = 'team',
  Plan = 'plan',
  WorkItems = 'workItems',
}

enum ProjectAction {
  Edit = 'Edit',
  AssignLifecycle = 'Assign Lifecycle',
  ChangeLifecycle = 'Change Lifecycle',
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

  const [activeTab, setActiveTab] = useState(() => {
    if (typeof window !== 'undefined') {
      const hash = window.location.hash.replace('#', '')
      if (Object.values(ProjectTabs).includes(hash as ProjectTabs)) {
        return hash as ProjectTabs
      }
    }
    return ProjectTabs.Details
  })
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
  const [openAssignLifecycleForm, setOpenAssignLifecycleForm] =
    useState<boolean>(false)
  const [openChangeLifecycleForm, setOpenChangeLifecycleForm] =
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

  const tabs = (() => {
    const items = [
      { key: ProjectTabs.Details, label: 'Details' },
      { key: ProjectTabs.Team, label: 'Team' },
    ]
    if (projectData?.projectLifecycle) {
      items.push({ key: ProjectTabs.Plan, label: 'Plan' })
    }
    items.push({ key: ProjectTabs.WorkItems, label: 'Work Items' })
    return items
  })()

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

  const renderTabContent = () => {
    switch (activeTab) {
      case ProjectTabs.Details:
        return <ProjectDetailsTab project={projectData} />
      case ProjectTabs.Team:
        return <ProjectTeamGrid projectIdOrKey={projectKey} />
      case ProjectTabs.Plan:
        return (
          <ProjectPlan
            project={projectData}
            canManageTasks={canUpdateProject}
          />
        )
      case ProjectTabs.WorkItems:
        return (
          <ProjectWorkItemsViewManager
            workItems={workItemsData}
            isLoading={workItemsDataIsLoading}
            refetch={refetchWorkItemsData}
            hideProjectColumn={true}
          />
        )
      default:
        return null
    }
  }

  const onTabChange = (tabKey: string) => {
    setActiveTab(tabKey as ProjectTabs)
    const base = window.location.pathname + window.location.search
    if (tabKey === ProjectTabs.Details) {
      window.history.replaceState(null, '', base)
    } else {
      window.history.replaceState(null, '', `${base}#${tabKey}`)
    }
  }

  const missingDates = projectData?.start === null || projectData?.end === null
  const missingLifecycle = !projectData?.projectLifecycle

  const actionsMenuItems: MenuProps['items'] = (() => {
    const currentStatus = projectData?.status.name
    const availableActions =
      currentStatus === 'Proposed'
        ? !missingDates && !missingLifecycle
          ? [
              ProjectAction.Edit,
              ProjectAction.Delete,
              ProjectAction.Approve,
              ProjectAction.Activate,
              ProjectAction.Cancel,
            ]
          : missingLifecycle
            ? [ProjectAction.Edit, ProjectAction.Delete, ProjectAction.Cancel]
            : [
                ProjectAction.Edit,
                ProjectAction.Delete,
                ProjectAction.Approve,
                ProjectAction.Cancel,
              ]
        : currentStatus === 'Approved'
          ? !missingDates
            ? [ProjectAction.Edit, ProjectAction.Activate, ProjectAction.Cancel]
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
      if (!projectData?.projectLifecycle) {
        items.push({
          key: 'assign-lifecycle',
          label: ProjectAction.AssignLifecycle,
          onClick: () => setOpenAssignLifecycleForm(true),
        })
      } else {
        items.push({
          key: 'change-lifecycle',
          label: ProjectAction.ChangeLifecycle,
          onClick: () => setOpenChangeLifecycleForm(true),
        })
      }
    }
    if (canDeleteProject && availableActions.includes(ProjectAction.Delete)) {
      items.push({
        key: 'delete',
        label: ProjectAction.Delete,
        danger: true,
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
  })()

  const onEditProjectFormClosed = (wasSaved: boolean) => {
    setOpenEditProjectForm(false)
    if (wasSaved) {
      refetchProject()
    }
  }

  const onAssignLifecycleFormClosed = (wasSaved: boolean) => {
    setOpenAssignLifecycleForm(false)
    if (wasSaved) {
      refetchProject()
    }
  }

  const onChangeLifecycleFormClosed = (wasSaved: boolean) => {
    setOpenChangeLifecycleForm(false)
    if (wasSaved) {
      refetchProject()
    }
  }

  const onChangeProgramFormClosed = (wasSaved: boolean) => {
    setOpenChangeProgramForm(false)
    if (wasSaved) {
      refetchProject()
    }
  }

  const onChangeKeyFormClosed = (wasSaved: boolean, newKey?: string) => {
    setOpenChangeKeyForm(false)
    if (wasSaved) {
      if (newKey && newKey !== projectData?.key) {
        router.push(`/ppm/projects/${newKey}`)
        return
      }
      refetchProject()
    }
  }

  const onApproveProjectFormClosed = (wasSaved: boolean) => {
    setOpenApproveProjectForm(false)
    if (wasSaved) {
      refetchProject()
    }
  }

  const onActivateProjectFormClosed = (wasSaved: boolean) => {
    setOpenActivateProjectForm(false)
    if (wasSaved) {
      refetchProject()
    }
  }

  const onCompleteProjectFormClosed = (wasSaved: boolean) => {
    setOpenCompleteProjectForm(false)
    if (wasSaved) {
      refetchProject()
    }
  }

  const onCancelProjectFormClosed = (wasSaved: boolean) => {
    setOpenCancelProjectForm(false)
    if (wasSaved) {
      refetchProject()
    }
  }

  const onDeleteProjectFormClosed = (wasDeleted: boolean) => {
    setOpenDeleteProjectForm(false)
    if (wasDeleted) {
      router.push('/ppm/projects')
    }
  }

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

      {(projectData?.status.name === 'Proposed' ||
        projectData?.status.name === 'Approved') && (
        <>
          {missingDates && (
            <Alert
              title="Project Dates are required before activating."
              type="warning"
              showIcon
            />
          )}
          {!projectData?.projectLifecycle && (
            <Alert
              title="A Project Lifecycle is required before approving."
              type="warning"
              showIcon
              style={missingDates ? { marginTop: 8 } : undefined}
            />
          )}
          {(missingDates || !projectData?.projectLifecycle) && <br />}
        </>
      )}
      <Tabs
        size="large"
        items={tabs}
        activeKey={activeTab}
        onChange={onTabChange}
      />
      {renderTabContent()}

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
      {openAssignLifecycleForm && (
        <AssignProjectLifecycleForm
          project={projectData}
          onFormComplete={() => onAssignLifecycleFormClosed(true)}
          onFormCancel={() => onAssignLifecycleFormClosed(false)}
        />
      )}
      {openChangeLifecycleForm && (
        <ChangeProjectLifecycleForm
          project={projectData}
          onFormComplete={() => onChangeLifecycleFormClosed(true)}
          onFormCancel={() => onChangeLifecycleFormClosed(false)}
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
