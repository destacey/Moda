'use client'

import { ModaGrid } from '@/src/components/common'
import { SprintBacklogItemDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { useCallback, useMemo } from 'react'
import {
  workItemKeyComparator,
  workStatusCategoryComparator,
} from '@/src/components/common/work'
import {
  AssignedToLinkCellRenderer,
  NestedTeamNameLinkCellRenderer,
  NestedWorkSprintLinkCellRenderer,
  ParentWorkItemLinkCellRenderer,
  ProjectLinkCellRenderer,
  WorkItemLinkCellRenderer,
  WorkStatusTagCellRenderer,
} from '@/src/components/common/moda-grid-cell-renderers'

export interface SprintBacklogGridProps {
  workItems: SprintBacklogItemDto[]
  isLoading: boolean
  refetch: () => void
  hideTeamColumn?: boolean
  hideSprintColumn?: boolean
  /** Grid height in pixels. Use -1 for auto-height (expands to fit all rows). Default: -1 */
  gridHeight?: number
}

const SprintBacklogGrid = (props: SprintBacklogGridProps) => {
  const {
    workItems = [],
    refetch,
    hideTeamColumn = false,
    hideSprintColumn = true,
    gridHeight = -1,
  } = props

  const columnDefs = useMemo<ColDef<SprintBacklogItemDto>[]>(
    () => [
      { field: 'rank', width: 50, filter: false },
      {
        field: 'key',
        comparator: workItemKeyComparator,
        cellRenderer: WorkItemLinkCellRenderer,
      },
      { field: 'type', width: 125 },
      { field: 'title', width: 400 },
      {
        field: 'storyPoints',
        headerName: 'SPs',
        headerTooltip: 'Story Points',
        width: 80,
      },
      { field: 'status', width: 125, cellRenderer: WorkStatusTagCellRenderer },
      {
        field: 'statusCategory.name',
        headerName: 'Status Category',
        width: 120,
        comparator: workStatusCategoryComparator,
      },
      {
        field: 'team.name',
        headerName: 'Team',
        cellRenderer: NestedTeamNameLinkCellRenderer,
        hide: hideTeamColumn,
      },
      {
        field: 'sprint.name',
        headerName: 'Sprint',
        cellRenderer: NestedWorkSprintLinkCellRenderer,
        cellRendererParams: { showTeamCode: false },
        hide: hideSprintColumn,
      },
      {
        field: 'parent.key',
        headerName: 'Parent Key',
        comparator: workItemKeyComparator,
        cellRenderer: ParentWorkItemLinkCellRenderer,
      },
      {
        field: 'parent.title',
        headerName: 'Parent',
        width: 400,
      },
      {
        field: 'assignedTo.name',
        headerName: 'Assigned To',
        cellRenderer: AssignedToLinkCellRenderer,
      },
      {
        field: 'project.name',
        headerName: 'Project',
        width: 300,
        cellRenderer: ProjectLinkCellRenderer,
      },
    ],
    [hideTeamColumn, hideSprintColumn],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <ModaGrid
      height={gridHeight}
      columnDefs={columnDefs}
      rowData={workItems}
      loadData={refresh}
      loading={props.isLoading}
      emptyMessage="No planned work items"
    />
  )
}

export default SprintBacklogGrid
