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
import EditTeamForm from '../../components/edit-team'
import useAuth from '@/src/app/components/contexts/auth'
import useBreadcrumb from '@/src/app/components/contexts/breadcrumbs'

const TeamDetailsPage = ({ params }) => {
  useDocumentTitle('Team Details')
  const [activeTab, setActiveTab] = useState('details')
  const [team, setTeam] = useState<TeamDetailsDto | null>(null)
  const [teamMemberships, setTeamMemberships] = useState<TeamMembershipsDto[]>(
    []
  )
  const [openUpdateTeamModal, setOpenUpdateTeamModal] = useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())
  const { id } = params
  const { setBreadcrumbTitle } = useBreadcrumb()

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const showActions = canUpdateTeam

  const getRisks = useCallback(
    async (teamId: string, includeClosed = false) => {
      const teamsClient = await getTeamsClient()
      return await teamsClient.getRisks(teamId, includeClosed)
    },
    []
  )

  const Actions = () => {
    return (
      <>
        {canUpdateTeam && (
          <Button onClick={() => setOpenUpdateTeamModal(true)}>
            Edit Team
          </Button>
        )}
      </>
    )
  }

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: createElement(TeamDetails, team),
    },
    {
      key: 'risk-management',
      tab: 'Risk Management',
      content: createElement(RisksGrid, {
        getRisks: getRisks,
        getRisksObjectId: team?.id,
        newRisksAllowed: true,
        teamId: team?.id,
        hideTeamColumn: true,
      } as RisksGridProps),
    },
    {
      key: 'team-memberships',
      tab: 'Team Memberships',
      content: createElement(TeamMembershipsGrid, {
        teamMemberships: teamMemberships,
      }),
    },
  ]

  useEffect(() => {
    const getTeam = async () => {
      const teamsClient = await getTeamsClient()
      const teamDto = await teamsClient.getById(id)
      setTeam(teamDto)

      const teamMembershipDtos = await teamsClient.getTeamMemberships(
        teamDto.id
      )
      setTeamMemberships(teamMembershipDtos)
      setBreadcrumbTitle(teamDto.name)
    }

    getTeam()
  }, [id, setBreadcrumbTitle, lastRefresh])

  const onUpdateTeamFormClosed = (wasUpdated: boolean) => {
    setOpenUpdateTeamModal(false)
    if (wasUpdated) {
      // TODO: refresh the team details only
      setLastRefresh(Date.now())
    }
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
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {team && (
        <EditTeamForm
          showForm={openUpdateTeamModal}
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
