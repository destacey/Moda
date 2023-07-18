'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { getProgramIncrementsClient } from '@/src/services/clients'
import { ProgramIncrementDetailsDto } from '@/src/services/moda-api'
import { Button, Card } from 'antd'
import { createElement, useCallback, useEffect, useState } from 'react'
import ProgramIncrementDetails from './program-increment-details'
import ProgramIncrementObjectivesGrid, {
  ProgramIncrementObjectivesGridProps,
} from '@/src/app/components/common/planning/program-increment-objectives-grid'
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
import ProgramIncrementViewSelector from './program-increment-view-selector'

const ProgramIncrementDetailsPage = ({ params }) => {
  useDocumentTitle('PI Details')
  const { setBreadcrumbTitle } = useBreadcrumbs()
  const [activeTab, setActiveTab] = useState('details')
  const [programIncrement, setProgramIncrement] =
    useState<ProgramIncrementDetailsDto | null>(null)
  const [openManageTeamsModal, setOpenManageTeamsModal] =
    useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())

  const { hasClaim } = useAuth()
  const canUpdateProgramIncrement = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Update'
  )
  const showActions = canUpdateProgramIncrement

  const getTeams = useCallback(async (programIncrementId: string) => {
    const programIncrementsClient = await getProgramIncrementsClient()
    return await programIncrementsClient.getTeams(programIncrementId)
  }, [])

  const getObjectives = useCallback(async (programIncrementId: string) => {
    const programIncrementsClient = await getProgramIncrementsClient()
    return await programIncrementsClient.getObjectives(programIncrementId, null)
  }, [])

  const getRisks = useCallback(
    async (programIncrementId: string, includeClosed = false) => {
      const programIncrementsClient = await getProgramIncrementsClient()
      return await programIncrementsClient.getRisks(
        programIncrementId,
        includeClosed
      )
    },
    []
  )

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
      content: createElement(TeamsGrid, {
        getTeams: getTeams,
        getTeamsObjectId: programIncrement?.id,
      } as TeamsGridProps),
    },
    {
      key: 'objectives',
      tab: 'Objectives',
      content: createElement(ProgramIncrementObjectivesGrid, {
        getObjectives: getObjectives,
        programIncrementId: programIncrement?.id,
        hideProgramIncrementColumn: true,
        hideTeamColumn: false,
        newObjectivesAllowed: true,
      } as ProgramIncrementObjectivesGridProps),
    },
    {
      key: 'risk-management',
      tab: 'Risk Management',
      content: createElement(RisksGrid, {
        getRisks: getRisks,
        getRisksObjectId: programIncrement?.id,
        newRisksAllowed: true,
      } as RisksGridProps),
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
    }

    getProgramIncrement()
  }, [getObjectives, getRisks, params.id, setBreadcrumbTitle])

  const onManageTeamsFormClosed = (wasSaved: boolean) => {
    setOpenManageTeamsModal(false)
    if (wasSaved) {
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
      <ProgramIncrementViewSelector
        startingView="details"
        programIncrementLocalId={programIncrement?.localId}
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
