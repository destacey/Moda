'use client'

import {
  DependencyDto,
  TeamDetailsDto,
  WorkItemDetailsNavigationDto,
} from '@/src/services/moda-api'
import { ColDef, GetRowIdParams } from 'ag-grid-community'
import { FC, useCallback, useMemo } from 'react'
import ModaGrid from '../moda-grid'
import Link from 'next/link'
import { ExportOutlined } from '@ant-design/icons'
import {
  DependencyHealthCellRenderer,
  TeamNameLinkCellRenderer,
  WorkSprintLinkCellRenderer,
} from '../moda-grid-cell-renderers'
import { workItemKeyComparator } from '../work'

export interface TeamDependenciesGridProps {
  team: TeamDetailsDto
  dependencies: DependencyDto[]
  isLoading: boolean
  refetch: () => void
}

const WorkItemLinkCellRenderer = (data: WorkItemDetailsNavigationDto) => {
  return (
    <>
      <Link
        href={`/work/workspaces/${data.workspaceKey}/work-items/${data.key}`}
        prefetch={false}
      >
        {data.key}
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

const TeamDependenciesGrid: FC<TeamDependenciesGridProps> = (props) => {
  const { team, refetch } = props

  const getRowId = useCallback(({ data }: GetRowIdParams<DependencyDto>) => {
    return data.id
  }, [])

  const columnDefs = useMemo<ColDef<DependencyDto>[]>(
    () => [
      {
        headerName: 'Dependency Info',
        children: [
          { field: 'state.name', headerName: 'State', width: 125 },
          {
            field: 'health.name',
            headerName: 'Health',
            width: 100,
            cellRenderer: DependencyHealthCellRenderer,
          },
        ],
      },
      {
        headerName: 'Predecessor Info',
        children: [
          {
            field: 'source.key',
            headerName: 'Key',
            comparator: workItemKeyComparator,
            cellRenderer: (params) =>
              WorkItemLinkCellRenderer(params.data?.source),
          },
          { field: 'source.title', headerName: 'Title', width: 400 },
          { field: 'source.type', headerName: 'Type', width: 150 },
          { field: 'source.status', headerName: 'Status', width: 150 },
          {
            field: 'source.team.name',
            headerName: 'Team',
            cellRenderer: (params) =>
              TeamNameLinkCellRenderer({ data: params.data?.source.team }),
          },
          {
            field: 'source.sprint.name',
            headerName: 'Sprint',
            cellRenderer: (params) =>
              WorkSprintLinkCellRenderer({
                data: params.data?.source.sprint,
              }),
          },
        ],
      },
      {
        headerName: 'Successor Info',
        children: [
          {
            field: 'target.key',
            headerName: 'Key',
            comparator: workItemKeyComparator,
            cellRenderer: (params) =>
              WorkItemLinkCellRenderer(params.data?.target),
          },
          { field: 'target.title', headerName: 'Title', width: 400 },
          { field: 'target.type', headerName: 'Type', width: 150 },
          { field: 'target.status', headerName: 'Status', width: 150 },
          {
            field: 'target.team.name',
            headerName: 'Team',
            cellRenderer: (params) =>
              TeamNameLinkCellRenderer({ data: params.data?.target.team }),
          },
          {
            field: 'target.sprint.name',
            headerName: 'Sprint',
            cellRenderer: (params) =>
              WorkSprintLinkCellRenderer({
                data: params.data?.target.sprint,
              }),
          },
        ],
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
        rowData={props.dependencies}
        loadData={refresh}
        loading={props.isLoading}
        getRowId={getRowId}
      />
    </>
  )
}

export default TeamDependenciesGrid
