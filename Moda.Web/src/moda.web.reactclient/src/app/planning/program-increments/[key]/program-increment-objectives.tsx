'use client'

import ProgramIncrementObjectivesGrid from '@/src/app/components/common/planning/program-increment-objectives-grid'
import { ProgramIncrementDetailsDto } from '@/src/services/moda-api'
import { useGetProgramIncrementObjectives } from '@/src/services/queries/planning-queries'
import { BarsOutlined, BuildOutlined } from '@ant-design/icons'
import { Segmented } from 'antd'
import { SegmentedLabeledOption } from 'antd/es/segmented'
import { useMemo, useState } from 'react'
import { ProgramIncrementObjectivesTimeline } from '../../components'

interface ProgramIncrementObjectivesProps {
  programIncrement: ProgramIncrementDetailsDto
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

const ProgramIncrementObjectives = ({
  programIncrement,
  objectivesQueryEnabled,
  newObjectivesAllowed,
  teamNames,
}: ProgramIncrementObjectivesProps) => {
  const [currentView, setCurrentView] = useState<string | number>('List')

  const objectivesQuery = useGetProgramIncrementObjectives(
    programIncrement?.id,
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

  if (!programIncrement) return null

  return (
    <>
      {currentView === 'List' && (
        <ProgramIncrementObjectivesGrid
          objectivesQuery={objectivesQuery}
          programIncrementId={programIncrement?.id}
          hideProgramIncrementColumn={true}
          hideTeamColumn={false}
          newObjectivesAllowed={newObjectivesAllowed}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <ProgramIncrementObjectivesTimeline
          objectivesQuery={objectivesQuery}
          programIncrement={programIncrement}
          enableGroups={true}
          teamNames={teamNames}
          viewSelector={viewSelector}
        />
      )}
    </>
  )
}

export default ProgramIncrementObjectives
