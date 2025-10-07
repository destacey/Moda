'use client'

import { RoadmapDetailsDto, RoadmapItemListDto } from '@/src/services/moda-api'
import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useEffect, useMemo, useState } from 'react'
import { RoadmapsTimeline } from '.'
import RoadmapItemsGrid from './roadmap-items-grid'

interface RoadmapViewManagerProps {
  roadmap: RoadmapDetailsDto
  roadmapItems: RoadmapItemListDto[]
  isRoadmapItemsLoading: boolean
  refreshRoadmapItems: () => void
  canUpdateRoadmap: boolean
  openRoadmapItemDrawer: (itemId: string) => void
  timelineEditMode?: boolean
}

const RoadmapViewManager = (props: RoadmapViewManagerProps) => {
  const [currentView, setCurrentView] = useState<string | number>('Timeline')
  const [roadmapItems, setRoadmapItems] = useState<RoadmapItemListDto[]>([])

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
          openRoadmapItemDrawer={props.openRoadmapItemDrawer}
          isRoadmapManager={props.canUpdateRoadmap}
          editMode={props.timelineEditMode}
        />
      )}
      {currentView === 'List' && (
        <RoadmapItemsGrid
          roadmapItemsData={roadmapItems}
          roadmapItemsIsLoading={props.isRoadmapItemsLoading}
          refreshRoadmapItems={props.refreshRoadmapItems}
          gridHeight={550}
          viewSelector={viewSelector}
          enableRowDrag={props.canUpdateRoadmap}
          roadmapId={props.roadmap.id}
          openRoadmapItemDrawer={props.openRoadmapItemDrawer}
          isRoadmapManager={props.canUpdateRoadmap}
        />
      )}
    </>
  )
}

export default memo(RoadmapViewManager)
