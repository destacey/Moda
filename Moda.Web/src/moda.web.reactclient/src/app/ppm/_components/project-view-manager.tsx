'use client'

import { BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useEffect, useMemo, useState } from 'react'
import { MessageInstance } from 'antd/es/message/interface'
import { ProjectListDto } from '@/src/services/moda-api'
import ProjectsGrid from './projects-grid'

interface ProjectViewManagerProps {
  projects: ProjectListDto[]
  isLoading: boolean
  refetch: () => void
  messageApi: MessageInstance
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
  const [projects, setProjectItems] = useState<ProjectListDto[]>([])

  useEffect(() => {
    setProjectItems(props.projects)
  }, [props.projects])

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
          projects={projects}
          isLoading={props.isLoading}
          refetch={props.refetch}
          messageApi={props.messageApi}
          hidePortfolio={true}
          gridHeight={550}
          viewSelector={viewSelector}
        />
      )}
      {/* {currentView === 'Timeline' && (
        <ProjectsTimeline
          projects={projects}
          isProjectItemsLoading={props.isLoading}
          refreshProjectItems={props.refetch}
          viewSelector={viewSelector}
        />
      )} */}
    </>
  )
}

export default memo(ProjectViewManager)
