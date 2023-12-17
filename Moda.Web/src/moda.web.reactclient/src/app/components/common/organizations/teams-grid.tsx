import Link from 'next/link'
import { useCallback, useMemo } from 'react'
import ModaGrid from '../moda-grid'
import { UseQueryResult } from 'react-query'
import { PlanningIntervalTeamResponse } from '@/src/services/moda-api'

export interface TeamsGridProps {
  teamsQuery: UseQueryResult<PlanningIntervalTeamResponse[], unknown>
}

const TeamLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.type === 'Team' ? 'teams' : 'team-of-teams'
  return <Link href={`/organizations/${teamRoute}/${data.key}`}>{value}</Link>
}

const TeamOfTeamsLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/team-of-teams/${data.teamOfTeams?.key}`}>
      {value}
    </Link>
  )
}

const TeamsGrid = ({ teamsQuery }: TeamsGridProps) => {
  const refresh = useCallback(async () => {
    teamsQuery.refetch()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const columnDefs = useMemo(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: TeamLinkCellRenderer },
      { field: 'code', width: 125 },
      { field: 'type' },
      {
        field: 'teamOfTeams.name',
        headerName: 'Team of Teams',
        cellRenderer: TeamOfTeamsLinkCellRenderer,
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
