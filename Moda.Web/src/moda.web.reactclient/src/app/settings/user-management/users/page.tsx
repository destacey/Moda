'use client'

import PageTitle from '@/src/components/common/page-title'
import { useEffect, useMemo, useState } from 'react'
import ModaGrid from '@/src/components/common/moda-grid'
import { authorizePage } from '@/src/components/hoc'
import Link from 'next/link'
import { Button, Tag } from 'antd'
import { ModaTooltip } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { useDocumentTitle } from '@/src/hooks'
import { useGetUsersQuery } from '@/src/store/features/user-management/users-api'
import { RoleListDto, UserDetailsDto } from '@/src/services/moda-api'
import { ColDef, ValueFormatterParams } from 'ag-grid-community'
import dayjs from 'dayjs'
import {
  RowMenuCellRenderer,
  UserLinkCellRenderer,
} from '@/src/components/common/moda-grid-cell-renderers'
import {
  CreateUserForm,
  EditUserForm,
  ManageUserRolesForm,
  ResetPasswordForm,
  useUserAccountActions,
} from './_components'
import { ItemType } from 'antd/es/menu/interface'

const EmployeeLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/employees/${data.employee?.key}`}>{value}</Link>
  )
}

const dateTimeValueFormatter = (
  params: ValueFormatterParams<UserDetailsDto>,
) => (params.value ? dayjs(params.value).format('YYYY-MM-DD h:mm A') : '')

const UsersListPage = () => {
  useDocumentTitle('Users')
  const [openCreateUserForm, setOpenCreateUserForm] = useState(false)
  const [editingUser, setEditingUser] = useState<UserDetailsDto | null>(null)
  const [managingRolesUserId, setManagingRolesUserId] = useState<string | null>(
    null,
  )
  const [resettingPasswordUser, setResettingPasswordUser] =
    useState<UserDetailsDto | null>(null)

  const { hasClaim } = useAuth()
  const canCreateUser = hasClaim('Permission', 'Permissions.Users.Create')
  const canUpdateUser = hasClaim('Permission', 'Permissions.Users.Update')
  const canUpdateUserRoles = hasClaim(
    'Permission',
    'Permissions.UserRoles.Update',
  )
  const showRowActions = canUpdateUser || canUpdateUserRoles

  const { getAccountActionMenuItems } = useUserAccountActions()
  const { data: usersData, isLoading, error, refetch } = useGetUsersQuery()

  const columnDefs = useMemo<ColDef<UserDetailsDto>[]>(() => [
      {
        width: 50,
        filter: false,
        sortable: false,
        hide: !showRowActions,
        suppressHeaderMenuButton: true,
        cellRenderer: (params) => {
          const menuItems: ItemType[] = []
          if (canUpdateUser) {
            menuItems.push({
              key: 'edit',
              label: 'Edit',
              onClick: () => setEditingUser(params.data),
            })
            menuItems.push(
              ...getAccountActionMenuItems({
                id: params.data.id,
                userName: params.data.userName,
                firstName: params.data.firstName,
                lastName: params.data.lastName,
                isActive: params.data.isActive,
                isLockedOut:
                  !!params.data.lockoutEnd &&
                  new Date(params.data.lockoutEnd) > new Date(),
              }),
            )
          }
          const secondaryItems: ItemType[] = []
          if (canUpdateUser && params.data.loginProvider === 'Moda') {
            secondaryItems.push({
              key: 'reset-password',
              label: 'Reset Password',
              onClick: () => setResettingPasswordUser(params.data),
            })
          }
          if (canUpdateUserRoles) {
            secondaryItems.push({
              key: 'manage-roles',
              label: 'Manage Roles',
              onClick: () => setManagingRolesUserId(params.data.id),
            })
          }
          if (secondaryItems.length > 0 && menuItems.length > 0) {
            menuItems.push({ key: 'divider', type: 'divider' })
          }
          menuItems.push(...secondaryItems)
          return <RowMenuCellRenderer {...params} menuItems={menuItems} />
        },
      },
      { field: 'id', hide: true },
      { field: 'userName', cellRenderer: UserLinkCellRenderer },
      { field: 'firstName' },
      { field: 'lastName' },
      { field: 'email' },
      {
        field: 'loginProvider',
        headerName: 'Login Provider',
        valueFormatter: (params) =>
          params.value === 'MicrosoftEntraId'
            ? 'Microsoft Entra ID'
            : (params.value ?? ''),
      },
      {
        field: 'employee.name',
        headerName: 'Employee',
        cellRenderer: EmployeeLinkCellRenderer,
      },
      {
        field: 'roles',
        valueFormatter: (params) =>
          params.value
            ?.map((r: RoleListDto) => r.name)
            .sort()
            .join(', ') ?? '',
      },
      {
        field: 'lockoutEnd',
        headerName: 'Locked Out',
        width: 120,
        cellRenderer: (params: { value: Date | undefined }) =>
          params.value && new Date(params.value) > new Date() ? (
            <ModaTooltip
              title={`Locked until ${dayjs(params.value).format('MMM D, YYYY h:mm A')}`}
            >
              <Tag color="error">Locked</Tag>
            </ModaTooltip>
          ) : null,
      },
      {
        field: 'lastActivityAt',
        headerName: 'Last Activity',
        valueFormatter: dateTimeValueFormatter,
      },
      {
        field: 'isActive',
        headerName: 'Active',
        width: 100,
        cellRenderer: (params: { value: boolean | undefined }) =>
          params.value ? (
            <Tag color="success">Active</Tag>
          ) : (
            <Tag color="error">Inactive</Tag>
          ),
      },
    ], [showRowActions, canUpdateUser, canUpdateUserRoles, getAccountActionMenuItems, setEditingUser, setResettingPasswordUser, setManagingRolesUserId])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const refresh = async () => {
    refetch()
  }

  const actions = () => {
    return (
      <>
        {canCreateUser && (
          <Button onClick={() => setOpenCreateUserForm(true)}>
            Create User
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      <PageTitle title="Users" actions={actions()} />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        rowData={usersData}
        loadData={refresh}
        loading={isLoading}
      />

      {openCreateUserForm && (
        <CreateUserForm
          onFormCreate={() => {
            setOpenCreateUserForm(false)
            refetch()
          }}
          onFormCancel={() => setOpenCreateUserForm(false)}
        />
      )}
      {editingUser && (
        <EditUserForm
          user={editingUser}
          onFormUpdate={() => setEditingUser(null)}
          onFormCancel={() => setEditingUser(null)}
        />
      )}
      {managingRolesUserId && (
        <ManageUserRolesForm
          userId={managingRolesUserId}
          onFormComplete={() => setManagingRolesUserId(null)}
          onFormCancel={() => setManagingRolesUserId(null)}
        />
      )}
      {resettingPasswordUser && (
        <ResetPasswordForm
          userId={resettingPasswordUser.id}
          userName={`${resettingPasswordUser.firstName} ${resettingPasswordUser.lastName}`}
          onFormComplete={() => setResettingPasswordUser(null)}
          onFormCancel={() => setResettingPasswordUser(null)}
        />
      )}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  UsersListPage,
  'Permission',
  'Permissions.Users.View',
)

export default PageWithAuthorization
