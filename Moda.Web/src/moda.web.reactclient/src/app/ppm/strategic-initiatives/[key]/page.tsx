'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { Card, MenuProps } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import StrategicInitiativeDetailsLoading from './loading'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import {
  useGetStrategicInitiativeKpisQuery,
  useGetStrategicInitiativeProjectsQuery,
  useGetStrategicInitiativeQuery,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import {
  ChangeStrategicInitiativeStatusForm,
  CreateStrategicInitiativeKpiForm,
  DeleteStrategicInitiativeForm,
  ManageStrategicInitiativeProjectsForm,
  StrategicInitiativeDetails,
  StrategicInitiativeKpisGrid,
} from '../_components'
import EditStrategicInitiativeForm from '../_components/edit-strategic-initiative-form'
import { StrategicInitiativeStatusAction } from '../_components/change-strategic-initiative-status-form'
import { ProjectViewManager } from '../../_components'

enum StrategicInitiativeTabs {
  Details = 'details',
  Kpis = 'kpis',
  Projects = 'projects',
}

enum StrategicInitiativeAction {
  Edit = 'Edit',
  Delete = 'Delete',
  Approve = 'Approve',
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const StrategicInitiativeDetailsPage = (props: {
  params: Promise<{ key: number }>
}) => {
  const { key } = use(props.params)

  useDocumentTitle('Strategic Initiative Details')

  const [activeTab, setActiveTab] = useState(StrategicInitiativeTabs.Details)
  const [kpisQueried, setKpisQueried] = useState(false)
  const [projectsQueried, setProjectsQueried] = useState(false)
  const [isReadOnly, setIsReadOnly] = useState(false)

  const [openEditStrategicInitiativeForm, setOpenEditStrategicInitiativeForm] =
    useState<boolean>(false)
  const [
    openApproveStrategicInitiativeForm,
    setOpenApproveStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [
    openActivateStrategicInitiativeForm,
    setOpenActivateStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [
    openCompleteStrategicInitiativeForm,
    setOpenCompleteStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [
    openCancelStrategicInitiativeForm,
    setOpenCancelStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [
    openDeleteStrategicInitiativeForm,
    setOpenDeleteStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [openCreateKpiForm, setOpenCreateKpiForm] = useState(false)
  const [openManageProjectsForm, setOpenManageProjectsForm] = useState(false)

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdateStrategicInitiative = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )
  const canDeleteStrategicInitiative = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Delete',
  )

  const {
    data: strategicInitiativeData,
    isLoading,
    error,
    refetch: refetchStrategicInitiative,
  } = useGetStrategicInitiativeQuery(key)

  const {
    data: kpiData,
    isLoading: isLoadingKpis,
    error: kpiError,
    refetch: refetchKpis,
  } = useGetStrategicInitiativeKpisQuery(strategicInitiativeData?.id, {
    skip: !kpisQueried,
  })

  const {
    data: projectData,
    isLoading: isLoadingProjects,
    error: errorProjects,
    refetch: refetchProjects,
  } = useGetStrategicInitiativeProjectsQuery(strategicInitiativeData?.id, {
    skip: !projectsQueried,
  })

  useEffect(() => {
    if (!strategicInitiativeData) return

    const status = strategicInitiativeData.status.name
    setIsReadOnly(status === 'Completed' || status === 'Cancelled')

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'PPM',
      },
      {
        href: `/ppm/strategic-initiatives`,
        title: 'Strategic Initiatives',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, strategicInitiativeData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const tabs = useMemo(() => {
    const pageTabs = [
      {
        key: StrategicInitiativeTabs.Details,
        label: 'Details',
        content: (
          <StrategicInitiativeDetails
            strategicInitiative={strategicInitiativeData}
          />
        ),
      },
      {
        key: StrategicInitiativeTabs.Kpis,
        label: 'KPIs',
        content: (
          <StrategicInitiativeKpisGrid
            strategicInitiativeId={strategicInitiativeData?.id}
            kpis={kpiData}
            canManageKpis={canUpdateStrategicInitiative}
            isLoading={isLoadingKpis}
            refetch={refetchKpis}
            gridHeight={550}
            isReadOnly={isReadOnly}
          />
        ),
      },
      {
        key: StrategicInitiativeTabs.Projects,
        label: 'Projects',
        content: (
          <ProjectViewManager
            projects={projectData}
            isLoading={isLoadingProjects}
            refetch={refetchProjects}
          />
        ),
      },
    ]
    return pageTabs
  }, [
    strategicInitiativeData,
    kpiData,
    canUpdateStrategicInitiative,
    isLoadingKpis,
    refetchKpis,
    isReadOnly,
    projectData,
    isLoadingProjects,
    refetchProjects,
  ])

  // doesn't trigger on first render
  const onTabChange = useCallback(
    (tabKey: StrategicInitiativeTabs) => {
      if (tabKey === StrategicInitiativeTabs.Kpis && !kpisQueried) {
        setKpisQueried(true)
      } else if (
        tabKey === StrategicInitiativeTabs.Projects &&
        !projectsQueried
      ) {
        setProjectsQueried(true)
      }

      setActiveTab(tabKey)
    },
    [kpisQueried, projectsQueried],
  )

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const currentStatus = strategicInitiativeData?.status.name
    const availableActions =
      currentStatus === 'Proposed'
        ? [
            StrategicInitiativeAction.Edit,
            StrategicInitiativeAction.Delete,
            StrategicInitiativeAction.Approve,
            StrategicInitiativeAction.Cancel,
          ]
        : currentStatus === 'Approved'
          ? [
              StrategicInitiativeAction.Edit,
              StrategicInitiativeAction.Delete,
              StrategicInitiativeAction.Activate,
              StrategicInitiativeAction.Cancel,
            ]
          : currentStatus === 'Active'
            ? [
                StrategicInitiativeAction.Edit,
                StrategicInitiativeAction.Complete,
                StrategicInitiativeAction.Cancel,
              ]
            : []

    // TODO: Implement On Hold status

    const items: ItemType[] = []
    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Edit)
    ) {
      items.push({
        key: 'edit',
        label: StrategicInitiativeAction.Edit,
        onClick: () => setOpenEditStrategicInitiativeForm(true),
      })
    }
    if (
      canDeleteStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Delete)
    ) {
      items.push({
        key: 'delete',
        label: StrategicInitiativeAction.Delete,
        onClick: () => setOpenDeleteStrategicInitiativeForm(true),
      })
    }

    if (
      canUpdateStrategicInitiative &&
      (availableActions.includes(StrategicInitiativeAction.Approve) ||
        availableActions.includes(StrategicInitiativeAction.Activate) ||
        availableActions.includes(StrategicInitiativeAction.Complete) ||
        availableActions.includes(StrategicInitiativeAction.Cancel))
    ) {
      items.push({
        key: 'manage-divider',
        type: 'divider',
      })
    }

    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Approve)
    ) {
      items.push({
        key: 'approve',
        label: StrategicInitiativeAction.Approve,
        onClick: () => setOpenApproveStrategicInitiativeForm(true),
      })
    }

    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Activate)
    ) {
      items.push({
        key: 'activate',
        label: StrategicInitiativeAction.Activate,
        onClick: () => setOpenActivateStrategicInitiativeForm(true),
      })
    }

    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Complete)
    ) {
      items.push({
        key: 'complete',
        label: StrategicInitiativeAction.Complete,
        onClick: () => setOpenCompleteStrategicInitiativeForm(true),
      })
    }

    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Cancel)
    ) {
      items.push({
        key: 'cancel',
        label: StrategicInitiativeAction.Cancel,
        onClick: () => setOpenCancelStrategicInitiativeForm(true),
      })
    }

    //KPI and Project actions
    if (!isReadOnly && canUpdateStrategicInitiative) {
      items.push(
        {
          key: 'manage-divider-kps',
          type: 'divider',
        },
        {
          key: 'createKpi',
          label: 'Create KPI',
          onClick: () => setOpenCreateKpiForm(true),
        },
        {
          key: 'manage-divider-projects',
          type: 'divider',
        },
        {
          key: 'manageProjects',
          label: 'Manage Projects',
          onClick: () => setOpenManageProjectsForm(true),
        },
      )
    }

    return items
  }, [
    canDeleteStrategicInitiative,
    canUpdateStrategicInitiative,
    isReadOnly,
    strategicInitiativeData?.status.name,
  ])

  const onEditStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onApproveStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenApproveStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onActivateStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenActivateStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onCompleteStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCompleteStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onCancelStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCancelStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onDeleteStrategicInitiativeFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteStrategicInitiativeForm(false)
      if (wasDeleted) {
        router.push('/ppm/strategic-initiatives')
      }
    },
    [router],
  )

  const onCreateKpiFormClosed = useCallback(() => {
    setOpenCreateKpiForm(false)
  }, [])

  if (isLoading) {
    return <StrategicInitiativeDetailsLoading />
  }

  if (!strategicInitiativeData) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={`${strategicInitiativeData?.key} - ${strategicInitiativeData?.name}`}
        subtitle="Strategic Initiative Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>

      {openEditStrategicInitiativeForm && (
        <EditStrategicInitiativeForm
          strategicInitiativeKey={strategicInitiativeData?.key}
          showForm={openEditStrategicInitiativeForm}
          onFormComplete={() => onEditStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onEditStrategicInitiativeFormClosed(false)}
        />
      )}
      {openApproveStrategicInitiativeForm && (
        <ChangeStrategicInitiativeStatusForm
          strategicInitiative={strategicInitiativeData}
          statusAction={StrategicInitiativeStatusAction.Approve}
          showForm={openApproveStrategicInitiativeForm}
          onFormComplete={() => onApproveStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onApproveStrategicInitiativeFormClosed(false)}
        />
      )}
      {openActivateStrategicInitiativeForm && (
        <ChangeStrategicInitiativeStatusForm
          strategicInitiative={strategicInitiativeData}
          statusAction={StrategicInitiativeStatusAction.Activate}
          showForm={openActivateStrategicInitiativeForm}
          onFormComplete={() => onActivateStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onActivateStrategicInitiativeFormClosed(false)}
        />
      )}
      {openCompleteStrategicInitiativeForm && (
        <ChangeStrategicInitiativeStatusForm
          strategicInitiative={strategicInitiativeData}
          statusAction={StrategicInitiativeStatusAction.Complete}
          showForm={openCompleteStrategicInitiativeForm}
          onFormComplete={() => onCompleteStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onCompleteStrategicInitiativeFormClosed(false)}
        />
      )}
      {openCancelStrategicInitiativeForm && (
        <ChangeStrategicInitiativeStatusForm
          strategicInitiative={strategicInitiativeData}
          statusAction={StrategicInitiativeStatusAction.Cancel}
          showForm={openCancelStrategicInitiativeForm}
          onFormComplete={() => onCancelStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onCancelStrategicInitiativeFormClosed(false)}
        />
      )}
      {openDeleteStrategicInitiativeForm && (
        <DeleteStrategicInitiativeForm
          strategicInitiative={strategicInitiativeData}
          showForm={openDeleteStrategicInitiativeForm}
          onFormComplete={() => onDeleteStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onDeleteStrategicInitiativeFormClosed(false)}
        />
      )}
      {openCreateKpiForm && (
        <CreateStrategicInitiativeKpiForm
          strategicInitiativeId={strategicInitiativeData?.id}
          showForm={openCreateKpiForm}
          onFormComplete={() => onCreateKpiFormClosed()}
          onFormCancel={() => onCreateKpiFormClosed()}
        />
      )}
      {openManageProjectsForm && (
        <ManageStrategicInitiativeProjectsForm
          strategicInitiativeId={strategicInitiativeData?.id}
          portfolioKey={strategicInitiativeData?.portfolio.key}
          showForm={openManageProjectsForm}
          onFormComplete={() => setOpenManageProjectsForm(false)}
          onFormCancel={() => setOpenManageProjectsForm(false)}
        />
      )}
    </>
  )
}

const StrategicInitiativeDetailsPageWithAuthorization = authorizePage(
  StrategicInitiativeDetailsPage,
  'Permission',
  'Permissions.StrategicInitiatives.View',
)

export default StrategicInitiativeDetailsPageWithAuthorization
