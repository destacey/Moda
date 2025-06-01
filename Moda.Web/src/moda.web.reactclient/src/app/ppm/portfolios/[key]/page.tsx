'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { Card, MenuProps } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import PortfolioDetailsLoading from './loading'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import {
  DeletePortfolioForm,
  EditPortfolioForm,
  PortfolioDetails,
} from '../_components'
import {
  useGetPortfolioProjectsQuery,
  useGetPortfolioQuery,
  useGetPortfolioStrategicInitiativesQuery,
} from '@/src/store/features/ppm/portfolios-api'
import ChangePortfolioStatusForm, {
  PortfolioStatusAction,
} from '../_components/change-portfolio-status-form'
import {
  ProjectViewManager,
  StrategicInitiativeViewManager,
} from '../../_components'

enum PortfolioTabs {
  Details = 'details',
  Projects = 'projects',
  StrategicInitiatives = 'strategicInitiatives',
}

const tabs = [
  {
    key: PortfolioTabs.Details,
    label: 'Details',
  },
  {
    key: PortfolioTabs.Projects,
    label: 'Projects',
  },
  {
    key: PortfolioTabs.StrategicInitiatives,
    label: 'Strategic Initiatives',
  },
]

enum MenuActions {
  Edit = 'Edit',
  Delete = 'Delete',
  Activate = 'Activate',
  Close = 'Close',
  Archive = 'Archive',
}

const PortfolioDetailsPage = (props: { params: Promise<{ key: number }> }) => {
  const { key } = use(props.params)

  useDocumentTitle('Portfolio Details')

  const [activeTab, setActiveTab] = useState(PortfolioTabs.Details)
  const [projectsQueried, setProjectsQueried] = useState(false)
  const [strategicInitiativesQueried, setStrategicInitiativesQueried] =
    useState(false)

  const [openEditPortfolioForm, setOpenEditPortfolioForm] =
    useState<boolean>(false)
  const [openActivatePortfolioForm, setOpenActivatePortfolioForm] =
    useState<boolean>(false)
  const [openClosePortfolioForm, setOpenClosePortfolioForm] =
    useState<boolean>(false)
  const [openArchivePortfolioForm, setOpenArchivePortfolioForm] =
    useState<boolean>(false)
  const [openDeletePortfolioForm, setOpenDeletePortfolioForm] =
    useState<boolean>(false)

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdatePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Update',
  )
  const canDeletePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Delete',
  )

  const {
    data: portfolioData,
    isLoading,
    error,
    refetch: refetchPortfolio,
  } = useGetPortfolioQuery(key)

  const {
    data: projectData,
    isLoading: isLoadingProjects,
    error: errorProjects,
    refetch: refetchProjects,
  } = useGetPortfolioProjectsQuery(key.toString(), { skip: !projectsQueried })

  const {
    data: strategicInitiativeData,
    isLoading: isLoadingStrategicInitiatives,
    error: errorStrategicInitiatives,
    refetch: refetchStrategicInitiatives,
  } = useGetPortfolioStrategicInitiativesQuery(key.toString(), {
    skip: !strategicInitiativesQueried,
  })

  useEffect(() => {
    if (!portfolioData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'PPM',
      },
      {
        href: `/ppm/portfolios`,
        title: 'Portfolios',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, portfolioData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case PortfolioTabs.Details:
        return <PortfolioDetails portfolio={portfolioData} />
      case PortfolioTabs.Projects:
        return (
          <ProjectViewManager
            projects={projectData}
            isLoading={isLoadingProjects}
            refetch={refetchProjects}
          />
        )
      case PortfolioTabs.StrategicInitiatives:
        return (
          <StrategicInitiativeViewManager
            strategicInitiatives={strategicInitiativeData}
            isLoading={isLoadingStrategicInitiatives}
            refetch={refetchStrategicInitiatives}
          />
        )
      default:
        return null
    }
  }, [
    activeTab,
    portfolioData,
    projectData,
    strategicInitiativeData,
    isLoadingProjects,
    isLoadingStrategicInitiatives,
    refetchProjects,
    refetchStrategicInitiatives,
  ])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const currentStatus = portfolioData?.status.name
    const availableActions =
      currentStatus === 'Proposed'
        ? [MenuActions.Delete, MenuActions.Activate]
        : currentStatus === 'Active'
          ? [MenuActions.Close]
          : currentStatus === 'Closed'
            ? [MenuActions.Archive]
            : []

    // TODO: Implement On Hold status

    const items: ItemType[] = []
    if (canUpdatePortfolio && currentStatus !== 'Archived') {
      items.push({
        key: 'edit',
        label: MenuActions.Edit,
        onClick: () => setOpenEditPortfolioForm(true),
      })
    }
    if (canDeletePortfolio && availableActions.includes(MenuActions.Delete)) {
      items.push({
        key: 'delete',
        label: MenuActions.Delete,
        onClick: () => setOpenDeletePortfolioForm(true),
      })
    }

    if (
      canUpdatePortfolio &&
      (availableActions.includes(MenuActions.Activate) ||
        availableActions.includes(MenuActions.Archive))
    ) {
      items.push({
        key: 'manage-divider',
        type: 'divider',
      })
    }

    if (canUpdatePortfolio && availableActions.includes(MenuActions.Activate)) {
      items.push({
        key: 'activate',
        label: MenuActions.Activate,
        onClick: () => setOpenActivatePortfolioForm(true),
      })
    }

    if (canUpdatePortfolio && availableActions.includes(MenuActions.Close)) {
      items.push({
        key: 'close',
        label: MenuActions.Close,
        onClick: () => setOpenClosePortfolioForm(true),
      })
    }

    if (canUpdatePortfolio && availableActions.includes(MenuActions.Archive)) {
      items.push({
        key: 'archive',
        label: MenuActions.Archive,
        onClick: () => setOpenArchivePortfolioForm(true),
      })
    }

    return items
  }, [canDeletePortfolio, canUpdatePortfolio, portfolioData?.status.name])

  // doesn't trigger on first render
  const onTabChange = useCallback(
    (tabKey: string) => {
      const tab = tabKey as PortfolioTabs

      if (tab === PortfolioTabs.Projects && !projectsQueried) {
        setProjectsQueried(true)
      } else if (
        tab === PortfolioTabs.StrategicInitiatives &&
        !strategicInitiativesQueried
      ) {
        setStrategicInitiativesQueried(true)
      }

      setActiveTab(tab)
    },
    [projectsQueried, strategicInitiativesQueried],
  )

  const onEditPortfolioFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditPortfolioForm(false)
      if (wasSaved) {
        refetchPortfolio()
      }
    },
    [refetchPortfolio],
  )

  const onActivatePortfolioFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenActivatePortfolioForm(false)
      if (wasSaved) {
        refetchPortfolio()
      }
    },
    [refetchPortfolio],
  )

  const onClosePortfolioFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenClosePortfolioForm(false)
      if (wasSaved) {
        refetchPortfolio()
      }
    },
    [refetchPortfolio],
  )

  const onArchivePortfolioFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenArchivePortfolioForm(false)
      if (wasSaved) {
        refetchPortfolio()
      }
    },
    [refetchPortfolio],
  )

  const onDeletePortfolioFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeletePortfolioForm(false)
      if (wasDeleted) {
        router.push('/ppm/portfolios')
      }
    },
    [router],
  )

  if (isLoading) {
    return <PortfolioDetailsLoading />
  }

  if (!portfolioData) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={`${portfolioData?.key} - ${portfolioData?.name}`}
        subtitle="Portfolio Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {renderTabContent()}
      </Card>

      {openEditPortfolioForm && (
        <EditPortfolioForm
          portfolioKey={portfolioData?.key}
          showForm={openEditPortfolioForm}
          onFormComplete={() => onEditPortfolioFormClosed(true)}
          onFormCancel={() => onEditPortfolioFormClosed(false)}
        />
      )}
      {openActivatePortfolioForm && (
        <ChangePortfolioStatusForm
          portfolio={portfolioData}
          statusAction={PortfolioStatusAction.Activate}
          showForm={openActivatePortfolioForm}
          onFormComplete={() => onActivatePortfolioFormClosed(true)}
          onFormCancel={() => onActivatePortfolioFormClosed(false)}
        />
      )}
      {openClosePortfolioForm && (
        <ChangePortfolioStatusForm
          portfolio={portfolioData}
          statusAction={PortfolioStatusAction.Close}
          showForm={openClosePortfolioForm}
          onFormComplete={() => onClosePortfolioFormClosed(true)}
          onFormCancel={() => onClosePortfolioFormClosed(false)}
        />
      )}
      {openArchivePortfolioForm && (
        <ChangePortfolioStatusForm
          portfolio={portfolioData}
          statusAction={PortfolioStatusAction.Archive}
          showForm={openArchivePortfolioForm}
          onFormComplete={() => onArchivePortfolioFormClosed(true)}
          onFormCancel={() => onArchivePortfolioFormClosed(false)}
        />
      )}
      {openDeletePortfolioForm && (
        <DeletePortfolioForm
          portfolio={portfolioData}
          showForm={openDeletePortfolioForm}
          onFormComplete={() => onDeletePortfolioFormClosed(true)}
          onFormCancel={() => onDeletePortfolioFormClosed(false)}
        />
      )}
    </>
  )
}

const PortfolioDetailsPageWithAuthorization = authorizePage(
  PortfolioDetailsPage,
  'Permission',
  'Permissions.ProjectPortfolios.View',
)

export default PortfolioDetailsPageWithAuthorization
