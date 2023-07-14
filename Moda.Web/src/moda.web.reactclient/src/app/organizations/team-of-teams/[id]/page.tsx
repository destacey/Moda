'use client'

import PageTitle from '@/src/app/components/common/page-title'
import {
  RiskListDto,
  TeamMembershipsDto,
  TeamOfTeamsDetailsDto,
} from '@/src/services/moda-api'
import { Button, Card } from 'antd'
import { createElement, useEffect, useState } from 'react'
import TeamOfTeamsDetails from './team-of-teams-details'
import { getTeamsOfTeamsClient } from '@/src/services/clients'
import RisksGrid, {
  RisksGridProps,
} from '@/src/app/components/common/planning/risks-grid'
import TeamMembershipsGrid from '@/src/app/components/common/organizations/team-memberships-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import UpdateTeamForm from '../../components/update-team/update-team-form'
import useAuth from '@/src/app/components/contexts/auth'
import useBreadcrumb from '@/src/app/components/contexts/breadcrumbs'

const TeamOfTeamsDetailsPage = ({ params }) => {
  useDocumentTitle('Team of Teams Details')
  const [activeTab, setActiveTab] = useState('details')
  const [team, setTeam] = useState<TeamOfTeamsDetailsDto | null>(null)
  const [risks, setRisks] = useState<RiskListDto[]>([])
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
      content: createElement(TeamOfTeamsDetails, team),
    },
    {
      key: 'risk-management',
      tab: 'Risk Management',
      content: createElement(RisksGrid, {
        risks: risks,
        teamId: team?.id,
        newRisksAllowed: true,
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
      const teamsOfTeamsClient = await getTeamsOfTeamsClient()
      const teamDto = await teamsOfTeamsClient.getById(id)
      setTeam(teamDto)

      // TODO: move these to an onclick event based on when the user clicks the tab
      // TODO: setup the ability to change whether or not to show risks that are closed
      const riskDtos = await teamsOfTeamsClient.getRisks(teamDto.id, true)
      setRisks(riskDtos)

      const teamMembershipDtos = await teamsOfTeamsClient.getTeamMemberships(
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
        subtitle="Team of Teams Details"
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
        <UpdateTeamForm
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

export default TeamOfTeamsDetailsPage
