'use client'

import PlanningIntervalObjectivesGrid from '@/src/components/common/planning/planning-interval-objectives-grid'
import { useDocumentTitle } from '@/src/hooks'
import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { use, useCallback, useMemo, useState } from 'react'
import {
  CreatePlanningIntervalObjectiveForm,
  PlanningIntervalObjectivesTimeline,
} from '../../_components'
import { PageTitle } from '@/src/components/common'
import { notFound } from 'next/navigation'
import { Button } from 'antd'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import {
  useGetPlanningIntervalCalendarQuery,
  useGetPlanningIntervalObjectivesQuery,
  useGetPlanningIntervalQuery,
  useGetPlanningIntervalTeamsQuery,
} from '@/src/store/features/planning/planning-interval-api'

const viewSelectorOptions: SegmentedLabeledOption[] = [
  {
    value: 'List',
    icon: <MenuOutlined alt="List" title="List" />,
  },
  {
    value: 'Timeline',
    icon: <BuildOutlined alt="Timeline" title="Timeline" />,
  },
]

const PlanningIntervalObjectivesPage = (props: {
  params: Promise<{ key: string }>
}) => {
  const { key } = use(props.params)
  const piKey = Number(key)

  useDocumentTitle('PI Objectives')
  const [currentView, setCurrentView] = useState<string | number>('List')
  const [openCreateObjectiveForm, setOpenCreateObjectiveForm] =
    useState<boolean>(false)

  const { data: planningIntervalData, isLoading } =
    useGetPlanningIntervalQuery(piKey)

  const {
    data: objectivesData,
    isLoading: isLoadingObjectives,
    refetch: refectObjectives,
  } = useGetPlanningIntervalObjectivesQuery({
    planningIntervalKey: piKey,
    teamId: null,
  })

  const { data: calendarData } = useGetPlanningIntervalCalendarQuery(piKey)

  const { data: teamData } = useGetPlanningIntervalTeamsQuery(piKey)

  const { hasPermissionClaim } = useAuth()
  const canManageObjectives = hasPermissionClaim(
    'Permissions.PlanningIntervalObjectives.Manage',
  )
  const canCreateObjectives =
    canManageObjectives &&
    planningIntervalData &&
    !planningIntervalData.objectivesLocked &&
    teamData?.filter((t) => t.type == 'Team').length > 0
  const showActions = canCreateObjectives

  const viewSelector = useMemo(
    () => (
      <Segmented
        options={viewSelectorOptions}
        value={currentView}
        onChange={setCurrentView}
      />
    ),
    [currentView],
  )

  const createObjectiveButtonClicked = useCallback(() => {
    setOpenCreateObjectiveForm(true)
  }, [])

  const onCreateObjectiveFormClosed = (wasCreated: boolean) => {
    setOpenCreateObjectiveForm(false)
  }

  const actions = () => {
    return (
      <>
        {canCreateObjectives && (
          <Button onClick={createObjectiveButtonClicked}>
            Create Objective
          </Button>
        )}
      </>
    )
  }

  if (!isLoading && !planningIntervalData) {
    return notFound()
  }

  return (
    <>
      <PageTitle title="PI Objectives" actions={showActions && actions()} />
      {currentView === 'List' && (
        <PlanningIntervalObjectivesGrid
          objectivesData={objectivesData}
          isLoading={isLoadingObjectives}
          refreshObjectives={refectObjectives}
          planningIntervalKey={piKey}
          hidePlanningIntervalColumn={true}
          hideTeamColumn={false}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <PlanningIntervalObjectivesTimeline
          objectivesData={objectivesData}
          planningIntervalCalendar={calendarData}
          enableGroups={true}
          teamNames={teamData
            ?.filter((t) => t.type == 'Team')
            .map((t) => t.name)}
          viewSelector={viewSelector}
        />
      )}
      {openCreateObjectiveForm && (
        <CreatePlanningIntervalObjectiveForm
          showForm={openCreateObjectiveForm}
          planningIntervalKey={piKey}
          onFormCreate={() => onCreateObjectiveFormClosed(true)}
          onFormCancel={() => onCreateObjectiveFormClosed(false)}
        />
      )}
    </>
  )
}

const PlanningIntervalObjectivesPageWithAuthorization = authorizePage(
  PlanningIntervalObjectivesPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default PlanningIntervalObjectivesPageWithAuthorization
