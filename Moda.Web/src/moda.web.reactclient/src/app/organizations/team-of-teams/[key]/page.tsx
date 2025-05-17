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
import TeamOfTeamsDetails from './team-of-teams-details'
import RisksGrid, {
  RisksGridProps,
} from '@/src/components/common/planning/risks-grid'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import { EditTeamForm, TeamMembershipsGrid } from '../../_components'
import useAuth from '@/src/components/contexts/auth'
import {
  useGetTeamOfTeamsMemberships,
  useGetTeamOfTeamsRisks,
} from '@/src/services/queries/organization-queries'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import {
  retrieveTeam,
  setEditMode,
  selectTeamContext,
} from '../../../../store/features/organizations/team-slice'
import { useAppDispatch, useAppSelector } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { CreateTeamMembershipForm } from '../../_components'
import { InactiveTag, PageActions } from '@/src/components/common'
import { ItemType } from 'antd/es/menu/interface'
import DeactivateTeamOfTeamsForm from '../../_components/deactivate-team-of-teams-form'

enum TeamOfTeamsTabs {
  Details = 'details',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const TeamOfTeamsDetailsPage = (props: {
  params: Promise<{ key: number }>
}) => {
  const { key: teamKey } = use(props.params)

  useDocumentTitle('Team of Teams Details')

  const [activeTab, setActiveTab] = useState(TeamOfTeamsTabs.Details)
  const [openCreateTeamMembershipForm, setOpenCreateTeamMembershipForm] =
    useState<boolean>(false)
  const [openDeactivateTeamForm, setOpenDeactivateTeamForm] =
    useState<boolean>(false)
  const [teamMembershipsQueryEnabled, setTeamMembershipsQueryEnabled] =
    useState<boolean>(false)
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Teams.ManageTeamMemberships',
  )

  const {
    item: team,
    isInEditMode,
    notFound: teamNotFound,
    error,
  } = useAppSelector(selectTeamContext)
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const teamMembershipsQuery = useGetTeamOfTeamsMemberships(
    team?.id,
    teamMembershipsQueryEnabled,
  )

  const risksQuery = useGetTeamOfTeamsRisks(
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
      key: TeamOfTeamsTabs.Details,
      tab: 'Details',
      content: <TeamOfTeamsDetails team={team} />,
    },
    {
      key: TeamOfTeamsTabs.RiskManagement,
      tab: 'Risk Management',
      content: createElement(RisksGrid, {
        risks: risksQuery.data,
        updateIncludeClosed: onIncludeClosedRisksChanged,
        isLoadingRisks: risksQuery.isLoading,
        refreshRisks: risksQuery.refetch,
        newRisksAllowed: true,
        teamId: team?.id,
        hideTeamColumn: true,
      } as RisksGridProps),
    },
    {
      key: TeamOfTeamsTabs.TeamMemberships,
      tab: 'Team Memberships',
      content: createElement(TeamMembershipsGrid, {
        teamId: team?.id,
        teamMembershipsQuery: teamMembershipsQuery,
        teamType: 'Team of Teams',
      }),
    },
  ]

  useEffect(() => {
    dispatch(retrieveTeam({ key: teamKey, type: 'Team of Teams' }))
  }, [teamKey, dispatch])

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
      if (tabKey == TeamOfTeamsTabs.RiskManagement && !risksQueryEnabled) {
        setRisksQueryEnabled(true)
      } else if (
        tabKey == TeamOfTeamsTabs.TeamMemberships &&
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
      dispatch(retrieveTeam({ key: teamKey, type: 'Team of Teams' }))
    }
  }

  const onDeactivateTeamFormClosed = (wasSaved: boolean) => {
    setOpenDeactivateTeamForm(false)
    if (wasSaved) {
      dispatch(retrieveTeam({ key: teamKey, type: 'Team of Teams' }))
    }
  }

  if (teamNotFound) {
    return notFound()
  }

  const teamName = !team
    ? null
    : team.isActive
      ? team?.name
      : `${team?.name} (Inactive)`

  return (
    <>
      <PageTitle
        title={team?.name}
        subtitle="Team of Teams Details"
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
          teamType={'Team of Teams'}
          onFormCreate={() => onCreateTeamMembershipFormClosed(true)}
          onFormCancel={() => onCreateTeamMembershipFormClosed(false)}
        />
      )}
      {openDeactivateTeamForm && (
        <DeactivateTeamOfTeamsForm
          team={team}
          showForm={openDeactivateTeamForm}
          onFormComplete={() => onDeactivateTeamFormClosed(true)}
          onFormCancel={() => onDeactivateTeamFormClosed(false)}
        />
      )}
    </>
  )
}

const TeamOfTeamsDetailsPageWithAuthorization = authorizePage(
  TeamOfTeamsDetailsPage,
  'Permission',
  'Permissions.Teams.View',
)

export default TeamOfTeamsDetailsPageWithAuthorization
