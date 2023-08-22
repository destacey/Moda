'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { TeamDetailsDto, TeamMembershipsDto } from '@/src/services/moda-api'
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

enum TeamTabs {
  Details = 'details',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const TeamDetailsPage = ({ params }) => {
  useDocumentTitle('Team Details')
  const [activeTab, setActiveTab] = useState(TeamTabs.Details)
  const [team, setTeam] = useState<TeamDetailsDto | null>(null)
  const [openUpdateTeamForm, setOpenUpdateTeamForm] = useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())
  const { key } = params
  const { setBreadcrumbTitle } = useBreadcrumbs()
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const showActions = canUpdateTeam

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
          <Button onClick={() => setOpenUpdateTeamForm(true)}>Edit Team</Button>
        )}
      </>
    )
  }

  const tabs = [
    {
      key: TeamTabs.Details,
      tab: 'Details',
      content: createElement(TeamDetails, team),
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
    const getTeam = async () => {
      const teamsClient = await getTeamsClient()
      const teamDto = await teamsClient.getById(key)
      setTeam(teamDto)
      setBreadcrumbTitle(teamDto.name)
    }

    getTeam()
  }, [key, setBreadcrumbTitle, lastRefresh])

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
      if (key == TeamTabs.RiskManagement && !risksQueryEnabled) {
        setRisksQueryEnabled(true)
      }
    },
    [risksQueryEnabled],
  )

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

export default TeamDetailsPage
