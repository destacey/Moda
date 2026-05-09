'use client'

import { useMemo } from 'react'
import WaydGrid from '@/src/components/common/wayd-grid'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import { Tag } from 'antd'
import Link from 'next/link'
import {
  TeamMemberDto,
  useGetEmployeeTeamMembershipsQuery,
} from '@/src/store/features/organization/team-members-api'

interface Props {
  employeeId: string
}

const TeamLinkCellRenderer = ({ data }: ICellRendererParams<TeamMemberDto>) => {
  if (!data) return null
  return <Link href={`/organizations/teams/${data.team.key}`}>{data.team.name}</Link>
}

const RolesCellRenderer = ({ data }: ICellRendererParams<TeamMemberDto>) => {
  if (!data) return null
  return (
    <div style={{ display: 'flex', flexWrap: 'wrap', gap: 4, alignItems: 'center', height: '100%' }}>
      {data.roles.map((role) => (
        <Tag key={role.id} bordered={false}>{role.name}</Tag>
      ))}
    </div>
  )
}

const EmployeeTeamsGrid = ({ employeeId }: Props) => {
  const { data: memberships, isLoading, refetch } = useGetEmployeeTeamMembershipsQuery(
    { employeeId },
    { skip: !employeeId },
  )

  const columnDefs = useMemo<ColDef<TeamMemberDto>[]>(
    () => [
      {
        field: 'team.name',
        headerName: 'Team',
        cellRenderer: TeamLinkCellRenderer,
        flex: 1,
      },
      { field: 'team.type', headerName: 'Type', width: 150 },
      {
        colId: 'roles',
        headerName: 'Roles',
        cellRenderer: RolesCellRenderer,
        valueGetter: (params) => params.data?.roles.map((r) => r.name).join(', '),
        autoHeight: true,
        flex: 1,
      },
    ],
    [],
  )

  return (
    <WaydGrid
      height={500}
      columnDefs={columnDefs}
      rowData={memberships}
      loading={isLoading}
      loadData={() => { refetch() }}
    />
  )
}

export default EmployeeTeamsGrid
