'use client'

import { DependencyDto, TeamDetailsDto } from '@/src/services/moda-api'
import { ColDef, GetRowIdParams } from 'ag-grid-community'
import { CustomCellRendererProps } from 'ag-grid-react'
import { FC, useCallback, useMemo } from 'react'
import ModaGrid from '../moda-grid'
import {
  DependencyHealthCellRenderer,
  renderSprintLinkHelper,
  renderTeamLinkHelper,
  renderWorkItemLinkHelper,
} from '../moda-grid-cell-renderers'
import { workItemKeyComparator } from '../work'

export interface TeamDependenciesGridProps {
  team: TeamDetailsDto
  dependencies: DependencyDto[]
  isLoading: boolean
  refetch: () => void
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
          {
            field: 'scope.name',
            headerName: 'Scope',
            width: 100,
            headerTooltip:
              'Defines whether this dependency is managed inside a single team (intra-team) or requires coordination between multiple teams (cross-team).',
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
            cellRenderer: (params: CustomCellRendererProps<DependencyDto>) =>
              renderWorkItemLinkHelper(params.data?.source),
          },
          { field: 'source.title', headerName: 'Title', width: 400 },
          { field: 'source.type', headerName: 'Type', width: 150 },
          { field: 'source.status', headerName: 'Status', width: 150 },
          {
            field: 'source.team.name',
            headerName: 'Team',
            cellRenderer: (params: CustomCellRendererProps<DependencyDto>) =>
              renderTeamLinkHelper(params.data?.source.team),
          },
          {
            field: 'source.sprint.name',
            headerName: 'Sprint',
            cellRenderer: (params: CustomCellRendererProps<DependencyDto>) =>
              renderSprintLinkHelper(params.data?.source.sprint),
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
            cellRenderer: (params: CustomCellRendererProps<DependencyDto>) =>
              renderWorkItemLinkHelper(params.data?.target),
          },
          { field: 'target.title', headerName: 'Title', width: 400 },
          { field: 'target.type', headerName: 'Type', width: 150 },
          { field: 'target.status', headerName: 'Status', width: 150 },
          {
            field: 'target.team.name',
            headerName: 'Team',
            cellRenderer: (params: CustomCellRendererProps<DependencyDto>) =>
              renderTeamLinkHelper(params.data?.target.team),
          },
          {
            field: 'target.sprint.name',
            headerName: 'Sprint',
            cellRenderer: (params: CustomCellRendererProps<DependencyDto>) =>
              renderSprintLinkHelper(params.data?.target.sprint),
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
