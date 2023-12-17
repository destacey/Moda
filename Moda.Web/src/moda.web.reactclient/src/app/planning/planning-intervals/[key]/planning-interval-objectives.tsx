'use client'

import PlanningIntervalObjectivesGrid from '@/src/app/components/common/planning/planning-interval-objectives-grid'
import { PlanningIntervalDetailsDto } from '@/src/services/moda-api'
import { useGetPlanningIntervalObjectives } from '@/src/services/queries/planning-queries'
import { BarsOutlined, BuildOutlined } from '@ant-design/icons'
import { Segmented } from 'antd'
import { SegmentedLabeledOption } from 'antd/es/segmented'
import { useMemo, useState } from 'react'
import { PlanningIntervalObjectivesTimeline } from '../../components'

interface PlanningIntervalObjectivesProps {
  planningInterval: PlanningIntervalDetailsDto
  objectivesQueryEnabled: boolean
  newObjectivesAllowed: boolean
  teamNames: string[]
}

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

const PlanningIntervalObjectives = ({
  planningInterval,
  objectivesQueryEnabled,
  newObjectivesAllowed,
  teamNames,
}: PlanningIntervalObjectivesProps) => {
  const [currentView, setCurrentView] = useState<string | number>('List')

  const objectivesQuery = useGetPlanningIntervalObjectives(
    planningInterval?.id,
    objectivesQueryEnabled,
  )

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

  if (!planningInterval) return null

  return (
    <>
      {currentView === 'List' && (
        <PlanningIntervalObjectivesGrid
          objectivesQuery={objectivesQuery}
          planningIntervalId={planningInterval?.id}
          hidePlanningIntervalColumn={true}
          hideTeamColumn={false}
          newObjectivesAllowed={newObjectivesAllowed}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <PlanningIntervalObjectivesTimeline
          objectivesQuery={objectivesQuery}
          planningInterval={planningInterval}
          enableGroups={true}
          teamNames={teamNames}
          viewSelector={viewSelector}
        />
      )}
    </>
  )
}

export default PlanningIntervalObjectives
