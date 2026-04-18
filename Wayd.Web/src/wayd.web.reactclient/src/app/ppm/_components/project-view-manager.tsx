'use client'

import {
  AppstoreOutlined,
  BuildOutlined,
  MenuOutlined,
} from '@ant-design/icons'
import { Segmented } from 'antd'
import { memo, useState } from 'react'
import { ProjectListDto } from '@/src/services/wayd-api'
import { Spin } from 'antd'
import dynamic from 'next/dynamic'
import ProjectsGrid from './projects-grid'
import ProjectsCardView from './projects-card-view'
import ProjectDrawer from './project-drawer'

const ProjectsTimeline = dynamic(() => import('./projects-timeline'), {
  ssr: false,
  loading: () => <Spin />,
})

type ProjectView = 'Card' | 'List' | 'Timeline'

const viewSelectorOptions = [
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

interface ProjectViewManagerProps {
  projects: ProjectListDto[]
  isLoading: boolean
  refetch: () => void
  hidePortfolio?: boolean
  hideProgram?: boolean
  groupByProgram?: boolean
  defaultView?: ProjectView
}

const ProjectViewManager = (props: ProjectViewManagerProps) => {
  const [currentView, setCurrentView] = useState<string | number>(
    props.defaultView ?? 'List',
  )
  const [selectedProjectKey, setSelectedProjectKey] = useState<string | null>(
    null,
  )
  const [drawerOpen, setDrawerOpen] = useState(false)

  const onCardClick = (key: string) => {
    setSelectedProjectKey(key)
    setDrawerOpen(true)
  }

  const viewSelector = (
    <Segmented
      options={viewSelectorOptions}
      value={currentView}
      onChange={setCurrentView}
    />
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
