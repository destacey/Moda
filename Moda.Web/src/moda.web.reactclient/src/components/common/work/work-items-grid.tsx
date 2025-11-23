'use client'

import { ModaGrid } from '@/src/components/common'
import { WorkItemListDto } from '@/src/services/moda-api'
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

export interface WorkItemsGridProps {
  workItems: WorkItemListDto[]
  isLoading: boolean
  refetch: () => void
  gridHeight?: number
  hideParentColumn?: boolean
  hideProjectColumn?: boolean
}

const WorkItemsGrid = (props: WorkItemsGridProps) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<WorkItemListDto>[]>(
    () => [
      {
        field: 'key',
        comparator: workItemKeyComparator,
        cellRenderer: WorkItemLinkCellRenderer,
      },
      { field: 'title', width: 400 },
      { field: 'type.name', headerName: 'Type', width: 125 },
      {
        field: 'storyPoints',
        headerName: 'SPs',
        title: 'Story Points',
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
      },
      {
        field: 'sprint.name',
        headerName: 'Sprint',
        cellRenderer: NestedWorkSprintLinkCellRenderer,
      },
      {
        field: 'parent.key',
        headerName: 'Parent Key',
        hide: props.hideParentColumn,
        comparator: workItemKeyComparator,
        cellRenderer: ParentWorkItemLinkCellRenderer,
      },
      {
        field: 'parent.title',
        headerName: 'Parent',
        width: 400,
        hide: props.hideParentColumn,
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
        hide: props.hideProjectColumn,
        cellRenderer: ProjectLinkCellRenderer,
      },
      {
        field: 'storyPoints',
        headerName: 'Story Points',
        width: 100,
        filter: 'agNumberColumnFilter',
        type: 'numericColumn',
      },
    ],
    [props.hideParentColumn, props.hideProjectColumn],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <ModaGrid
      height={props.gridHeight ?? 550}
      columnDefs={columnDefs}
      rowData={props.workItems}
      loadData={refresh}
      loading={props.isLoading}
    />
  )
}

export default WorkItemsGrid
