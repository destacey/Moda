'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Button, Card } from 'antd'
import { createElement, useCallback, useEffect, useState } from 'react'
import TeamDetails from './team-details'
import { getTeamsClient } from '@/src/services/clients'
import RisksGrid, {
  RisksGridProps,
} from '@/src/app/components/common/planning/risks-grid'
import TeamMembershipsGrid from '@/src/app/components/common/organizations/team-memberships-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import { EditTeamForm } from '../../components'
import useAuth from '@/src/app/components/contexts/auth'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import { useGetTeamRisks } from '@/src/services/queries/organization-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound } from 'next/navigation'
import { resetActiveTeam, retrieveTeam, setEditTeamOpen, selectTeamDetail } from '../../teams-slice'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'

enum TeamTabs {
  Details = 'details',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const TeamDetailsPage = ({ params }) => {
  useDocumentTitle('Team Details')
  const [activeTab, setActiveTab] = useState(TeamTabs.Details)
  const { key } = params
  const { setBreadcrumbTitle } = useBreadcrumbs()
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const showActions = canUpdateTeam

  const { team, error, isEditOpen, teamNotFound } = useAppSelector(selectTeamDetail)
  const dispatch = useAppDispatch()

  const risksQuery = useGetTeamRisks(
    team?.id,
    includeClosedRisks,
    risksQueryEnabled,
  )

  const onIncludeClosedRisksChanged = useCallback((includeClosed: boolean) => {
    setIncludeClosedRisks(includeClosed)
  }, [])

  const getTeamMemberships = useCallback(async (teamId: string) => {
    const teamsClient = await getTeamsClient()
    return await teamsClient.getTeamMemberships(teamId)
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
        getRisksObjectId: team?.id,
        newRisksAllowed: true,
        teamId: team?.id,
        hideTeamColumn: true,
      } as RisksGridProps),
    },
    {
      key: TeamTabs.TeamMemberships,
      tab: 'Team Memberships',
      content: createElement(TeamMembershipsGrid, {
        getTeamMemberships: getTeamMemberships,
        getTeamMembershipsObjectId: team?.id,
      }),
    },
  ]

  useEffect(() => {
    dispatch(resetActiveTeam)
    dispatch(retrieveTeam({key, type: 'Team'}))
  }, [key, dispatch])

  useEffect(() => {
    team && setBreadcrumbTitle(team.name)
  }, [team, setBreadcrumbTitle])

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
      }
    },
    [risksQueryEnabled],
  )

  if (teamNotFound) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={team?.name}
        subtitle="Team Details"
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

const TeamDetailsPageWithAuthorization = authorizePage(
  TeamDetailsPage,
  'Permission',
  'Permissions.Teams.View',
)

export default TeamDetailsPageWithAuthorization
