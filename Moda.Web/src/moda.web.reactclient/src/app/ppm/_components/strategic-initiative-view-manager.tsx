'use client'

import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useMemo, useState } from 'react'
import { StrategicInitiativeListDto } from '@/src/services/moda-api'
import { StrategicInitiativesGrid, StrategicInitiativesTimeline } from '.'

interface StrategicInitiativeViewManagerProps {
  strategicInitiatives: StrategicInitiativeListDto[]
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

const StrategicInitiativeViewManager = (
  props: StrategicInitiativeViewManagerProps,
) => {
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
        <StrategicInitiativesGrid
          strategicInitiatives={props.strategicInitiatives}
          isLoading={props.isLoading}
          refetch={props.refetch}
          hidePortfolio={true}
          gridHeight={550}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <StrategicInitiativesTimeline
          strategicInitiatives={props.strategicInitiatives}
          isLoading={props.isLoading}
          refetch={props.refetch}
          viewSelector={viewSelector}
        />
      )}
    </>
  )
}

export default memo(StrategicInitiativeViewManager)
