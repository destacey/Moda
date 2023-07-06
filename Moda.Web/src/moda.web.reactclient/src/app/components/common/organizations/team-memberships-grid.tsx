import Link from 'next/link'
import { useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { TeamMembershipsDto } from '@/src/services/moda-api'

export interface TeamMembershipsGridProps {
  teamMemberships: TeamMembershipsDto[]
}

const ChildLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.child.type === 'Team' ? 'teams' : 'team-of-teams'
  return (
    <Link href={`/organizations/${teamRoute}/${data.child.localId}`}>
      {value}
    </Link>
  )
}

const ParentLinkCellRenderer = ({ value, data }) => {
  const teamRoute = data.parent.type === 'Team' ? 'teams' : 'team-of-teams'
  return (
    <Link href={`/organizations/${teamRoute}/${data.parent.localId}`}>
      {value}
    </Link>
  )
}

const TeamMembershipsGrid = ({ teamMemberships }: TeamMembershipsGridProps) => {
  const columnDefs = useMemo(
    () => [
      { field: 'child.name', cellRenderer: ChildLinkCellRenderer },
      { field: 'parent.name', cellRenderer: ParentLinkCellRenderer },
      { field: 'state' },
      // TODO: fix these dates
      {
        field: 'start',
        valueGetter: (params) => new Date(params.data.start).toUTCString(),
      },
      {
        field: 'end',
        valueGetter: (params) =>
          params.data.end ? new Date(params.data.end).toUTCString() : null,
      },
    ],
    []
  )

  return (
    <>
      <ModaGrid columnDefs={columnDefs} rowData={teamMemberships} />
    </>
  )
}

export default TeamMembershipsGrid
