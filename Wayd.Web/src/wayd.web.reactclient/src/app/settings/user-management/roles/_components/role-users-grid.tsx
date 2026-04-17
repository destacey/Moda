'use client'

import { WaydGrid } from '@/src/components/common'
import { UserLinkCellRenderer } from '@/src/components/common/wayd-grid-cell-renderers'
import { RoleListDto, UserDetailsDto } from '@/src/services/wayd-api'
import { useGetRoleUsersQuery } from '@/src/store/features/user-management/roles-api'
import { ColDef } from 'ag-grid-community'
import { FC, useMemo } from 'react'

export interface RoleUsersGridProps {
  roleId: string
}

const RoleUsersGrid: FC<RoleUsersGridProps> = (props: RoleUsersGridProps) => {
  const {
    data: usersData,
    isLoading,
    refetch,
  } = useGetRoleUsersQuery(props.roleId)

  const columnDefs = useMemo<ColDef<UserDetailsDto>[]>(() => [
      { field: 'id', hide: true },
      { field: 'userName', cellRenderer: UserLinkCellRenderer },
      { field: 'firstName' },
      { field: 'lastName' },
      {
        field: 'roles',
        valueFormatter: (params) =>
          params.value
            ?.map((r: RoleListDto) => r.name)
            .sort()
            .join(', ') ?? '',
      },
      { field: 'isActive' }, // TODO: convert to yes/no
    ], [])

  const refresh = async () => {
    refetch()
  }

  return (
    <WaydGrid
      height={550}
      columnDefs={columnDefs}
      rowData={usersData}
      loadData={refresh}
      loading={isLoading}
    />
  )
}

export default RoleUsersGrid
