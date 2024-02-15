'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Button, Card, Dropdown, MenuProps, Space } from 'antd'
import { createElement, useCallback, useEffect, useMemo, useState } from 'react'
import PlanningIntervalDetails from './planning-interval-details'
import TeamsGrid, {
  TeamsGridProps,
} from '@/src/app/components/common/organizations/teams-grid'
import RisksGrid, {
  RisksGridProps,
} from '@/src/app/components/common/planning/risks-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import ManagePlanningIntervalTeamsForm from './manage-planning-interval-teams-form'
import { DownOutlined } from '@ant-design/icons'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { EditPlanningIntervalForm } from '../../components'
import {
  useGetPlanningIntervalByKey,
  useGetPlanningIntervalRisks,
  useGetPlanningIntervalTeams,
} from '@/src/services/queries/planning-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/app/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import PlanningIntervalDetailsLoading from './loading'
import ManagePlanningIntervalDatesForm from './manage-planning-interval-dates-form'

enum PlanningIntervalTabs {
  Details = 'details',
  Teams = 'teams',
  Risks = 'risks',
}

const PlanningIntervalDetailsPage = ({ params }) => {
  useDocumentTitle('PI Details')
  const [activeTab, setActiveTab] = useState(PlanningIntervalTabs.Details)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)
  const [openEditPlanningIntervalForm, setOpenEditPlanningIntervalForm] =
    useState<boolean>(false)
  const [teamsQueryEnabled, setTeamsQueryEnabled] = useState<boolean>(false)
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [
    openManagePlanningIntervalDatesForm,
    setOpenManagePlanningIntervalDatesForm,
  ] = useState<boolean>(false)
  const [openManageTeamsForm, setOpenManageTeamsForm] = useState<boolean>(false)

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const { hasClaim } = useAuth()
  const canUpdatePlanningInterval = hasClaim(
    'Permission',
    'Permissions.PlanningIntervals.Update',
  )

  const {
    data: planningIntervalData,
    isLoading,
    isFetching,
    refetch: refetchPlanningInterval,
  } = useGetPlanningIntervalByKey(params.key)

  const teamsQuery = useGetPlanningIntervalTeams(
    planningIntervalData?.id,
    teamsQueryEnabled,
  )

  const risksQuery = useGetPlanningIntervalRisks(
    planningIntervalData?.id,
    includeClosedRisks,
    risksQueryEnabled,
  )

  const onIncludeClosedRisksChanged = useCallback((includeClosed: boolean) => {
    setIncludeClosedRisks(includeClosed)
  }, [])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
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

  const actions = () => {
    return (
      <>
        <Space>
          <Dropdown placement="bottomRight" menu={{ items: actionsMenuItems }}>
            <Button>
              <Space>
                Actions
                <DownOutlined />
              </Space>
            </Button>
          </Dropdown>
        </Space>
      </>
    )
  }

  const tabs = [
    {
      key: PlanningIntervalTabs.Details,
      tab: 'Details',
      content: (
        <PlanningIntervalDetails planningInterval={planningIntervalData} />
      ),
    },
    {
      key: PlanningIntervalTabs.Teams,
      tab: 'Teams',
      content: createElement(TeamsGrid, {
        teamsQuery: teamsQuery,
      } as TeamsGridProps),
    },
    {
      key: PlanningIntervalTabs.Risks,
      tab: 'Risk Management',
      content: createElement(RisksGrid, {
        risksQuery: risksQuery,
        updateIncludeClosed: onIncludeClosedRisksChanged,
        getRisksObjectId: planningIntervalData?.id,
        newRisksAllowed: true,
      } as RisksGridProps),
    },
  ]

  useEffect(() => {
    planningIntervalData &&
      dispatch(
        setBreadcrumbTitle({ title: planningIntervalData.name, pathname }),
      )
  }, [dispatch, pathname, planningIntervalData])

  const onEditFormClosed = useCallback((wasSaved: boolean) => {
    setOpenEditPlanningIntervalForm(false)
    if (wasSaved) {
      refetchPlanningInterval()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const onManagePlanningIntervalDatesFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenManagePlanningIntervalDatesForm(false)
      if (wasSaved) {
        refetchPlanningInterval()
      }
    },
    [refetchPlanningInterval],
  )

  const onManageTeamsFormClosed = useCallback((wasSaved: boolean) => {
    setOpenManageTeamsForm(false)
    if (wasSaved) {
      refetchPlanningInterval()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // doesn't trigger on first render
  const onTabChange = useCallback(
    (tabKey) => {
      setActiveTab(tabKey)

      // enables the query for the tab on first render if it hasn't been enabled yet
      if (tabKey == PlanningIntervalTabs.Teams && !teamsQueryEnabled) {
        setTeamsQueryEnabled(true)
      } else if (tabKey == PlanningIntervalTabs.Risks && !risksQueryEnabled) {
        setRisksQueryEnabled(true)
      }
    },
    [risksQueryEnabled, teamsQueryEnabled],
  )

  if (isLoading) {
    return <PlanningIntervalDetailsLoading />
  }

  if (!isLoading && !isFetching && !planningIntervalData) {
    notFound()
  }

  return (
    <>
      <PageTitle title="PI Details" actions={actions()} />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {openEditPlanningIntervalForm && (
        <EditPlanningIntervalForm
          showForm={openEditPlanningIntervalForm}
          id={planningIntervalData?.id}
          onFormUpdate={() => onEditFormClosed(true)}
          onFormCancel={() => onEditFormClosed(false)}
        />
      )}
      {openManagePlanningIntervalDatesForm && (
        <ManagePlanningIntervalDatesForm
          showForm={openManagePlanningIntervalDatesForm}
          id={planningIntervalData?.id}
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
