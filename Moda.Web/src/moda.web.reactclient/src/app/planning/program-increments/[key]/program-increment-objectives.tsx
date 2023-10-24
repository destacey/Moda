import ProgramIncrementObjectivesGrid from '@/src/app/components/common/planning/program-increment-objectives-grid'
import { ProgramIncrementDetailsDto } from '@/src/services/moda-api'
import { useGetProgramIncrementObjectives } from '@/src/services/queries/planning-queries'
import { BarsOutlined, BuildOutlined } from '@ant-design/icons'
import { Segmented, Space, Typography } from 'antd'
import { SegmentedLabeledOption } from 'antd/es/segmented'
import { useState } from 'react'
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

  if (!programIncrement) return null

  return (
    <>
      <Space
        style={{
          display: 'flex',
          justifyContent: 'flex-end',
          alignItems: 'center',
          paddingBottom: '16px',
        }}
      >
        <Segmented
          options={viewSelectorOptions}
          value={currentView}
          onChange={setCurrentView}
        />
      </Space>
      {currentView === 'List' && (
        <ProgramIncrementObjectivesGrid
          objectivesQuery={objectivesQuery}
          programIncrementId={programIncrement?.id}
          hideProgramIncrementColumn={true}
          hideTeamColumn={true}
          newObjectivesAllowed={newObjectivesAllowed}
        />
      )}
      {currentView === 'Timeline' && (
        <ProgramIncrementObjectivesTimeline
          objectivesQuery={objectivesQuery}
          programIncrement={programIncrement}
          enableGroups={true}
          teamNames={teamNames}
        />
      )}
    </>
  )
}

export default ProgramIncrementObjectives
