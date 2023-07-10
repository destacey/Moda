'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveListDto,
  RiskListDto,
} from '@/src/services/moda-api'
import { Button, Card } from 'antd'
import { createElement, useEffect, useState } from 'react'
import ProgramIncrementDetails from './program-increment-details'
import ProgramIncrementObjectivesGrid, {
  ProgramIncrementObjectivesGridProps,
} from '@/src/app/components/common/planning/program-increment-objectives-grid'
import { TeamListItem } from '@/src/app/organizations/types'
import TeamsGrid, {
  TeamsGridProps,
} from '@/src/app/components/common/organizations/teams-grid'
import RisksGrid, {
  RisksGridProps,
} from '@/src/app/components/common/planning/risks-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import useAuth from '@/src/app/components/contexts/auth'
import ManageProgramIncrementTeamsForm from './manage-program-increment-teams-form'

const ProgramIncrementDetailsPage = ({ params }) => {
  useDocumentTitle('PI Details')
  const { setBreadcrumbTitle } = useBreadcrumbs()
  const [activeTab, setActiveTab] = useState('details')
  const [programIncrement, setProgramIncrement] =
    useState<ProgramIncrementDetailsDto | null>(null)
  const [teams, setTeams] = useState<TeamListItem[]>([])
  const [objectives, setObjectives] = useState<
    ProgramIncrementObjectiveListDto[]
  >([])
  const [risks, setRisks] = useState<RiskListDto[]>([])
  const [openManageTeamsModal, setOpenManageTeamsModal] =
    useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())

  const { hasClaim } = useAuth()
  const canUpdateProgramIncrement = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Update'
  )
  const showActions = canUpdateProgramIncrement

  const Actions = () => {
    return (
      <>
        {canUpdateProgramIncrement && (
          <Button onClick={() => setOpenManageTeamsModal(true)}>
            Manage Teams
          </Button>
        )}
      </>
    )
  }

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: createElement(ProgramIncrementDetails, programIncrement),
    },
    {
      key: 'teams',
      tab: 'Teams',
      content: createElement(TeamsGrid, { teams: teams } as TeamsGridProps),
    },
    {
      key: 'objectives',
      tab: 'Objectives',
      content: createElement(ProgramIncrementObjectivesGrid, {
        objectives: objectives,
        hideProgramIncrementColumn: true,
        hideTeamColumn: false,
        newObjectivesAllowed: true,
        programIncrementId: programIncrement?.id,
      } as ProgramIncrementObjectivesGridProps),
    },
    {
      key: 'risk-management',
      tab: 'Risk Management',
      content: createElement(RisksGrid, { risks: risks } as RisksGridProps),
    },
  ]

  useEffect(() => {
    const getProgramIncrement = async () => {
      const programIncrementsClient = await getProgramIncrementsClient()
      const programIncrementDto = await programIncrementsClient.getByLocalId(
        params.id
      )
      setProgramIncrement(programIncrementDto)
      setBreadcrumbTitle(programIncrementDto.name)

      if (!programIncrementDto) return

      // TODO: move these to an onclick event based on when the user clicks the tab
      const teamDtos = await programIncrementsClient.getTeams(
        programIncrementDto.id
      )
      setTeams(teamDtos as TeamListItem[])

      const objectiveDtos = await programIncrementsClient.getObjectives(
        programIncrementDto.id,
        null
      )
      setObjectives(objectiveDtos)

      // TODO: setup the ability to change whether or not to show risks that are closed
      const riskDtos = await programIncrementsClient.getRisks(
        programIncrementDto.id,
        true
      )
      setRisks(riskDtos)
    }

    getProgramIncrement()
  }, [params.id, setBreadcrumbTitle, lastRefresh])

  const onManageTeamsFormClosed = (wasSaved: boolean) => {
    setOpenManageTeamsModal(false)
    if (wasSaved) {
      // TODO: refresh the PI details and Teams tab only
      setLastRefresh(Date.now())
    }
  }

  return (
    <>
      <PageTitle
        title={programIncrement?.name}
        subtitle="Program Increment Details"
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
      {programIncrement && (
        <ManageProgramIncrementTeamsForm
          showForm={openManageTeamsModal}
          id={programIncrement.id}
          onFormSave={() => onManageTeamsFormClosed(true)}
          onFormCancel={() => onManageTeamsFormClosed(false)}
        />
      )}
    </>
  )
}

export default ProgramIncrementDetailsPage
