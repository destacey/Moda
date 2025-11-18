'use client'

import PageTitle from '@/src/components/common/page-title'
import { Card, Space } from 'antd'
import {
  createElement,
  use,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import TeamsGrid, {
  TeamsGridProps,
} from '@/src/components/common/organizations/teams-grid'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { useAppDispatch } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import PlanningIntervalDetailsLoading from './loading'
import { IconMenu, PageActions } from '@/src/components/common'
import { ItemType } from 'antd/es/menu/interface'
import {
  EditPlanningIntervalForm,
  ManagePlanningIntervalDatesForm,
  ManagePlanningIntervalTeamsForm,
  PlanningIntervalDetails,
} from '../_components'
import {
  useGetPlanningIntervalQuery,
  useGetPlanningIntervalsQuery,
  useGetPlanningIntervalTeamsQuery,
} from '@/src/store/features/planning/planning-interval-api'
import { IterationState } from '@/src/components/types'
import { IterationStateTag } from '@/src/components/common/planning'
import { SwapOutlined } from '@ant-design/icons'

enum PlanningIntervalTabs {
  Details = 'details',
  Teams = 'teams',
}

const tabs = [
  {
    key: PlanningIntervalTabs.Details,
    tab: 'Details',
  },
  {
    key: PlanningIntervalTabs.Teams,
    tab: 'Teams',
  },
]

const PlanningIntervalDetailsPage = (props: {
  params: Promise<{ key: string }>
}) => {
  const { key } = use(props.params)
  const piKey = Number(key)

  const [activeTab, setActiveTab] = useState(PlanningIntervalTabs.Details)
  const [openEditPlanningIntervalForm, setOpenEditPlanningIntervalForm] =
    useState<boolean>(false)
  const [teamsQueryEnabled, setTeamsQueryEnabled] = useState<boolean>(false)
  const [
    openManagePlanningIntervalDatesForm,
    setOpenManagePlanningIntervalDatesForm,
  ] = useState<boolean>(false)
  const [openManageTeamsForm, setOpenManageTeamsForm] = useState<boolean>(false)

  const pathname = usePathname()
  const router = useRouter()
  const dispatch = useAppDispatch()

  const { hasPermissionClaim } = useAuth()
  const canUpdatePlanningInterval = hasPermissionClaim(
    'Permissions.PlanningIntervals.Update',
  )

  const {
    data: planningIntervalData,
    isLoading,
    error,
    refetch: refetchPlanningInterval,
  } = useGetPlanningIntervalQuery(+piKey)

  useDocumentTitle(`${planningIntervalData?.name ?? piKey} - PI Details`)

  const {
    data: teamsData,
    isLoading: teamsIsLoading,
    refetch: refetchTeams,
  } = useGetPlanningIntervalTeamsQuery(+piKey, {
    skip: !teamsQueryEnabled,
  })

  const { data: piListData } = useGetPlanningIntervalsQuery()

  const actionsMenuItems = useMemo(() => {
    const items = [] as ItemType[]
    if (canUpdatePlanningInterval) {
      items.push(
        {
          key: 'edit-pi-menu-item',
          label: 'Edit',
          onClick: () => setOpenEditPlanningIntervalForm(true),
        },
        {
          key: 'manage-dates-menu-item',
          label: 'Manage Dates',
          onClick: () => setOpenManagePlanningIntervalDatesForm(true),
        },
        {
          key: 'manage-teams-menu-item',
          label: 'Manage Teams',
          onClick: () => setOpenManageTeamsForm(true),
        },
      )
    }
    return items
  }, [canUpdatePlanningInterval])

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case PlanningIntervalTabs.Details:
        return (
          <PlanningIntervalDetails planningInterval={planningIntervalData} />
        )
      case PlanningIntervalTabs.Teams:
        return createElement(TeamsGrid, {
          teams: teamsData,
          isLoading: teamsIsLoading,
          refetch: refetchTeams,
        } as TeamsGridProps)
      default:
        return null
    }
  }, [activeTab, planningIntervalData, refetchTeams, teamsData, teamsIsLoading])

  useEffect(() => {
    planningIntervalData &&
      dispatch(
        setBreadcrumbTitle({ title: planningIntervalData.name, pathname }),
      )
  }, [dispatch, pathname, planningIntervalData])

  const onEditFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditPlanningIntervalForm(false)
      if (wasSaved) {
        refetchPlanningInterval()
      }
    },
    [refetchPlanningInterval],
  )

  const onManagePlanningIntervalDatesFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenManagePlanningIntervalDatesForm(false)
      if (wasSaved) {
        refetchPlanningInterval()
      }
    },
    [refetchPlanningInterval],
  )

  const onManageTeamsFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenManageTeamsForm(false)
      if (wasSaved) {
        refetchPlanningInterval()
      }
    },
    [refetchPlanningInterval],
  )

  // doesn't trigger on first render
  const onTabChange = useCallback(
    (tabKey: string) => {
      setActiveTab(tabKey as PlanningIntervalTabs)

      // enables the query for the tab on first render if it hasn't been enabled yet
      if (tabKey == PlanningIntervalTabs.Teams && !teamsQueryEnabled) {
        setTeamsQueryEnabled(true)
      }
    },
    [teamsQueryEnabled],
  )

  const handlePIChange = useCallback(
    (value: string | number) => {
      router.push(`/planning/planning-intervals/${value}`)
    },
    [router],
  )

  const piItems = useMemo(() => {
    if (!piListData) return []

    return [...piListData]
      .sort((a, b) => new Date(b.start).getTime() - new Date(a.start).getTime())
      .map((option) => ({
        label: option.name,
        extra: option.state.name,
        value: option.key,
      }))
  }, [piListData])

  const switchSprints = useMemo(() => {
    if (!piItems.length) return null

    return (
      <IconMenu
        icon={<SwapOutlined />}
        tooltip="Switch to another PI"
        items={piItems}
        selectedKeys={[piKey.toString()]}
        onChange={handlePIChange}
      />
    )
  }, [piItems, piKey, handlePIChange])

  if (isLoading) {
    return <PlanningIntervalDetailsLoading />
  }

  if (!isLoading && !planningIntervalData) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title="PI Details"
        tags={
          <Space>
            {switchSprints}
            <IterationStateTag
              state={planningIntervalData.state.id as IterationState}
            />
          </Space>
        }
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
      {openEditPlanningIntervalForm && (
        <EditPlanningIntervalForm
          showForm={openEditPlanningIntervalForm}
          planningIntervalKey={piKey}
          onFormUpdate={() => onEditFormClosed(true)}
          onFormCancel={() => onEditFormClosed(false)}
        />
      )}
      {openManagePlanningIntervalDatesForm && (
        <ManagePlanningIntervalDatesForm
          showForm={openManagePlanningIntervalDatesForm}
          id={planningIntervalData?.id}
          planningIntervalKey={piKey}
          onFormSave={() => onManagePlanningIntervalDatesFormClosed(true)}
          onFormCancel={() => onManagePlanningIntervalDatesFormClosed(false)}
        />
      )}
      {openManageTeamsForm && (
        <ManagePlanningIntervalTeamsForm
          showForm={openManageTeamsForm}
          id={planningIntervalData?.id}
          onFormSave={() => onManageTeamsFormClosed(true)}
          onFormCancel={() => onManageTeamsFormClosed(false)}
        />
      )}
    </>
  )
}

const PlanningIntervalDetailsPageWithAuthorization = authorizePage(
  PlanningIntervalDetailsPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default PlanningIntervalDetailsPageWithAuthorization
