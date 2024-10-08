'use client'

import {
  RoadmapChildrenDto,
  RoadmapDetailsDto,
  RoadmapListDto,
} from '@/src/services/moda-api'
import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { useEffect, useMemo, useState } from 'react'
import { MessageInstance } from 'antd/es/message/interface'
import { RoadmapsGrid, RoadmapsTimeline } from '../components'

interface RoadmapViewManagerProps {
  roadmap: RoadmapDetailsDto
  roadmapChildren: RoadmapChildrenDto[]
  isChildrenLoading: boolean
  refreshChildren: () => void
  canUpdateRoadmap: boolean
  messageApi: MessageInstance
}

const RoadmapViewManager = (props: RoadmapViewManagerProps) => {
  const [currentView, setCurrentView] = useState<string | number>('Timeline')
  const [children, setChildren] = useState<RoadmapListDto[]>([])

  useEffect(() => {
    const children: RoadmapChildrenDto[] = props.roadmapChildren
      .slice()
      .sort((a, b) => a.order - b.order)

    setChildren(children)
  }, [props.roadmap, props.roadmapChildren])

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
          roadmapChildren={children}
          isChildrenLoading={props.isChildrenLoading}
          refreshChildren={props.refreshChildren}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'List' && (
        <RoadmapsGrid
          roadmapsData={children}
          roadmapsLoading={props.isChildrenLoading}
          refreshRoadmaps={props.refreshChildren}
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
