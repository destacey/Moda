'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { TeamOfTeamsDetailsDto } from '@/src/services/moda-api'
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

enum TeamOfTeamsTabs {
  Details = 'details',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const TeamOfTeamsDetailsPage = ({ params }) => {
  useDocumentTitle('Team of Teams Details')
  const [activeTab, setActiveTab] = useState(TeamOfTeamsTabs.Details)
  const [team, setTeam] = useState<TeamOfTeamsDetailsDto | null>(null)
  const [openUpdateTeamForm, setOpenUpdateTeamForm] = useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())
  const { id } = params
  const { setBreadcrumbTitle } = useBreadcrumbs()
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const showActions = canUpdateTeam

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
          <Button onClick={() => setOpenUpdateTeamForm(true)}>Edit Team</Button>
        )}
      </>
    )
  }

  const tabs = [
    {
      key: TeamOfTeamsTabs.Details,
      tab: 'Details',
      content: createElement(TeamOfTeamsDetails, team),
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
    const getTeam = async () => {
      const teamsOfTeamsClient = await getTeamsOfTeamsClient()
      const teamDto = await teamsOfTeamsClient.getById(id)
      setTeam(teamDto)
      setBreadcrumbTitle(teamDto.name)
    }

    getTeam()
  }, [id, setBreadcrumbTitle, lastRefresh])

  const onUpdateTeamFormClosed = (wasUpdated: boolean) => {
    setOpenUpdateTeamForm(false)
    if (wasUpdated) {
      setLastRefresh(Date.now())
    }
  }

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
      {openUpdateTeamForm && team && canUpdateTeam && (
        <EditTeamForm
          showForm={openUpdateTeamForm}
          localId={team.localId}
          type={team.type}
          onFormUpdate={() => onUpdateTeamFormClosed(true)}
          onFormCancel={() => onUpdateTeamFormClosed(false)}
        />
      )}
    </>
  )
}

export default TeamOfTeamsDetailsPage
