'use client'

import { RoadmapDetailsDto, RoadmapItemDto } from '@/src/services/moda-api'
import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { useEffect, useMemo, useState } from 'react'
import { MessageInstance } from 'antd/es/message/interface'
import { RoadmapsTimeline } from '../components'
import RoadmapItemsGrid from '../components/roadmap-items-grid'

interface RoadmapViewManagerProps {
  roadmap: RoadmapDetailsDto
  roadmapItems: RoadmapItemDto[]
  isRoadmapItemsLoading: boolean
  refreshRoadmapItems: () => void
  canUpdateRoadmap: boolean
  messageApi: MessageInstance
}

const RoadmapViewManager = (props: RoadmapViewManagerProps) => {
  const [currentView, setCurrentView] = useState<string | number>('Timeline')
  const [roadmapItems, setRoadmapItems] = useState<RoadmapItemDto[]>([])

  useEffect(() => {
    setRoadmapItems(props.roadmapItems)
  }, [props.roadmapItems])

  const viewSelectorOptions: SegmentedLabeledOption[] = useMemo(() => {
    const options = [
      {
        value: 'Timeline',
        icon: <BuildOutlined alt="Timeline" title="Timeline" />,
      },
      {
        value: 'List',
        icon: <MenuOutlined alt="List" title="List" />,
      },
    ]

    return options
  }, [])

  const viewSelector = useMemo(
    () => (
      <Segmented
        options={viewSelectorOptions}
        value={currentView}
        onChange={setCurrentView}
      />
    ),
    [currentView, viewSelectorOptions],
  )

  return (
    <>
      {currentView === 'Timeline' && (
        <RoadmapsTimeline
          roadmap={props.roadmap}
          roadmapItems={roadmapItems}
          isRoadmapItemsLoading={props.isRoadmapItemsLoading}
          refreshRoadmapItems={props.refreshRoadmapItems}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'List' && (
        <RoadmapItemsGrid
          roadmapItemsData={roadmapItems}
          roadmapItemsLoading={props.isRoadmapItemsLoading}
          isRoadmapItemsLoading={props.refreshRoadmapItems}
          gridHeight={550}
          viewSelector={viewSelector}
          enableRowDrag={props.canUpdateRoadmap}
          parentRoadmapId={props.roadmap.id}
          messageApi={props.messageApi}
        />
      )}
    </>
  )
}

export default RoadmapViewManager
