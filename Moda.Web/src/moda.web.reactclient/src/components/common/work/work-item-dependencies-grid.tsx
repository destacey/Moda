'use client'

import {
  ScopedDependencyDto,
  WorkItemDetailsDto,
} from '@/src/services/moda-api'
import { ColDef, GetRowIdParams } from 'ag-grid-community'
import { CustomCellRendererProps } from 'ag-grid-react'
import { FC, useCallback, useMemo } from 'react'
import ModaGrid from '../moda-grid'
import { workItemKeyComparator } from './work-item-utils'
import Link from 'next/link'
import { ExportOutlined } from '@ant-design/icons'
import {
  DependencyHealthCellRenderer,
  renderSprintLinkHelper,
  renderTeamLinkHelper,
} from '../moda-grid-cell-renderers'

export interface WorkItemDependenciesGridProps {
  workItem: WorkItemDetailsDto
  dependencies: ScopedDependencyDto[]
  isLoading: boolean
  refetch: () => void
}

const WorkItemLinkCellRenderer = (
  props: CustomCellRendererProps<ScopedDependencyDto>,
) => {
  const { value, data } = props
  if (!data?.dependency) return null

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

const WorkItemDependenciesGrid: FC<WorkItemDependenciesGridProps> = (props) => {
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
          {
            field: 'health.name',
            headerName: 'Health',
            width: 100,
            cellRenderer: DependencyHealthCellRenderer,
          },
          {
            field: 'scope.name',
            headerName: 'Scope',
            width: 100,
            headerTooltip:
              'Defines whether this dependency is managed inside a single team (intra-team) or requires coordination between multiple teams (cross-team).',
          },
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
            cellRenderer: (
              params: CustomCellRendererProps<ScopedDependencyDto>,
            ) => renderTeamLinkHelper(params.data?.dependency.team),
          },
          {
            field: 'dependency.sprint.name',
            headerName: 'Sprint',
            cellRenderer: (
              params: CustomCellRendererProps<ScopedDependencyDto>,
            ) => renderSprintLinkHelper(params.data?.dependency.sprint),
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
