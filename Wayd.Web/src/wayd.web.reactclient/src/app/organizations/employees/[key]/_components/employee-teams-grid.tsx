'use client'

import { useMemo } from 'react'
import WaydGrid from '@/src/components/common/wayd-grid'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import { Flex, Tag, Tooltip } from 'antd'
import Link from 'next/link'
import {
  TeamMemberDto,
  useGetEmployeeTeamMembershipsQuery,
} from '@/src/store/features/organization/team-members-api'
import { useGetTeamMemberRolesQuery } from '@/src/store/features/organization/team-member-roles-api'

interface Props {
  employeeId: string
}

const TeamLinkCellRenderer = ({ data }: ICellRendererParams<TeamMemberDto>) => {
  if (!data) return null
  return (
    <Link href={`/organizations/teams/${data.team.key}`}>{data.team.name}</Link>
  )
}

const EmployeeTeamsGrid = ({ employeeId }: Props) => {
  const {
    data: memberships,
    isLoading,
    refetch,
  } = useGetEmployeeTeamMembershipsQuery({ employeeId }, { skip: !employeeId })

  const { data: allRoles } = useGetTeamMemberRolesQuery(false)
  const roleDescriptionById = useMemo(() => {
    const map = new Map<string, string | undefined>()
    allRoles?.forEach((r) => map.set(r.id, r.description ?? undefined))
    return map
  }, [allRoles])

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
        cellRenderer: ({ data }: ICellRendererParams<TeamMemberDto>) => {
          if (!data) return null
          return (
            <Flex wrap gap={4}>
              {data.roles.map((role) => (
                <Tooltip
                  key={role.id}
                  title={roleDescriptionById.get(role.id)}
                  placement="top"
                >
                  <Tag variant="filled">{role.name}</Tag>
                </Tooltip>
              ))}
            </Flex>
          )
        },
        valueGetter: (params) =>
          params.data?.roles.map((r) => r.name).join(', '),
        cellStyle: { display: 'flex', alignItems: 'center' },
        flex: 1,
      },
    ],
    [roleDescriptionById],
  )

  return (
    <WaydGrid
      height={500}
      columnDefs={columnDefs}
      rowData={memberships}
      loading={isLoading}
      loadData={() => {
        refetch()
      }}
    />
  )
}

export default EmployeeTeamsGrid
