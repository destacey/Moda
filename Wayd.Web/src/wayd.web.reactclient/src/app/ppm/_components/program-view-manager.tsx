'use client'

import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useState } from 'react'
import { ProgramListDto } from '@/src/services/moda-api'
import { Spin } from 'antd'
import dynamic from 'next/dynamic'
import ProgramsGrid from './programs-grid'

const ProgramsTimeline = dynamic(() => import('./programs-timeline'), {
  ssr: false,
  loading: () => <Spin />,
})

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

  const viewSelector = (
    <Segmented
      options={viewSelectorOptions}
      value={currentView}
      onChange={setCurrentView}
    />
  )

  return (
    <>
      {currentView === 'List' && (
        <ProgramsGrid
          programs={props.programs}
          isLoading={props.isLoading}
          refetch={props.refetch}
          hidePortfolio={true}
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
