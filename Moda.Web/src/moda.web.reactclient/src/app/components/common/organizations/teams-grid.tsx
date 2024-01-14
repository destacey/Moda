import { useCallback, useMemo } from 'react'
import ModaGrid from '../moda-grid'
import { UseQueryResult } from 'react-query'
import { PlanningIntervalTeamResponse } from '@/src/services/moda-api'
import {
  TeamLinkCellRenderer,
  TeamRecordTeamLinkCellRenderer,
} from '../moda-grid-cell-renderers'

export interface TeamsGridProps {
  teamsQuery: UseQueryResult<PlanningIntervalTeamResponse[], unknown>
}

const TeamsGrid = ({ teamsQuery }: TeamsGridProps) => {
  const refresh = useCallback(async () => {
    teamsQuery.refetch()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const columnDefs = useMemo(
    () => [
      { field: 'key', width: 90 },
      {
        field: 'name',
        cellRenderer: TeamRecordTeamLinkCellRenderer,
      },
      { field: 'code', width: 125 },
      { field: 'type' },
      {
        field: 'teamOfTeams',
        headerName: 'Team of Teams',
        valueFormatter: (params) => params?.value?.name,
        cellRenderer: TeamLinkCellRenderer,
      },
      { field: 'isActive' }, // TODO: convert to yes/no
    ],
    [],
  )

  return (
    <>
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={teamsQuery.data}
        loadData={refresh}
      />
    </>
  )
}

export default TeamsGrid
