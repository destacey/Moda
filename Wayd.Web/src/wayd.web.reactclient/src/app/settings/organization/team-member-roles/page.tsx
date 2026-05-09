'use client'

import { WaydGrid, PageTitle } from '@/src/components/common'
import { RowMenuCellRenderer } from '@/src/components/common/wayd-grid-cell-renderers'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { TeamMemberRoleDto } from '@/src/services/wayd-api'
import {
  useGetTeamMemberRolesQuery,
  useActivateTeamMemberRoleMutation,
  useDeactivateTeamMemberRoleMutation,
} from '@/src/store/features/organization/team-member-roles-api'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import { Button } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import { ControlItemSwitch } from '@/src/components/common/control-items-menu'
import { ItemType } from 'antd/es/menu/interface'
import { useMessage } from '@/src/components/contexts/messaging'
import { isApiError } from '@/src/utils'
import CreateTeamMemberRoleForm from './_components/create-team-member-role-form'
import EditTeamMemberRoleForm from './_components/edit-team-member-role-form'
import DeleteTeamMemberRoleForm from './_components/delete-team-member-role-form'

const TeamMemberRolesPage = () => {
  useDocumentTitle('Organization - Team Member Roles')

  const [includeInactive, setIncludeInactive] = useState(false)
  const [openCreateForm, setOpenCreateForm] = useState(false)
  const [editingRole, setEditingRole] = useState<TeamMemberRoleDto | null>(null)
  const [deletingRole, setDeletingRole] = useState<TeamMemberRoleDto | null>(null)

  const messageApi = useMessage()
  const { hasPermissionClaim } = useAuth()

  const canCreate = hasPermissionClaim('Permissions.TeamMemberRoles.Create')
  const canUpdate = hasPermissionClaim('Permissions.TeamMemberRoles.Update')
  const canDelete = hasPermissionClaim('Permissions.TeamMemberRoles.Delete')

  const { data: roles, isLoading, error, refetch } = useGetTeamMemberRolesQuery(includeInactive)
  const [activateRole] = useActivateTeamMemberRoleMutation()
  const [deactivateRole] = useDeactivateTeamMemberRoleMutation()

  useEffect(() => {
    if (error) {
      messageApi.error(
        (isApiError(error) ? error.detail : undefined) ??
          'An error occurred while loading team member roles.',
      )
      console.error(error)
    }
  }, [error, messageApi])

  const columnDefs = useMemo<ColDef<TeamMemberRoleDto>[]>(() => {
    const getRowMenuItems = (role: TeamMemberRoleDto): ItemType[] => {
      const items: ItemType[] = []
      if (canUpdate) {
        items.push({ key: 'edit', label: 'Edit', onClick: () => setEditingRole(role) })
        if (role.isActive) {
          items.push({
            key: 'deactivate',
            label: 'Deactivate',
            onClick: async () => {
              const response = await deactivateRole(role.id)
              if (response.error) {
                messageApi.error(
                  (isApiError(response.error) ? response.error.detail : undefined) ??
                    'Failed to deactivate role.',
                )
              } else {
                messageApi.success(`"${role.name}" deactivated.`)
              }
            },
          })
        } else {
          items.push({
            key: 'activate',
            label: 'Activate',
            onClick: async () => {
              const response = await activateRole(role.id)
              if (response.error) {
                messageApi.error(
                  (isApiError(response.error) ? response.error.detail : undefined) ??
                    'Failed to activate role.',
                )
              } else {
                messageApi.success(`"${role.name}" activated.`)
              }
            },
          })
        }
      }
      if (canDelete) {
        items.push({ key: 'delete', label: 'Delete', danger: true, onClick: () => setDeletingRole(role) })
      }
      return items
    }

    return [
      {
        width: 50,
        filter: false,
        sortable: false,
        hide: !canUpdate && !canDelete,
        suppressHeaderMenuButton: true,
        cellRenderer: (params: ICellRendererParams<TeamMemberRoleDto>) => {
          if (!params.data) return null
          return RowMenuCellRenderer({ ...params, menuItems: getRowMenuItems(params.data) })
        },
      },
      { field: 'key', width: 90 },
      { field: 'name', flex: 1 },
      { field: 'isActive', headerName: 'Active', width: 90 },
    ]
  }, [canUpdate, canDelete, activateRole, deactivateRole, messageApi])

  const controlItems: ItemType[] = [
    {
      label: (
        <ControlItemSwitch
          label="Include Inactive"
          checked={includeInactive}
          onChange={(checked) => setIncludeInactive(checked)}
        />
      ),
      key: 'include-inactive',
      onClick: () => setIncludeInactive((prev) => !prev),
    },
  ]

  const actions = canCreate ? (
    <Button onClick={() => setOpenCreateForm(true)}>Create Team Member Role</Button>
  ) : null

  return (
    <>
      <PageTitle title="Team Member Roles" actions={actions} />

      <WaydGrid
        height={600}
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={roles}
        loadData={() => { refetch() }}
        loading={isLoading}
      />

      {openCreateForm && (
        <CreateTeamMemberRoleForm
          onFormComplete={() => { setOpenCreateForm(false); refetch() }}
          onFormCancel={() => setOpenCreateForm(false)}
        />
      )}
      {editingRole && (
        <EditTeamMemberRoleForm
          role={editingRole}
          onFormComplete={() => { setEditingRole(null); refetch() }}
          onFormCancel={() => setEditingRole(null)}
        />
      )}
      {deletingRole && (
        <DeleteTeamMemberRoleForm
          role={deletingRole}
          onFormComplete={() => { setDeletingRole(null); refetch() }}
          onFormCancel={() => setDeletingRole(null)}
        />
      )}
    </>
  )
}

const TeamMemberRolesPageWithAuthorization = authorizePage(
  TeamMemberRolesPage,
  'Permission',
  'Permissions.TeamMemberRoles.View',
)

export default TeamMemberRolesPageWithAuthorization
