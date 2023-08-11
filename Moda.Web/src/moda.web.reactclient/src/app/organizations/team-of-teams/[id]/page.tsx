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

const TeamOfTeamsDetailsPage = ({ params }) => {
  useDocumentTitle('Team of Teams Details')
  const [activeTab, setActiveTab] = useState('details')
  const [team, setTeam] = useState<TeamOfTeamsDetailsDto | null>(null)
  const [openUpdateTeamModal, setOpenUpdateTeamModal] = useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())
  const { id } = params
  const { setBreadcrumbTitle } = useBreadcrumbs()

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const showActions = canUpdateTeam

  const getRisks = useCallback(
    async (teamId: string, includeClosed = false) => {
      const teamOfTeamsClient = await getTeamsOfTeamsClient()
      return await teamOfTeamsClient.getRisks(teamId, includeClosed)
    },
    [],
  )

  const getTeamMemberships = useCallback(async (teamId: string) => {
    const teamOfTeamsClient = await getTeamsOfTeamsClient()
    return await teamOfTeamsClient.getTeamMemberships(teamId)
  }, [])

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
    setOpenUpdateTeamModal(false)
    if (wasUpdated) {
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
      {team && canUpdateTeam && (
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

export default TeamOfTeamsDetailsPage
