import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { TeamMembershipsDto } from '@/src/services/moda-api'
import dayjs from 'dayjs'
import { UseQueryResult } from 'react-query'

export interface TeamMembershipsGridProps {
  teamMembershipsQuery: UseQueryResult<TeamMembershipsDto[], unknown>
}

const ChildLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.child.type === 'Team' ? 'teams' : 'team-of-teams'
  return (
    <Link href={`/organizations/${teamRoute}/${data.child.key}`}>{value}</Link>
  )
}

const ParentLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.parent.type === 'Team' ? 'teams' : 'team-of-teams'
  return (
    <Link href={`/organizations/${teamRoute}/${data.parent.key}`}>{value}</Link>
  )
}

const TeamMembershipsGrid = ({
  teamMembershipsQuery,
}: TeamMembershipsGridProps) => {
  const refresh = useCallback(async () => {
    teamMembershipsQuery.refetch()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const columnDefs = useMemo(
    () => [
      { field: 'child.name', cellRenderer: ChildLinkCellRenderer },
      { field: 'parent.name', cellRenderer: ParentLinkCellRenderer },
      { field: 'state' },
      {
        field: 'start',
        valueGetter: (params) => dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        valueGetter: (params) =>
          params.data.end ? dayjs(params.data.end).format('M/D/YYYY') : null,
      },
    ],
    [],
  )

  return (
    <>
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={teamMembershipsQuery.data}
        loadData={refresh}
      />
    </>
  )
}

export default TeamMembershipsGrid
