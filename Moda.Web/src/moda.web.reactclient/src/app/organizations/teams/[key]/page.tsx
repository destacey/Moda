'use client'

import PageTitle from '@/src/components/common/page-title'
import { Card, MenuProps } from 'antd'
import { CloseOutlined } from '@ant-design/icons'
import {
  createElement,
  use,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import TeamDetails from '../_components/team-details'
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
import TeamDependencyManagement from '../_components/team-dependency-management'
import { ItemType } from 'antd/es/menu/interface'
import { InactiveTag, PageActions } from '@/src/components/common'
import DeactivateTeamForm from '../../_components/deactivate-team-form'
import TeamSprints from '../_components/team-sprints'
import { CycleTimeReport } from '@/src/components/common/work/cycle-time-report'
import TeamBacklog from '../_components/team-backlog'

enum TeamTabs {
  Details = 'details',
  Backlog = 'backlog',
  Sprints = 'sprints',
  DependencyManagement = 'dependency-management',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
  CycleTimeReport = 'cycle-time-report',
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
    key: TeamTabs.Sprints,
    tab: 'Sprints',
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

const TeamDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key } = use(props.params)
  const teamKey = Number(key)

  const [activeTab, setActiveTab] = useState<TeamTabs>(TeamTabs.Details)
  const [openCreateTeamMembershipForm, setOpenCreateTeamMembershipForm] =
    useState<boolean>(false)
  const [openDeactivateTeamForm, setOpenDeactivateTeamForm] =
    useState<boolean>(false)
  const [teamMembershipsQueryEnabled, setTeamMembershipsQueryEnabled] =
    useState<boolean>(false)
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)
  const [dynamicTabs, setDynamicTabs] = useState<
    Array<{ key: string; tab: string; closable: boolean }>
  >([])

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

  useDocumentTitle(`${team?.code ?? teamKey} - Team Details`)

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

  const openCycleTimeReport = useCallback(() => {
    // Check if the cycle time report tab is already open
    const cycleTimeTabExists = dynamicTabs.some(
      (tab) => tab.key === TeamTabs.CycleTimeReport,
    )

    if (!cycleTimeTabExists) {
      // Add the tab to the end
      setDynamicTabs((prevTabs) => [
        ...prevTabs,
        {
          key: TeamTabs.CycleTimeReport,
          tab: 'Cycle Time Report',
          closable: true,
        },
      ])
    }

    // Switch to the cycle time report tab
    setActiveTab(TeamTabs.CycleTimeReport)
  }, [dynamicTabs])

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

    if (items.length > 0) {
      items.push({
        type: 'divider',
      })
    }

    items.push({
      type: 'group',
      label: 'Reports',
      children: [
        {
          key: 'cycle-time-report',
          label: 'Cycle Time Report',
          onClick: openCycleTimeReport,
        },
      ],
    })

    return items
  }, [
    canManageTeamMemberships,
    canUpdateTeam,
    dispatch,
    team?.isActive,
    openCycleTimeReport,
  ])
  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case TeamTabs.Details:
        return <TeamDetails team={team} />
      case TeamTabs.Backlog:
        return <TeamBacklog teamId={team?.id} />
      case TeamTabs.Sprints:
        return <TeamSprints teamId={team?.id} />
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
      case TeamTabs.CycleTimeReport:
        return <CycleTimeReport teamCode={team?.code} />
      default:
        return null
    }
  }, [
    activeTab,
    team,
    risksQuery,
    teamMembershipsQuery,
    onIncludeClosedRisksChanged,
  ])

  useEffect(() => {
    dispatch(retrieveTeam({ key: teamKey, type: 'Team' }))
  }, [teamKey, dispatch])

  useEffect(() => {
    team && dispatch(setBreadcrumbTitle({ title: team.name, pathname }))
  }, [team, dispatch, pathname])

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

  const closeTab = useCallback(
    (tabKey: string, e: React.MouseEvent) => {
      e.stopPropagation()
      // Remove the tab from dynamicTabs
      setDynamicTabs((prevTabs) => prevTabs.filter((tab) => tab.key !== tabKey))

      // If the active tab is being closed, switch to Details tab
      if (activeTab === tabKey) {
        setActiveTab(TeamTabs.Details)
      }
    },
    [activeTab],
  )

  const allTabs = useMemo(() => {
    const staticTabs = tabs.map((tab) => ({
      key: tab.key,
      tab: tab.tab,
    }))

    const closableTabs = dynamicTabs.map((tab) => ({
      key: tab.key,
      tab: (
        <span>
          {tab.tab}
          <CloseOutlined
            style={{ marginLeft: 8 }}
            onClick={(e) => closeTab(tab.key, e)}
          />
        </span>
      ),
    }))

    return [...staticTabs, ...closableTabs]
  }, [dynamicTabs, closeTab])

  const onCreateTeamMembershipFormClosed = (wasSaved: boolean) => {
    setOpenCreateTeamMembershipForm(false)
    if (wasSaved) {
      dispatch(retrieveTeam({ key: teamKey, type: 'Team' }))
    }
  }

  const onDeactivateTeamFormClosed = (wasSaved: boolean) => {
    setOpenDeactivateTeamForm(false)
    if (wasSaved) {
      dispatch(retrieveTeam({ key: teamKey, type: 'Team' }))
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
        tabList={allTabs}
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
