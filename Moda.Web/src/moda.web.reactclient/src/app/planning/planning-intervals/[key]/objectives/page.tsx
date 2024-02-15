'use client'
import PlanningIntervalObjectivesGrid from '@/src/app/components/common/planning/planning-interval-objectives-grid'
import { useDocumentTitle } from '@/src/app/hooks'
import {
  useGetPlanningIntervalByKey,
  useGetPlanningIntervalCalendar,
  useGetPlanningIntervalObjectives,
  useGetPlanningIntervalTeams,
} from '@/src/services/queries/planning-queries'
import { BarsOutlined, BuildOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { useCallback, useMemo, useState } from 'react'
import { PlanningIntervalObjectivesTimeline } from '../../../components'
import { PageTitle } from '@/src/app/components/common'
import { notFound } from 'next/navigation'
import { Button } from 'antd'
import useAuth from '@/src/app/components/contexts/auth'
import CreatePlanningIntervalObjectiveForm from '../create-planning-interval-objective-form'

const viewSelectorOptions: SegmentedLabeledOption[] = [
  {
    value: 'List',
    icon: <BarsOutlined alt="List" title="List" />,
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
  } = useGetPlanningIntervalByKey(params.key)

  const objectivesQuery = useGetPlanningIntervalObjectives(
    planningIntervalData?.id,
    true,
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
    (!planningIntervalData?.objectivesLocked ?? false) &&
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
          objectivesQuery={objectivesQuery}
          planningIntervalId={planningIntervalData?.id}
          hidePlanningIntervalColumn={true}
          hideTeamColumn={false}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <PlanningIntervalObjectivesTimeline
          objectivesQuery={objectivesQuery}
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

export default PlanningIntervalObjectivesPage
