'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { Card, Descriptions, MenuProps, message, Space } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import PortfolioDetailsLoading from './loading'
import { useCallback, useEffect, useMemo, useState } from 'react'
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

const { Item } = Descriptions

enum PortfolioTabs {
  Details = 'details',
  Projects = 'projects',
  StrategicInitiatives = 'strategicInitiatives',
}

enum MenuActions {
  Edit = 'Edit',
  Delete = 'Delete',
  Activate = 'Activate',
  Close = 'Close',
  Archive = 'Archive',
}

const PortfolioDetailsPage = ({ params }) => {
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

  const [messageApi, contextHolder] = message.useMessage()

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
  } = useGetPortfolioQuery(params.key)

  const {
    data: projectData,
    isLoading: isLoadingProjects,
    error: errorProjects,
    refetch: refetchProjects,
  } = useGetPortfolioProjectsQuery(params.key, { skip: !projectsQueried })

  const {
    data: strategicInitiativeData,
    isLoading: isLoadingStrategicInitiatives,
    error: errorStrategicInitiatives,
    refetch: refetchStrategicInitiatives,
  } = useGetPortfolioStrategicInitiativesQuery(params.key, {
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

  const tabs = useMemo(() => {
    const pageTabs = [
      {
        key: PortfolioTabs.Details,
        label: 'Details',
        content: <PortfolioDetails portfolio={portfolioData} />,
      },
      {
        key: PortfolioTabs.Projects,
        label: 'Projects',
        content: (
          <ProjectViewManager
            projects={projectData}
            isLoading={isLoadingProjects}
            refetch={refetchProjects}
            messageApi={messageApi}
          />
        ),
      },
      {
        key: PortfolioTabs.StrategicInitiatives,
        label: 'Strategic Initiatives',
        content: (
          <StrategicInitiativeViewManager
            strategicInitiatives={strategicInitiativeData}
            isLoading={isLoadingStrategicInitiatives}
            refetch={refetchStrategicInitiatives}
            messageApi={messageApi}
          />
        ),
      },
    ]
    return pageTabs
  }, [
    isLoadingProjects,
    isLoadingStrategicInitiatives,
    messageApi,
    portfolioData,
    projectData,
    refetchProjects,
    refetchStrategicInitiatives,
    strategicInitiativeData,
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
    (tabKey) => {
      if (tabKey === PortfolioTabs.Projects && !projectsQueried) {
        setProjectsQueried(true)
      } else if (
        tabKey === PortfolioTabs.StrategicInitiatives &&
        !strategicInitiativesQueried
      ) {
        setStrategicInitiativesQueried(true)
      }
      setActiveTab(tabKey)
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
    notFound()
  }

  return (
    <>
      {contextHolder}
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
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>

      {openEditPortfolioForm && (
        <EditPortfolioForm
          portfolioKey={portfolioData?.key}
          showForm={openEditPortfolioForm}
          onFormComplete={() => onEditPortfolioFormClosed(true)}
          onFormCancel={() => onEditPortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openActivatePortfolioForm && (
        <ChangePortfolioStatusForm
          portfolio={portfolioData}
          statusAction={PortfolioStatusAction.Activate}
          showForm={openActivatePortfolioForm}
          onFormComplete={() => onActivatePortfolioFormClosed(true)}
          onFormCancel={() => onActivatePortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openClosePortfolioForm && (
        <ChangePortfolioStatusForm
          portfolio={portfolioData}
          statusAction={PortfolioStatusAction.Close}
          showForm={openClosePortfolioForm}
          onFormComplete={() => onClosePortfolioFormClosed(true)}
          onFormCancel={() => onClosePortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openArchivePortfolioForm && (
        <ChangePortfolioStatusForm
          portfolio={portfolioData}
          statusAction={PortfolioStatusAction.Archive}
          showForm={openArchivePortfolioForm}
          onFormComplete={() => onArchivePortfolioFormClosed(true)}
          onFormCancel={() => onArchivePortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openDeletePortfolioForm && (
        <DeletePortfolioForm
          portfolio={portfolioData}
          showForm={openDeletePortfolioForm}
          onFormComplete={() => onDeletePortfolioFormClosed(true)}
          onFormCancel={() => onDeletePortfolioFormClosed(false)}
          messageApi={messageApi}
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
