'use client'

import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useMemo, useState } from 'react'
import { ProjectListDto } from '@/src/services/moda-api'
import ProjectsGrid from './projects-grid'
import { ProjectsTimeline } from '.'

interface ProjectViewManagerProps {
  projects: ProjectListDto[]
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

const ProjectViewManager = (props: ProjectViewManagerProps) => {
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
        <ProjectsGrid
          projects={props.projects}
          isLoading={props.isLoading}
          refetch={props.refetch}
          hidePortfolio={true}
          gridHeight={550}
          viewSelector={viewSelector}
        />
      )}
      {currentView === 'Timeline' && (
        <ProjectsTimeline
          projects={props.projects}
          isLoading={props.isLoading}
          refetch={props.refetch}
          viewSelector={viewSelector}
        />
      )}
    </>
  )
}

export default memo(ProjectViewManager)
