'use client'

import { ModaGrid } from '@/src/components/common'
import { SprintBacklogItemDto } from '@/src/services/moda-api'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import Link from 'next/link'
import { useCallback, useMemo, memo } from 'react'
import { ExportOutlined } from '@ant-design/icons'
import {
  workItemKeyComparator,
  workStatusCategoryComparator,
} from '@/src/components/common/work'
import {
  NestedTeamNameLinkCellRenderer,
  ProjectLinkCellRenderer,
} from '@/src/components/common/moda-grid-cell-renderers'

export interface SprintBacklogGridProps {
  workItems: SprintBacklogItemDto[]
  hideTeamColumn: boolean
  isLoading: boolean
  refetch: () => void
}

const WorkItemLinkCellRenderer = ({
  value,
  data,
}: ICellRendererParams<SprintBacklogItemDto>) => {
  return (
    <>
      <Link
        href={`/work/workspaces/${data.workspace.key}/work-items/${data.key}`}
        prefetch={false}
      >
        {value}
      </Link>

      {data.externalViewWorkItemUrl && (
        <Link
          href={data.externalViewWorkItemUrl}
          target="_blank"
          title="Open in external system"
          style={{ marginLeft: '5px' }}
        >
          <ExportOutlined style={{ width: '10px' }} />
        </Link>
      )}
    </>
  )
}

const ParentWorkItemLinkCellRenderer = ({
  value,
  data,
}: ICellRendererParams<SprintBacklogItemDto>) => {
  if (!data.parent) return null
  return (
    <>
      <Link
        href={`/work/workspaces/${data.parent.workspaceKey}/work-items/${data.parent.key}`}
        prefetch={false}
      >
        {value}
      </Link>
      {data.parent.externalViewWorkItemUrl && (
        <Link
          href={data.parent.externalViewWorkItemUrl}
          target="_blank"
          title="Open in external system"
          style={{ marginLeft: '5px' }}
        >
          <ExportOutlined style={{ width: '10px' }} />
        </Link>
      )}
    </>
  )
}

const AssignedToLinkCellRenderer = ({
  value,
  data,
}: ICellRendererParams<SprintBacklogItemDto>) => {
  if (!data.assignedTo) return null
  return (
    <Link
      href={`/organizations/employees/${data.assignedTo.key}`}
      prefetch={false}
    >
      {value}
    </Link>
  )
}

const SprintBacklogGrid = (props: SprintBacklogGridProps) => {
  const { workItems = [], refetch } = props

  const columnDefs = useMemo<ColDef<SprintBacklogItemDto>[]>(
    () => [
      { field: 'rank', width: 80 },
      {
        field: 'key',
        comparator: workItemKeyComparator,
        cellRenderer: WorkItemLinkCellRenderer,
      },
      { field: 'type', width: 125 },
      { field: 'title', width: 400 },
      { field: 'status', width: 125 },
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
        rowData={workItems}
        loadData={refresh}
        loading={props.isLoading}
      />
    </>
  )
}

export default SprintBacklogGrid
