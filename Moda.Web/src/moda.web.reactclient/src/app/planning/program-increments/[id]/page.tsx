'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { getProgramIncrementsClient } from '@/src/services/clients'
import { ProgramIncrementDetailsDto } from '@/src/services/moda-api'
import { Button, Card, Dropdown, MenuProps, Space } from 'antd'
import { createElement, useCallback, useEffect, useMemo, useState } from 'react'
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
import Link from 'next/link'
import { DownOutlined } from '@ant-design/icons'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { EditProgramIncrementForm } from '../../components'

const ProgramIncrementDetailsPage = ({ params }) => {
  useDocumentTitle('PI Details')
  const { setBreadcrumbTitle } = useBreadcrumbs()
  const [activeTab, setActiveTab] = useState('details')
  const [programIncrement, setProgramIncrement] =
    useState<ProgramIncrementDetailsDto | null>(null)
  const [openEditProgramIncrementForm, setOpenEditProgramIncrementForm] =
    useState<boolean>(false)
  const [openManageTeamsForm, setOpenManageTeamsForm] = useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())

  const { hasClaim } = useAuth()
  const canUpdateProgramIncrement = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Update'
  )

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
        null,
        includeClosed
      )
    },
    []
  )

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canUpdateProgramIncrement) {
      items.push(
        {
          key: 'edit',
          label: 'Edit',
          onClick: () => setOpenEditProgramIncrementForm(true),
        },
        {
          key: 'manage-teams',
          label: 'Manage Teams',
          onClick: () => setOpenManageTeamsForm(true),
        }
      )
    }
    return items
  }, [canUpdateProgramIncrement])

  const Actions = () => {
    return (
      <>
        <Space>
          <Link
            href={`/planning/program-increments/${programIncrement?.localId}/plan-review`}
          >
            Plan Review
          </Link>
          {canUpdateProgramIncrement && (
            <Dropdown menu={{ items: actionsMenuItems }}>
              <Button>
                <Space>
                  Actions
                  <DownOutlined />
                </Space>
              </Button>
            </Dropdown>
          )}
        </Space>
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
        newObjectivesAllowed: !programIncrement?.objectivesLocked ?? false,
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
  }, [params.id, setBreadcrumbTitle, lastRefresh])

  const onEditFormClosed = useCallback((wasSaved: boolean) => {
    setOpenEditProgramIncrementForm(false)
    if (wasSaved) {
      setLastRefresh(Date.now())
    }
  }, [])

  const onManageTeamsFormClosed = useCallback((wasSaved: boolean) => {
    setOpenManageTeamsForm(false)
    if (wasSaved) {
      setLastRefresh(Date.now())
    }
  }, [])

  return (
    <>
      <PageTitle
        title={programIncrement?.name}
        subtitle="Program Increment Details"
        actions={<Actions />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {programIncrement && canUpdateProgramIncrement && (
        <>
          <EditProgramIncrementForm
            showForm={openEditProgramIncrementForm}
            id={programIncrement?.id}
            onFormUpdate={() => onEditFormClosed(true)}
            onFormCancel={() => onEditFormClosed(false)}
          />
          <ManageProgramIncrementTeamsForm
            showForm={openManageTeamsForm}
            id={programIncrement?.id}
            onFormSave={() => onManageTeamsFormClosed(true)}
            onFormCancel={() => onManageTeamsFormClosed(false)}
          />
        </>
      )}
    </>
  )
}

export default ProgramIncrementDetailsPage
