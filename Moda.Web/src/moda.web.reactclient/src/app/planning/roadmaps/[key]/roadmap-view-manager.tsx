'use client'

import { RoadmapDetailsDto, RoadmapListDto } from '@/src/services/moda-api'
import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import { Flex } from 'antd'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { useEffect, useMemo, useState } from 'react'
import RoadmapsGrid from '../components/roadmaps-grid'
import RoadmapsTimeline from '../components/roadmaps-timeline'

interface RoadmapViewManagerProps {
  roadmap: RoadmapDetailsDto
  isLoading: boolean
  refreshRoadmap: () => void
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

const RoadmapViewManager = (props: RoadmapViewManagerProps) => {
  const [currentView, setCurrentView] = useState<string | number>('List')
  const [children, setChildren] = useState<RoadmapListDto[]>([])

  useEffect(() => {
    const children: RoadmapListDto[] = props.roadmap.children
      .slice()
      .sort((a, b) => a.order - b.order)
      .map((r) => r.roadmap)

    setChildren(children)
  }, [props.roadmap])

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
        <RoadmapsGrid
          roadmapsData={children}
          roadmapsLoading={props.isLoading}
          refreshRoadmaps={props.refreshRoadmap}
          gridHeight={550}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <RoadmapsTimeline
          roadmap={props.roadmap}
          roadmaps={children}
          isLoading={props.isLoading}
          refreshRoadmap={props.refreshRoadmap}
          viewSelector={viewSelector}
        />
      )}
    </>
  )
}

export default RoadmapViewManager
