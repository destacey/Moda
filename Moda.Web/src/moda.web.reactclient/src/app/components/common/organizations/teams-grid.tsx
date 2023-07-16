import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { TeamListItem } from '../../../organizations/types'

export interface TeamsGridProps {
  getTeams: (id: string) => Promise<TeamListItem[]>
  getTeamsObjectId: string
}

const TeamLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.type === 'Team' ? 'teams' : 'team-of-teams'
  return (
    <Link href={`/organizations/${teamRoute}/${data.localId}`}>{value}</Link>
  )
}

const TeamOfTeamsLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/team-of-teams/${data.teamOfTeams?.localId}`}>
      {value}
    </Link>
  )
}

const TeamsGrid = ({ getTeams, getTeamsObjectId }: TeamsGridProps) => {
  const [teams, setTeams] = useState<TeamListItem[]>()

  const loadTeams = useCallback(async () => {
    const teams = await getTeams(getTeamsObjectId)
    setTeams(teams)
  }, [getTeams, getTeamsObjectId])

  const columnDefs = useMemo(
    () => [
      { field: 'localId', headerName: '#', width: 90 },
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
    []
  )

  return (
    <>
      {/* TODO:  setup dynamic height */}
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={teams}
        loadData={loadTeams}
      />
    </>
  )
}

export default TeamsGrid
