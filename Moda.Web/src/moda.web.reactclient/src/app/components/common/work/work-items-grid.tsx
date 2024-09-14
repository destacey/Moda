'use client'

import { ModaGrid } from '@/src/app/components/common'
import { WorkItemListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import Link from 'next/link'
import { useCallback, useMemo } from 'react'
import { ExportOutlined } from '@ant-design/icons'
import { NestedTeamNameLinkCellRenderer } from '../moda-grid-cell-renderers'
import {
  workItemKeyComparator,
  workStatusCategoryComparator,
} from './work-item-utils'

export interface WorkItemsGridProps {
  workItems: WorkItemListDto[]
  isLoading: boolean
  refetch: () => void
}

const WorkItemLinkCellRenderer = ({ value, data }) => {
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

const ParentWorkItemLinkCellRenderer = ({ value, data }) => {
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

const AssignedToLinkCellRenderer = ({ value, data }) => {
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
      { field: 'type', width: 125 },
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
    ],
    [],
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

export default WorkItemsGrid
