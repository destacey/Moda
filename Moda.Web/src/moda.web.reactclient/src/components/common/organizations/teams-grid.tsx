import { FC, useCallback, useMemo } from 'react'
import ModaGrid from '../moda-grid'
import { PlanningIntervalTeamResponse } from '@/src/services/moda-api'
import {
  NestedTeamOfTeamsNameLinkCellRenderer,
  TeamNameLinkCellRenderer,
} from '../moda-grid-cell-renderers'
import { ColDef } from 'ag-grid-community'

export interface TeamsGridProps {
  teams: PlanningIntervalTeamResponse[]
  isLoading: boolean
  refetch: () => void
}

const TeamsGrid: FC<TeamsGridProps> = (props) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<PlanningIntervalTeamResponse>[]>(
    () => [
      { field: 'key', width: 90 },
      {
        field: 'name',
        cellRenderer: TeamNameLinkCellRenderer,
      },
      { field: 'code', width: 125 },
      { field: 'type' },
      {
        field: 'teamOfTeams.name',
        headerName: 'Team of Teams',
        cellRenderer: NestedTeamOfTeamsNameLinkCellRenderer,
      },
      { field: 'isActive' }, // TODO: convert to yes/no
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={props.teams}
        loadData={refresh}
        loading={props.isLoading}
      />
    </>
  )
}

export default TeamsGrid
