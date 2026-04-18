'use client'

import { RoadmapDetailsDto, RoadmapItemListDto } from '@/src/services/wayd-api'
import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useState } from 'react'
import dynamic from 'next/dynamic'
import { Spin } from 'antd'
import RoadmapItemsGrid from './roadmap-items-grid'

const RoadmapsTimeline = dynamic(() => import('./roadmaps-timeline'), {
  ssr: false,
  loading: () => <Spin />,
})

interface RoadmapViewManagerProps {
  roadmap: RoadmapDetailsDto
  roadmapItems: RoadmapItemListDto[]
  isRoadmapItemsLoading: boolean
  refreshRoadmapItems: () => void
  canUpdateRoadmap: boolean
  openRoadmapItemDrawer: (itemId: string) => void
}

const RoadmapViewManager = (props: RoadmapViewManagerProps) => {
  const [currentView, setCurrentView] = useState<string | number>('Timeline')

  const viewSelectorOptions: SegmentedLabeledOption[] = [
    {
      value: 'Timeline',
      icon: <BuildOutlined alt="Timeline" title="Timeline" />,
    },
    {
      value: 'List',
      icon: <MenuOutlined alt="List" title="List" />,
    },
  ]

  const viewSelector = (
    <Segmented
      options={viewSelectorOptions}
      value={currentView}
      onChange={setCurrentView}
    />
  )

  return (
    <>
      {currentView === 'Timeline' && (
        <RoadmapsTimeline
          roadmap={props.roadmap}
          roadmapItems={props.roadmapItems}
          isRoadmapItemsLoading={props.isRoadmapItemsLoading}
          refreshRoadmapItems={props.refreshRoadmapItems}
          viewSelector={viewSelector}
          openRoadmapItemDrawer={props.openRoadmapItemDrawer}
          isRoadmapManager={props.canUpdateRoadmap}
        />
      )}
      {currentView === 'List' && (
        <RoadmapItemsGrid
          roadmapItemsData={props.roadmapItems}
          roadmapItemsIsLoading={props.isRoadmapItemsLoading}
          refreshRoadmapItems={props.refreshRoadmapItems}
          viewSelector={viewSelector}
          roadmapId={props.roadmap.id}
          openRoadmapItemDrawer={props.openRoadmapItemDrawer}
          isRoadmapManager={props.canUpdateRoadmap}
        />
      )}
    </>
  )
}

export default memo(RoadmapViewManager)
