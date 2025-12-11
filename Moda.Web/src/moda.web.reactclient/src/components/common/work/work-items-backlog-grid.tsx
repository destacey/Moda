'use client'

import { ModaGrid } from '@/src/components/common'
import { WorkItemBacklogItemDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { useCallback, useMemo } from 'react'
import {
  AssignedToLinkCellRenderer,
  NestedTeamNameLinkCellRenderer,
  NestedWorkSprintLinkCellRenderer,
  ParentWorkItemLinkCellRenderer,
  ProjectLinkCellRenderer,
  WorkItemLinkCellRenderer,
  WorkStatusTagCellRenderer,
} from '../moda-grid-cell-renderers'
import {
  workItemKeyComparator,
  workStatusCategoryComparator,
} from './work-item-utils'

export interface WorkItemsBacklogGridProps {
  workItems: WorkItemBacklogItemDto[]
  hideTeamColumn: boolean
  isLoading: boolean
  refetch: () => void
}

const WorkItemsBacklogGrid = (props: WorkItemsBacklogGridProps) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<WorkItemBacklogItemDto>[]>(
    () => [
      { field: 'rank', width: 125 },
      {
        field: 'key',
        comparator: workItemKeyComparator,
        cellRenderer: WorkItemLinkCellRenderer,
      },
      { field: 'title', width: 400 },
      { field: 'type', width: 125 },
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
        width: 140,
        comparator: workStatusCategoryComparator,
      },
      {
        field: 'team.name',
        headerName: 'Team',
        cellRenderer: NestedTeamNameLinkCellRenderer,
        hide: props.hideTeamColumn,
      },
      {
        field: 'sprint.name',
        headerName: 'Sprint',
        cellRenderer: NestedWorkSprintLinkCellRenderer,
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
    [props.hideTeamColumn],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={props.workItems}
        loadData={refresh}
        loading={props.isLoading}
      />
    </>
  )
}

export default WorkItemsBacklogGrid
