'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Card, MenuProps } from 'antd'
import { createElement, useCallback, useEffect, useMemo, useState } from 'react'
import PlanningIntervalDetails from './planning-interval-details'
import TeamsGrid, {
  TeamsGridProps,
} from '@/src/app/components/common/organizations/teams-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import ManagePlanningIntervalTeamsForm from './manage-planning-interval-teams-form'
import { EditPlanningIntervalForm } from '../../components'
import {
  useGetPlanningIntervalByKey,
  useGetPlanningIntervalTeams,
} from '@/src/services/queries/planning-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/app/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import PlanningIntervalDetailsLoading from './loading'
import ManagePlanningIntervalDatesForm from './manage-planning-interval-dates-form'
import { PageActions } from '@/src/app/components/common'

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
  } = useGetPlanningIntervalByKey(params.key)

  const teamsQuery = useGetPlanningIntervalTeams(
    planningIntervalData?.id,
    teamsQueryEnabled,
  )

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: MenuProps['items'] = []
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
