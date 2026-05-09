'use client'

import { useState, useMemo } from 'react'
import WaydGrid from '@/src/components/common/wayd-grid'
import { RowMenuCellRenderer } from '@/src/components/common/wayd-grid-cell-renderers'
import useAuth from '@/src/components/contexts/auth'
import { ItemType } from 'antd/es/menu/interface'
import { Tag } from 'antd'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import Link from 'next/link'
import {
  TeamMemberDto,
  useGetTeamMembersQuery,
  useGetTeamOfTeamsMembersQuery,
} from '@/src/store/features/organization/team-members-api'
import AddTeamMemberForm from './add-team-member-form'
import EditTeamMemberForm from './edit-team-member-form'
import RemoveTeamMemberForm from './remove-team-member-form'

interface TeamMembersGridProps {
  teamId: string
  teamType: 'Team' | 'TeamOfTeams'
  teamIsActive: boolean
  openAddForm?: boolean
  onAddFormClose?: () => void
}

const NameCellRenderer = ({ data }: ICellRendererParams<TeamMemberDto>) => {
  if (!data) return null
  return <Link href={`/organizations/employees/${data.employee.key}`}>{data.employee.name}</Link>
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

const TeamMembersGrid = ({ teamId, teamType, teamIsActive, openAddForm, onAddFormClose }: TeamMembersGridProps) => {
  const [editingMember, setEditingMember] = useState<TeamMemberDto | null>(null)
  const [removingMember, setRemovingMember] = useState<TeamMemberDto | null>(null)

  const { hasPermissionClaim } = useAuth()
  const canUpdate = hasPermissionClaim('Permissions.Teams.Update')

  const { data: teamMembers, isLoading: teamLoading, refetch: refetchTeam } = useGetTeamMembersQuery(
    { teamId },
    { skip: !teamId || teamType !== 'Team' },
  )
  const { data: totMembers, isLoading: totLoading, refetch: refetchTot } = useGetTeamOfTeamsMembersQuery(
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
        { key: 'remove', label: 'Remove', danger: true, onClick: () => setRemovingMember(member) },
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
          return RowMenuCellRenderer({ ...params, menuItems: getRowMenuItems(params.data) })
        },
      },
      {
        field: 'employee.name',
        headerName: 'Name',
        cellRenderer: NameCellRenderer,
        flex: 1,
      },
      {
        field: 'employee.email',
        headerName: 'Email',
        flex: 1,
      },
      {
        field: 'employee.jobTitle',
        headerName: 'Title',
        width: 200,
      },
      {
        colId: 'roles',
        headerName: 'Roles',
        cellRenderer: RolesCellRenderer,
        valueGetter: (params) => params.data?.roles.map((r) => r.name).join(', '),
        autoHeight: true,
        flex: 1,
      },
    ]
  }, [canUpdate])

  return (
    <>
      <WaydGrid
        height={550}
        columnDefs={columnDefs}
        rowData={members}
        loading={isLoading}
        loadData={() => { refetch() }}
      />
      {openAddForm && teamIsActive && (
        <AddTeamMemberForm
          teamId={teamId}
          teamType={teamType}
          onFormComplete={() => { onAddFormClose?.(); refetch() }}
          onFormCancel={() => onAddFormClose?.()}
        />
      )}
      {editingMember && (
        <EditTeamMemberForm
          teamId={teamId}
          teamType={teamType}
          member={editingMember}
          onFormComplete={() => { setEditingMember(null); refetch() }}
          onFormCancel={() => setEditingMember(null)}
        />
      )}
      {removingMember && (
        <RemoveTeamMemberForm
          teamId={teamId}
          teamType={teamType}
          member={removingMember}
          onFormComplete={() => { setRemovingMember(null); refetch() }}
          onFormCancel={() => setRemovingMember(null)}
        />
      )}
    </>
  )
}

export default TeamMembersGrid
