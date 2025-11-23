'use client'

import { ModaGrid } from '@/src/components/common'
import { WorkItemListDto } from '@/src/services/moda-api'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import { forwardRef, useCallback, useMemo } from 'react'
import { AgGridReact } from 'ag-grid-react'
import {
  AssignedToLinkCellRenderer,
  DateTimeCellRenderer,
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
  showStats?: boolean
  onFilterChanged?: () => void
}

const WorkItemsGrid = forwardRef<AgGridReact<WorkItemListDto>, WorkItemsGridProps>((props, ref) => {
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
      { field: 'status', width: 125, cellRenderer: WorkStatusTagCellRenderer },
      {
        field: 'statusCategory.name',
        headerName: 'Status Category',
        width: 140,
        comparator: workStatusCategoryComparator,
      },
      {
        field: 'storyPoints',
        headerName: 'Story Points',
        width: 100,
        filter: 'agNumberColumnFilter',
        type: 'numericColumn',
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
        field: 'activated',
        hide: !props.showStats,
        filter: 'agDateColumnFilter',
        filterParams: {
          includeBlanksInEquals: false,
          includeBlanksInLessThan: false,
          includeBlanksInGreaterThan: false,
        },
        cellRenderer: DateTimeCellRenderer,
      },
      {
        field: 'done',
        hide: !props.showStats,
        sort: 'desc',
        filter: 'agDateColumnFilter',
        filterParams: {
          includeBlanksInEquals: false,
          includeBlanksInLessThan: false,
          includeBlanksInGreaterThan: false,
        },
        cellRenderer: DateTimeCellRenderer,
      },
      {
        field: 'cycleTime',
        headerName: 'Cycle Time (Days)',
        hide: !props.showStats,
        type: 'numericColumn',
        cellRenderer: (params: ICellRendererParams<WorkItemListDto>) =>
          params.value?.toFixed(2) ?? '',
      },
    ],
    [props.hideParentColumn, props.hideProjectColumn, props.showStats],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <ModaGrid
      ref={ref}
      height={props.gridHeight ?? 550}
      columnDefs={columnDefs}
      rowData={props.workItems}
      loadData={refresh}
      loading={props.isLoading}
      onFilterChanged={props.onFilterChanged}
    />
  )
})

WorkItemsGrid.displayName = 'WorkItemsGrid'

export default WorkItemsGrid
