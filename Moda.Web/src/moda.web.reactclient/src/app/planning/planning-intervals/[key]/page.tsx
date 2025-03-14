'use client'

import PageTitle from '@/src/components/common/page-title'
import { Card } from 'antd'
import { createElement, useCallback, useEffect, useMemo, useState } from 'react'
import TeamsGrid, {
  TeamsGridProps,
} from '@/src/components/common/organizations/teams-grid'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import useAuth from '@/src/components/contexts/auth'
import {
  useGetPlanningInterval,
  useGetPlanningIntervalTeams,
} from '@/src/services/queries/planning-queries'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import PlanningIntervalDetailsLoading from './loading'
import { PageActions } from '@/src/components/common'
import { ItemType } from 'antd/es/menu/interface'
import {
  EditPlanningIntervalForm,
  ManagePlanningIntervalDatesForm,
  ManagePlanningIntervalTeamsForm,
  PlanningIntervalDetails,
} from '../_components'

enum PlanningIntervalTabs {
  Details = 'details',
  Teams = 'teams',
}

const PlanningIntervalDetailsPage = ({ params }) => {
  useDocumentTitle('PI Details')
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
  } = useGetPlanningInterval(params.key)

  const teamsQuery = useGetPlanningIntervalTeams(
    planningIntervalData?.id,
    teamsQueryEnabled,
  )

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
      }
    },
    [teamsQueryEnabled],
  )

  if (isLoading) {
    return <PlanningIntervalDetailsLoading />
  }

  if (!isLoading && !isFetching && !planningIntervalData) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title="PI Details"
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
