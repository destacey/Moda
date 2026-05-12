'use client'

import { useState, useMemo } from 'react'
import WaydGrid from '@/src/components/common/wayd-grid'
import { RowMenuCellRenderer } from '@/src/components/common/wayd-grid-cell-renderers'
import useAuth from '@/src/components/contexts/auth'
import { ItemType } from 'antd/es/menu/interface'
import { Flex, Tag, Tooltip } from 'antd'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import Link from 'next/link'
import {
  TeamMemberDto,
  useGetTeamMembersQuery,
  useGetTeamOfTeamsMembersQuery,
} from '@/src/store/features/organization/team-members-api'
import { useGetTeamMemberRolesQuery } from '@/src/store/features/organization/team-member-roles-api'
import EditTeamMemberForm from './edit-team-member-form'
import RemoveTeamMemberForm from './remove-team-member-form'

interface TeamMembersGridProps {
  teamId: string
  teamType: 'Team' | 'TeamOfTeams'
}

const NameCellRenderer = ({ data }: ICellRendererParams<TeamMemberDto>) => {
  if (!data) return null
  return (
    <Link href={`/organizations/employees/${data.employee.key}`}>
      {data.employee.name}
    </Link>
  )
}

const TeamMembersGrid = ({
  teamId,
  teamType,
}: TeamMembersGridProps) => {
  const [editingMember, setEditingMember] = useState<TeamMemberDto | null>(null)
  const [removingMember, setRemovingMember] = useState<TeamMemberDto | null>(
    null,
  )

  const { hasPermissionClaim } = useAuth()
  const canUpdate = hasPermissionClaim('Permissions.Teams.Update')

  const { data: allRoles } = useGetTeamMemberRolesQuery(false)
  const roleDescriptionById = useMemo(() => {
    const map = new Map<string, string | undefined>()
    allRoles?.forEach((r) => map.set(r.id, r.description ?? undefined))
    return map
  }, [allRoles])

  const {
    data: teamMembers,
    isLoading: teamLoading,
    refetch: refetchTeam,
  } = useGetTeamMembersQuery(
    { teamId },
    { skip: !teamId || teamType !== 'Team' },
  )
  const {
    data: totMembers,
    isLoading: totLoading,
    refetch: refetchTot,
  } = useGetTeamOfTeamsMembersQuery(
    { teamId },
    { skip: !teamId || teamType !== 'TeamOfTeams' },
  )

  const members = teamType === 'Team' ? teamMembers : totMembers
  const isLoading = teamType === 'Team' ? teamLoading : totLoading
  const refetch = teamType === 'Team' ? refetchTeam : refetchTot

  const columnDefs = useMemo<ColDef<TeamMemberDto>[]>(() => {
    const getRowMenuItems = (member: TeamMemberDto): ItemType[] => {
      if (!canUpdate) return []
      return [
        { key: 'edit', label: 'Edit', onClick: () => setEditingMember(member) },
        {
          key: 'remove',
          label: 'Remove',
          danger: true,
          onClick: () => setRemovingMember(member),
        },
      ]
    }

    return [
      {
        width: 50,
        filter: false,
        sortable: false,
        hide: !canUpdate,
        suppressHeaderMenuButton: true,
        cellRenderer: (params: ICellRendererParams<TeamMemberDto>) => {
          if (!params.data) return null
          return RowMenuCellRenderer({
            ...params,
            menuItems: getRowMenuItems(params.data),
          })
        },
      },
      {
        field: 'employee.name',
        headerName: 'Name',
        cellRenderer: NameCellRenderer,
        flex: 1,
      },
      {
        field: 'employee.jobTitle',
        headerName: 'Title',
        width: 200,
      },
      {
        field: 'employee.email',
        headerName: 'Email',
        flex: 1,
      },
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
    ]
  }, [canUpdate, roleDescriptionById])

  return (
    <>
      <WaydGrid
        height={550}
        columnDefs={columnDefs}
        rowData={members}
        loading={isLoading}
        loadData={() => {
          refetch()
        }}
      />
      {editingMember && (
        <EditTeamMemberForm
          teamId={teamId}
          teamType={teamType}
          member={editingMember}
          onFormComplete={() => {
            setEditingMember(null)
            refetch()
          }}
          onFormCancel={() => setEditingMember(null)}
        />
      )}
      {removingMember && (
        <RemoveTeamMemberForm
          teamId={teamId}
          teamType={teamType}
          member={removingMember}
          onFormComplete={() => {
            setRemovingMember(null)
            refetch()
          }}
          onFormCancel={() => setRemovingMember(null)}
        />
      )}
    </>
  )
}

export default TeamMembersGrid
