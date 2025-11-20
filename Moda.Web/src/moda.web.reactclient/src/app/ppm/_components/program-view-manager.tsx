'use client'

import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useMemo, useState } from 'react'
import { ProgramListDto } from '@/src/services/moda-api'
import ProgramsGrid from './programs-grid'
import { ProgramsTimeline } from '.'

interface ProgramViewManagerProps {
  programs: ProgramListDto[]
  isLoading: boolean
  refetch: () => void
}

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

const ProgramViewManager = (props: ProgramViewManagerProps) => {
  const [currentView, setCurrentView] = useState<string | number>('List')

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

  return (
    <>
      {currentView === 'List' && (
        <ProgramsGrid
          programs={props.programs}
          isLoading={props.isLoading}
          refetch={props.refetch}
          hidePortfolio={true}
          gridHeight={550}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <ProgramsTimeline
          programs={props.programs}
          isLoading={props.isLoading}
          refetch={props.refetch}
          viewSelector={viewSelector}
        />
      )}
    </>
  )
}

export default memo(ProgramViewManager)
