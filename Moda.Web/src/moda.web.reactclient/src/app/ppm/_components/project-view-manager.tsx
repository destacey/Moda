'use client'

import { AppstoreOutlined, BuildOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useMemo, useState } from 'react'
import { ProjectListDto } from '@/src/services/moda-api'
import ProjectsGrid from './projects-grid'
import { ProjectsTimeline } from '.'
import ProjectsCardView from './projects-card-view'
import ProjectDrawer from './project-drawer'

type ProjectView = 'Card' | 'List' | 'Timeline'

interface ProjectViewManagerProps {
  projects: ProjectListDto[]
  isLoading: boolean
  refetch: () => void
  hidePortfolio?: boolean
  hideProgram?: boolean
  groupByProgram?: boolean
  defaultView?: ProjectView
}

const viewSelectorOptions: SegmentedLabeledOption[] = [
  {
    value: 'Card',
    icon: <AppstoreOutlined title="Card view" />,
  },
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
  const [currentView, setCurrentView] = useState<string | number>(
    props.defaultView ?? 'List',
  )
  const [selectedProjectKey, setSelectedProjectKey] = useState<string | null>(null)
  const [drawerOpen, setDrawerOpen] = useState(false)

  const onCardClick = (key: string) => {
    setSelectedProjectKey(key)
    setDrawerOpen(true)
  }

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
      {currentView === 'Card' && (
        <ProjectsCardView
          projects={props.projects}
          isLoading={props.isLoading}
          viewSelector={viewSelector}
          onCardClick={onCardClick}
        />
      )}
      {currentView === 'List' && (
        <ProjectsGrid
          projects={props.projects}
          isLoading={props.isLoading}
          refetch={props.refetch}
          hidePortfolio={props.hidePortfolio}
          hideProgram={props.hideProgram}
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
          groupByProgram={props.groupByProgram}
        />
      )}
      {selectedProjectKey && (
        <ProjectDrawer
          projectKey={selectedProjectKey}
          drawerOpen={drawerOpen}
          onDrawerClose={() => {
            setDrawerOpen(false)
            setSelectedProjectKey(null)
          }}
        />
      )}
    </>
  )
}

export default memo(ProjectViewManager)
