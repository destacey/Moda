'use client'

import {
  ScopedDependencyDto,
  WorkItemDetailsDto,
} from '@/src/services/moda-api'
import { ColDef, GetRowIdParams } from 'ag-grid-community'
import { useCallback, useMemo, useRef } from 'react'
import ModaGrid from '../moda-grid'
import { workItemKeyComparator } from './work-item-utils'
import Link from 'next/link'
import { ExportOutlined } from '@ant-design/icons'
import { TeamNameLinkCellRenderer } from '../moda-grid-cell-renderers'

export interface WorkItemDependenciesGridProps {
  workItem: WorkItemDetailsDto
  dependencies: ScopedDependencyDto[]
  isLoading: boolean
  refetch: () => void
}

const WorkItemLinkCellRenderer = ({ value, data }) => {
  return (
    <>
      <Link
        href={`/work/workspaces/${data.dependency.workspaceKey}/work-items/${data.dependency.key}`}
        prefetch={false}
      >
        {value}
      </Link>

      {data.dependency.externalViewWorkItemUrl && (
        <Link
          href={data.dependency.externalViewWorkItemUrl}
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

const dependencyTypeTooltip = (
  data: ScopedDependencyDto,
  workItem: WorkItemDetailsDto,
) => {
  if (data.type === 'Successor') {
    return `${data.dependency.key} is a successor to ${workItem.key}.  This means that ${data.dependency.key} cannot be completed until ${workItem.key} is completed.`
  } else if (data.type === 'Predecessor') {
    return `${data.dependency.key} is a predecessor to ${workItem.key}. This means that ${workItem.key} cannot be completed until ${data.dependency.key} is completed.`
  }

  return 'Unknown dependency type'
}

const WorkItemDependenciesGrid: React.FC<WorkItemDependenciesGridProps> = (
  props,
) => {
  const { workItem, refetch } = props

  const getRowId = useCallback(
    ({ data }: GetRowIdParams<ScopedDependencyDto>) => {
      return data.id
    },
    [],
  )

  const columnDefs = useMemo<ColDef<ScopedDependencyDto>[]>(
    () => [
      {
        headerName: 'Dependency Info',
        children: [
          {
            field: 'type',
            width: 125,
            tooltipValueGetter: (params) =>
              dependencyTypeTooltip(params.data, workItem),
            sort: 'asc',
            sortIndex: 0,
          },
          { field: 'state.name', headerName: 'State', width: 100 },
          // {
          //   field: 'createdOn',
          //   width: 150,
          //   valueGetter: (params) =>
          //     dayjs(params.data.createdOn).format('M/D/YYYY h:mm A'),
          // },
        ],
      },
      {
        headerName: 'Work Item Info',
        children: [
          {
            field: 'dependency.key',
            headerName: 'Key',
            comparator: workItemKeyComparator,
            cellRenderer: WorkItemLinkCellRenderer,
          },
          { field: 'dependency.title', headerName: 'Title', width: 400 },
          { field: 'dependency.type', headerName: 'Type', width: 150 },
          { field: 'dependency.status', headerName: 'Status', width: 150 },
          {
            field: 'dependency.team.name',
            headerName: 'Team',
            cellRenderer: (params) =>
              TeamNameLinkCellRenderer({ data: params.data?.dependency.team }),
          },
        ],
      },
    ],
    [workItem],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={props.dependencies}
        loadData={refresh}
        loading={props.isLoading}
        getRowId={getRowId}
      />
    </>
  )
}

export default WorkItemDependenciesGrid
