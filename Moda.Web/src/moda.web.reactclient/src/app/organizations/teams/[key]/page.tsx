'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Card, MenuProps, message } from 'antd'
import { createElement, useCallback, useEffect, useMemo, useState } from 'react'
import TeamDetails from './team-details'
import RisksGrid, {
  RisksGridProps,
} from '@/src/app/components/common/planning/risks-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import {
  CreateTeamMembershipForm,
  EditTeamForm,
  TeamMembershipsGrid,
} from '../../components'
import useAuth from '@/src/app/components/contexts/auth'
import {
  useGetTeamMemberships,
  useGetTeamRisks,
} from '@/src/services/queries/organization-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import {
  retrieveTeam,
  setEditMode,
  selectEditTeamContext,
} from '../../../../store/features/organizations/team-slice'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { WorkItemsBacklogGrid } from '@/src/app/components/common/work'
import { useGetTeamBacklogQuery } from '@/src/store/features/organizations/team-api'
import { WorkItemsBacklogGridProps } from '@/src/app/components/common/work/work-items-backlog-grid'
import TeamDependencyManagement from './team-dependency-management'
import { ItemType } from 'antd/es/menu/interface'
import { InactiveTag, PageActions } from '@/src/app/components/common'
import DeactivateTeamForm from '../../components/deactivate-team-form'

enum TeamTabs {
  Details = 'details',
  Backlog = 'backlog',
  DependencyManagement = 'dependency-management',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const TeamDetailsPage = ({ params }) => {
  useDocumentTitle('Team Details')
  const { key } = params
  const [activeTab, setActiveTab] = useState(TeamTabs.Details)
  const [openCreateTeamMembershipForm, setOpenCreateTeamMembershipForm] =
    useState<boolean>(false)
  const [openDeactivateTeamForm, setOpenDeactivateTeamForm] =
    useState<boolean>(false)
  const [teamMembershipsQueryEnabled, setTeamMembershipsQueryEnabled] =
    useState<boolean>(false)
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Teams.ManageTeamMemberships',
  )

  const {
    item: team,
    error,
    isInEditMode,
    notFound: teamNotFound,
  } = useAppSelector(selectEditTeamContext)
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const teamMembershipsQuery = useGetTeamMemberships(
    team?.id,
    teamMembershipsQueryEnabled,
  )

  const backlogQuery = useGetTeamBacklogQuery(team?.id, { skip: !team?.id })

  const risksQuery = useGetTeamRisks(
    team?.id,
    includeClosedRisks,
    risksQueryEnabled,
  )

  const onIncludeClosedRisksChanged = useCallback((includeClosed: boolean) => {
    setIncludeClosedRisks(includeClosed)
  }, [])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []

    if (canUpdateTeam) {
      items.push({
        key: 'edit',
        label: 'Edit',
        onClick: () => dispatch(setEditMode(true)),
      })

      if (team?.isActive === true) {
        items.push({
          key: 'deactivate',
          label: 'Deactivate',
          onClick: () => setOpenDeactivateTeamForm(true),
        })
      }
    }

    if (canManageTeamMemberships && team?.isActive === true) {
      items.push({
        key: 'add-team-membership',
        label: 'Add Team Membership',
        onClick: () => setOpenCreateTeamMembershipForm(true),
      })
    }

    return items
  }, [canManageTeamMemberships, canUpdateTeam, dispatch, team?.isActive])

  const tabs = [
    {
      key: TeamTabs.Details,
      tab: 'Details',
      content: <TeamDetails team={team} />,
    },
    {
      key: TeamTabs.Backlog,
      tab: 'Backlog',
      content: createElement(WorkItemsBacklogGrid, {
        workItems: backlogQuery.data,
        hideTeamColumn: true,
        isLoading: backlogQuery.isLoading,
        refetch: backlogQuery.refetch,
      } as WorkItemsBacklogGridProps),
    },
    {
      key: TeamTabs.DependencyManagement,
      tab: 'Dependency Management',
      content: <TeamDependencyManagement team={team} />,
    },
    {
      key: TeamTabs.RiskManagement,
      tab: 'Risk Management',
      content: createElement(RisksGrid, {
        risksQuery: risksQuery,
        updateIncludeClosed: onIncludeClosedRisksChanged,
        newRisksAllowed: true,
        teamId: team?.id,
        hideTeamColumn: true,
      } as RisksGridProps),
    },
    {
      key: TeamTabs.TeamMemberships,
      tab: 'Team Memberships',
      content: createElement(TeamMembershipsGrid, {
        teamId: team?.id,
        teamMembershipsQuery: teamMembershipsQuery,
        teamType: 'Team',
      }),
    },
  ]

  useEffect(() => {
    dispatch(retrieveTeam({ key, type: 'Team' }))
  }, [key, dispatch])

  useEffect(() => {
    team && dispatch(setBreadcrumbTitle({ title: team.name, pathname }))
  }, [team, dispatch, pathname])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  // doesn't trigger on first render
  const onTabChange = useCallback(
    (tabKey) => {
      setActiveTab(tabKey)

      // enables the query for the tab on first render if it hasn't been enabled yet
      if (tabKey == TeamTabs.RiskManagement && !risksQueryEnabled) {
        setRisksQueryEnabled(true)
      } else if (
        tabKey == TeamTabs.TeamMemberships &&
        !teamMembershipsQueryEnabled
      ) {
        setTeamMembershipsQueryEnabled(true)
      }
    },
    [risksQueryEnabled, teamMembershipsQueryEnabled],
  )

  const onCreateTeamMembershipFormClosed = (wasSaved: boolean) => {
    setOpenCreateTeamMembershipForm(false)
    if (wasSaved) {
      dispatch(retrieveTeam({ key, type: 'Team' }))
    }
  }

  const onDeactivateTeamFormClosed = (wasSaved: boolean) => {
    setOpenDeactivateTeamForm(false)
    if (wasSaved) {
      dispatch(retrieveTeam({ key, type: 'Team' }))
    }
  }

  if (teamNotFound) {
    return notFound()
  }

  return (
    <>
      {contextHolder}
      <PageTitle
        title={team?.name}
        subtitle="Team Details"
        tags={<InactiveTag isActive={team?.isActive} />}
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
      {isInEditMode && team && canUpdateTeam && <EditTeamForm team={team} />}
      {openCreateTeamMembershipForm && (
        <CreateTeamMembershipForm
          showForm={openCreateTeamMembershipForm}
          teamId={team?.id}
          teamType={'Team'}
          onFormCreate={() => onCreateTeamMembershipFormClosed(true)}
          onFormCancel={() => onCreateTeamMembershipFormClosed(false)}
        />
      )}
      {openDeactivateTeamForm && (
        <DeactivateTeamForm
          team={team}
          showForm={openDeactivateTeamForm}
          onFormComplete={() => onDeactivateTeamFormClosed(true)}
          onFormCancel={() => onDeactivateTeamFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

const TeamDetailsPageWithAuthorization = authorizePage(
  TeamDetailsPage,
  'Permission',
  'Permissions.Teams.View',
)

export default TeamDetailsPageWithAuthorization
