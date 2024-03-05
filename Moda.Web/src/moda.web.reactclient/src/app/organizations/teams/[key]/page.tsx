'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Button, Card } from 'antd'
import { createElement, useCallback, useEffect, useState } from 'react'
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

enum TeamTabs {
  Details = 'details',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const TeamDetailsPage = ({ params }) => {
  useDocumentTitle('Team Details')
  const { key } = params
  const [activeTab, setActiveTab] = useState(TeamTabs.Details)
  const [openCreateTeamMembershipForm, setOpenCreateTeamMembershipForm] =
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
  const showActions = canUpdateTeam || canManageTeamMemberships

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

  const risksQuery = useGetTeamRisks(
    team?.id,
    includeClosedRisks,
    risksQueryEnabled,
  )

  const onIncludeClosedRisksChanged = useCallback((includeClosed: boolean) => {
    setIncludeClosedRisks(includeClosed)
  }, [])

  const actions = () => {
    return (
      <>
        {canUpdateTeam && (
          <Button onClick={() => dispatch(setEditMode(true))}>Edit Team</Button>
        )}
        {canManageTeamMemberships && (
          <Button onClick={() => setOpenCreateTeamMembershipForm(true)}>
            Add Team Membership
          </Button>
        )}
      </>
    )
  }

  const tabs = [
    {
      key: TeamTabs.Details,
      tab: 'Details',
      content: <TeamDetails team={team} />,
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

  if (teamNotFound) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={team?.name}
        subtitle="Team Details"
        actions={showActions && actions()}
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
    </>
  )
}

const TeamDetailsPageWithAuthorization = authorizePage(
  TeamDetailsPage,
  'Permission',
  'Permissions.Teams.View',
)

export default TeamDetailsPageWithAuthorization
