'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Button, Card, Dropdown, MenuProps, Space } from 'antd'
import { createElement, useCallback, useEffect, useMemo, useState } from 'react'
import ProgramIncrementDetails from './program-increment-details'
import TeamsGrid, {
  TeamsGridProps,
} from '@/src/app/components/common/organizations/teams-grid'
import RisksGrid, {
  RisksGridProps,
} from '@/src/app/components/common/planning/risks-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import ManageProgramIncrementTeamsForm from './manage-program-increment-teams-form'
import Link from 'next/link'
import { DownOutlined } from '@ant-design/icons'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { EditProgramIncrementForm } from '../../components'
import {
  useGetProgramIncrementByKey,
  useGetProgramIncrementRisks,
  useGetProgramIncrementTeams,
} from '@/src/services/queries/planning-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/app/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import ProgramIncrementObjectives from './program-increment-objectives'
import ProgramIncrementDetailsLoading from './loading'

enum ProgramIncrementTabs {
  Details = 'details',
  Teams = 'teams',
  Objectives = 'objectives',
  RiskManagement = 'risk-management',
}

const ProgramIncrementDetailsPage = ({ params }) => {
  useDocumentTitle('PI Details')
  const [activeTab, setActiveTab] = useState(ProgramIncrementTabs.Details)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)
  const [openEditProgramIncrementForm, setOpenEditProgramIncrementForm] =
    useState<boolean>(false)
  const [teamsQueryEnabled, setTeamsQueryEnabled] = useState<boolean>(false)
  const [objectivesQueryEnabled, setObjectivesQueryEnabled] =
    useState<boolean>(false)
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [openManageTeamsForm, setOpenManageTeamsForm] = useState<boolean>(false)

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const { hasClaim } = useAuth()
  const canUpdateProgramIncrement = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Update',
  )

  const {
    data: programIncrementData,
    isLoading,
    isFetching,
    refetch: refetchProgramIncrement,
  } = useGetProgramIncrementByKey(params.key)

  const teamsQuery = useGetProgramIncrementTeams(
    programIncrementData?.id,
    teamsQueryEnabled,
  )

  const risksQuery = useGetProgramIncrementRisks(
    programIncrementData?.id,
    includeClosedRisks,
    risksQueryEnabled,
  )

  const onIncludeClosedRisksChanged = useCallback((includeClosed: boolean) => {
    setIncludeClosedRisks(includeClosed)
  }, [])

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
        },
        {
          key: 'divider',
          type: 'divider',
        },
        {
          key: 'healthReport',
          label: (
            <Link
              href={`/planning/program-increments/${params.key}/objectives/health-report`}
            >
              Health Report
            </Link>
          ),
        },
      )
    }
    return items
  }, [canUpdateProgramIncrement, params.key])

  const actions = () => {
    return (
      <>
        <Space>
          <Link href={`/planning/program-increments/${params.key}/plan-review`}>
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
      key: ProgramIncrementTabs.Details,
      tab: 'Details',
      content: (
        <ProgramIncrementDetails programIncrement={programIncrementData} />
      ),
    },
    {
      key: ProgramIncrementTabs.Teams,
      tab: 'Teams',
      content: createElement(TeamsGrid, {
        teamsQuery: teamsQuery,
      } as TeamsGridProps),
    },
    {
      key: ProgramIncrementTabs.Objectives,
      tab: 'Objectives',
      content: (
        <ProgramIncrementObjectives
          programIncrement={programIncrementData}
          objectivesQueryEnabled={objectivesQueryEnabled}
          newObjectivesAllowed={
            !programIncrementData?.objectivesLocked ?? false
          }
          teamNames={teamsQuery?.data?.map((t) => t.name)}
        />
      ),
    },
    {
      key: ProgramIncrementTabs.RiskManagement,
      tab: 'Risk Management',
      content: createElement(RisksGrid, {
        risksQuery: risksQuery,
        updateIncludeClosed: onIncludeClosedRisksChanged,
        getRisksObjectId: programIncrementData?.id,
        newRisksAllowed: true,
      } as RisksGridProps),
    },
  ]

  useEffect(() => {
    programIncrementData &&
      dispatch(
        setBreadcrumbTitle({ title: programIncrementData.name, pathname }),
      )
  }, [dispatch, pathname, programIncrementData])

  const onEditFormClosed = useCallback((wasSaved: boolean) => {
    setOpenEditProgramIncrementForm(false)
    if (wasSaved) {
      refetchProgramIncrement()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const onManageTeamsFormClosed = useCallback((wasSaved: boolean) => {
    setOpenManageTeamsForm(false)
    if (wasSaved) {
      refetchProgramIncrement()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // doesn't trigger on first render
  const onTabChange = useCallback(
    (tabKey) => {
      setActiveTab(tabKey)

      // enables the query for the tab on first render if it hasn't been enabled yet
      if (tabKey == ProgramIncrementTabs.Teams && !teamsQueryEnabled) {
        setTeamsQueryEnabled(true)
      } else if (
        tabKey == ProgramIncrementTabs.Objectives &&
        !objectivesQueryEnabled
      ) {
        setObjectivesQueryEnabled(true)
      } else if (
        tabKey == ProgramIncrementTabs.RiskManagement &&
        !risksQueryEnabled
      ) {
        setRisksQueryEnabled(true)
      }
    },
    [objectivesQueryEnabled, risksQueryEnabled, teamsQueryEnabled],
  )

  if (isLoading) {
    return <ProgramIncrementDetailsLoading />
  }

  if (!isLoading && !isFetching && !programIncrementData) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title={programIncrementData?.name}
        subtitle="Program Increment Details"
        actions={actions()}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {openEditProgramIncrementForm && (
        <EditProgramIncrementForm
          showForm={openEditProgramIncrementForm}
          id={programIncrementData?.id}
          onFormUpdate={() => onEditFormClosed(true)}
          onFormCancel={() => onEditFormClosed(false)}
        />
      )}
      {openManageTeamsForm && (
        <ManageProgramIncrementTeamsForm
          showForm={openManageTeamsForm}
          id={programIncrementData?.id}
          onFormSave={() => onManageTeamsFormClosed(true)}
          onFormCancel={() => onManageTeamsFormClosed(false)}
        />
      )}
    </>
  )
}

const ProgramIncrementDetailsPageWithAuthorization = authorizePage(
  ProgramIncrementDetailsPage,
  'Permission',
  'Permissions.ProgramIncrements.View',
)

export default ProgramIncrementDetailsPageWithAuthorization
