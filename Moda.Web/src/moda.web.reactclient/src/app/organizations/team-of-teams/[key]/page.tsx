'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Button, Card } from 'antd'
import { createElement, useCallback, useEffect, useState } from 'react'
import TeamOfTeamsDetails from './team-of-teams-details'
import { getTeamsOfTeamsClient } from '@/src/services/clients'
import RisksGrid, {
  RisksGridProps,
} from '@/src/app/components/common/planning/risks-grid'
import TeamMembershipsGrid from '@/src/app/components/common/organizations/team-memberships-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import { EditTeamForm } from '../../components'
import useAuth from '@/src/app/components/contexts/auth'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import { useGetTeamOfTeamsRisks } from '@/src/services/queries/organization-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound } from 'next/navigation'
import { resetActiveTeam, retrieveTeam, setEditTeamOpen, useTeamDetail } from '../../teamsSlice'
import { useAppDispatch } from '@/src/app/hooks'

enum TeamOfTeamsTabs {
  Details = 'details',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const TeamOfTeamsDetailsPage = ({ params }) => {
  useDocumentTitle('Team of Teams Details')
  const [activeTab, setActiveTab] = useState(TeamOfTeamsTabs.Details)
  const { key } = params
  const { setBreadcrumbTitle } = useBreadcrumbs()
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)
  const [notTeamFound, setTeamNotFound] = useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const showActions = canUpdateTeam

  const {team, error, isEditOpen} = useTeamDetail()
  const dispatch = useAppDispatch()

  const risksQuery = useGetTeamOfTeamsRisks(
    team?.id,
    includeClosedRisks,
    risksQueryEnabled,
  )

  const onIncludeClosedRisksChanged = useCallback((includeClosed: boolean) => {
    setIncludeClosedRisks(includeClosed)
  }, [])

  const getTeamMemberships = useCallback(async (teamId: string) => {
    const teamOfTeamsClient = await getTeamsOfTeamsClient()
    return await teamOfTeamsClient.getTeamMemberships(teamId)
  }, [])

  const Actions = () => {
    return (
      <>
        {canUpdateTeam && (
          <Button onClick={() => dispatch(setEditTeamOpen(true))}>Edit Team</Button>
        )}
      </>
    )
  }

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
        risksQuery: risksQuery,
        updateIncludeClosed: onIncludeClosedRisksChanged,
        newRisksAllowed: true,
        teamId: team?.id,
        hideTeamColumn: true,
      } as RisksGridProps),
    },
    {
      key: TeamOfTeamsTabs.TeamMemberships,
      tab: 'Team Memberships',
      content: createElement(TeamMembershipsGrid, {
        getTeamMemberships: getTeamMemberships,
        getTeamMembershipsObjectId: team?.id,
      }),
    },
  ]

  useEffect(() => {
    dispatch(resetActiveTeam)
    dispatch(retrieveTeam({key, type: 'Team of Teams'}))
  }, [key, dispatch])

  useEffect(() => {
    team && setBreadcrumbTitle(team.name)
  }, [team, setBreadcrumbTitle])

  // doesn't trigger on first render
  const onTabChange = useCallback(
    (key) => {
      setActiveTab(key)

      // enables the query for the tab on first render if it hasn't been enabled yet
      if (key == TeamOfTeamsTabs.RiskManagement && !risksQueryEnabled) {
        setRisksQueryEnabled(true)
      }
    },
    [risksQueryEnabled],
  )

  if (notTeamFound) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={team?.name}
        subtitle="Team of Teams Details"
        actions={showActions && <Actions />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {isEditOpen && team && canUpdateTeam && (
        <EditTeamForm team={team} />
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
