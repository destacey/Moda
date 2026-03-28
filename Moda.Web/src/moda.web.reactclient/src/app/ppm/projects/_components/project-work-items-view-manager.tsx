'use client'

import { ClusterOutlined, MenuOutlined } from '@ant-design/icons'
import Segmented, { SegmentedLabeledOption } from 'antd/es/segmented'
import { memo, useMemo, useRef, useState } from 'react'
import { useRemainingHeight } from '@/src/hooks'
import { WorkItemListDto } from '@/src/services/moda-api'
import ProjectWorkItemsTreeGrid from './project-work-items-tree-grid'
import WorkItemsGrid from '@/src/components/common/work/work-items-grid'

type WorkItemsView = 'Tree' | 'List'

/** Approximate height of the ModaGrid toolbar (row count, search, buttons). */
const MODA_GRID_TOOLBAR_HEIGHT = 45

const viewSelectorOptions: SegmentedLabeledOption[] = [
  {
    value: 'Tree',
    icon: <ClusterOutlined alt="Tree view" title="Tree view" />,
  },
  {
    value: 'List',
    icon: <MenuOutlined alt="List view" title="List view" />,
  },
]

interface ProjectWorkItemsViewManagerProps {
  workItems: WorkItemListDto[]
  isLoading: boolean
  refetch: () => void
  hideProjectColumn?: boolean
  defaultView?: WorkItemsView
}

const ProjectWorkItemsViewManager = (
  props: ProjectWorkItemsViewManagerProps,
) => {
  const [currentView, setCurrentView] = useState<string | number>(
    props.defaultView ?? 'Tree',
  )
  const containerRef = useRef<HTMLDivElement>(null)
  const gridHeight = useRemainingHeight(containerRef)

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
    <div ref={containerRef} style={{ height: '100%' }}>
      {currentView === 'Tree' && (
        <ProjectWorkItemsTreeGrid
          workItems={props.workItems}
          isLoading={props.isLoading}
          refetch={props.refetch}
          hideProjectColumn={props.hideProjectColumn}
          viewSelector={viewSelector}
          gridHeight={gridHeight}
        />
      )}
      {currentView === 'List' && (
        <WorkItemsGrid
          workItems={props.workItems}
          isLoading={props.isLoading}
          refetch={props.refetch}
          hideProjectColumn={props.hideProjectColumn}
          viewSelector={viewSelector}
          gridHeight={gridHeight - MODA_GRID_TOOLBAR_HEIGHT}
        />
      )}
    </div>
  )
}

export default memo(ProjectWorkItemsViewManager)

