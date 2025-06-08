'use client'

import PageTitle from '@/src/components/common/page-title'
import { Card, MenuProps } from 'antd'
import {
  createElement,
  use,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import TeamDetails from './team-details'
import RisksGrid, {
  RisksGridProps,
} from '@/src/components/common/planning/risks-grid'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import {
  CreateTeamMembershipForm,
  EditTeamForm,
  TeamMembershipsGrid,
} from '../../_components'
import useAuth from '@/src/components/contexts/auth'
import {
  useGetTeamMembershipsQuery,
  useGetTeamRisksQuery,
} from '@/src/store/features/organizations/team-api'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import {
  retrieveTeam,
  setEditMode,
  selectEditTeamContext,
} from '../../../../store/features/organizations/team-slice'
import { useAppDispatch, useAppSelector } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { WorkItemsBacklogGrid } from '@/src/components/common/work'
import { useGetTeamBacklogQuery } from '@/src/store/features/organizations/team-api'
import { WorkItemsBacklogGridProps } from '@/src/components/common/work/work-items-backlog-grid'
import TeamDependencyManagement from './team-dependency-management'
import { ItemType } from 'antd/es/menu/interface'
import { InactiveTag, PageActions } from '@/src/components/common'
import DeactivateTeamForm from '../../_components/deactivate-team-form'

enum TeamTabs {
  Details = 'details',
  Backlog = 'backlog',
  DependencyManagement = 'dependency-management',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const tabs = [
  {
    key: TeamTabs.Details,
    tab: 'Details',
  },
  {
    key: TeamTabs.Backlog,
    tab: 'Backlog',
  },
  {
    key: TeamTabs.DependencyManagement,
    tab: 'Dependency Management',
  },
  {
    key: TeamTabs.RiskManagement,
    tab: 'Risk Management',
  },
  {
    key: TeamTabs.TeamMemberships,
    tab: 'Team Memberships',
  },
]

const TeamDetailsPage = (props: { params: Promise<{ key: number }> }) => {
  const { key } = use(props.params)

  useDocumentTitle('Team Details')

  const [activeTab, setActiveTab] = useState(TeamTabs.Details)
  const [openCreateTeamMembershipForm, setOpenCreateTeamMembershipForm] =
    useState<boolean>(false)
  const [openDeactivateTeamForm, setOpenDeactivateTeamForm] =
    useState<boolean>(false)
  const [teamMembershipsQueryEnabled, setTeamMembershipsQueryEnabled] =
    useState<boolean>(false)
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const { hasPermissionClaim } = useAuth()
  const canUpdateTeam = hasPermissionClaim('Permissions.Teams.Update')
  const canManageTeamMemberships = hasPermissionClaim(
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
  const teamMembershipsQuery = useGetTeamMembershipsQuery(
    { teamId: team?.id, enabled: teamMembershipsQueryEnabled },
    { skip: !team?.id || !teamMembershipsQueryEnabled },
  )

  const backlogQuery = useGetTeamBacklogQuery(team?.id, { skip: !team?.id })

  const risksQuery = useGetTeamRisksQuery(
    {
      id: team?.id,
      includeClosed: includeClosedRisks,
      enabled: risksQueryEnabled,
    },
    { skip: !team?.id || !risksQueryEnabled },
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
  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case TeamTabs.Details:
        return <TeamDetails team={team} />
      case TeamTabs.Backlog:
        return createElement(WorkItemsBacklogGrid, {
          workItems: backlogQuery.data,
          hideTeamColumn: true,
          isLoading: backlogQuery.isLoading,
          refetch: backlogQuery.refetch,
        } as WorkItemsBacklogGridProps)
      case TeamTabs.DependencyManagement:
        return <TeamDependencyManagement team={team} />
      case TeamTabs.RiskManagement:
        return createElement(RisksGrid, {
          risks: risksQuery.data,
          updateIncludeClosed: onIncludeClosedRisksChanged,
          isLoadingRisks: risksQuery.isLoading,
          refreshRisks: risksQuery.refetch,
          newRisksAllowed: true,
          teamId: team?.id,
          hideTeamColumn: true,
        } as RisksGridProps)
      case TeamTabs.TeamMemberships:
        return createElement(TeamMembershipsGrid, {
          teamId: team?.id,
          teamMemberships: teamMembershipsQuery.data,
          isLoading: teamMembershipsQuery.isLoading,
          refetch: teamMembershipsQuery.refetch,
          teamType: 'Team',
        })
      default:
        return null
    }
  }, [
    activeTab,
    team,
    risksQuery,
    teamMembershipsQuery,
    backlogQuery,
    onIncludeClosedRisksChanged,
  ])

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
    (tabKey: string) => {
      setActiveTab(tabKey as TeamTabs)

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
        {renderTabContent()}
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
