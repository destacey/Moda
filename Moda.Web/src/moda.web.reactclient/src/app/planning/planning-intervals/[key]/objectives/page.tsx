'use client'
import PlanningIntervalObjectivesGrid from '@/src/app/components/common/planning/planning-interval-objectives-grid'
import { useDocumentTitle } from '@/src/app/hooks'
import {
  useGetPlanningInterval,
  useGetPlanningIntervalCalendar,
  useGetPlanningIntervalTeams,
} from '@/src/services/queries/planning-queries'
import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { useCallback, useMemo, useState } from 'react'
import {
  CreatePlanningIntervalObjectiveForm,
  PlanningIntervalObjectivesTimeline,
} from '../../components'
import { PageTitle } from '@/src/app/components/common'
import { notFound } from 'next/navigation'
import { Button } from 'antd'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetPlanningIntervalObjectivesQuery } from '@/src/store/features/planning/planning-interval-api'

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

const PlanningIntervalObjectivesPage = ({ params }) => {
  useDocumentTitle('PI Objectives')
  const [currentView, setCurrentView] = useState<string | number>('List')
  const [openCreateObjectiveForm, setOpenCreateObjectiveForm] =
    useState<boolean>(false)

  const {
    data: planningIntervalData,
    isLoading,
    isFetching,
    refetch: refetchPlanningInterval,
  } = useGetPlanningInterval(params.key)

  const {
    data: objectivesData,
    isLoading: isLoadingObjectives,
    refetch: refectObjectives,
  } = useGetPlanningIntervalObjectivesQuery(
    {
      planningIntervalId: planningIntervalData?.id,
      teamId: null,
    },
    { skip: !planningIntervalData?.id },
  )

  const calendarQuery = useGetPlanningIntervalCalendar(planningIntervalData?.id)

  const { data: teamData } = useGetPlanningIntervalTeams(
    planningIntervalData?.id,
    true,
  )

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
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

  if (!isLoading && !isFetching && !planningIntervalData) {
    notFound()
  }

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

  return (
    <>
      <PageTitle title="PI Objectives" actions={showActions && actions()} />
      {currentView === 'List' && (
        <PlanningIntervalObjectivesGrid
          objectivesData={objectivesData}
          isLoading={isLoadingObjectives}
          refreshObjectives={refectObjectives}
          planningIntervalId={planningIntervalData?.id}
          hidePlanningIntervalColumn={true}
          hideTeamColumn={false}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <PlanningIntervalObjectivesTimeline
          objectivesData={objectivesData}
          planningIntervalCalendarQuery={calendarQuery}
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
          planningIntervalId={planningIntervalData?.id}
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
