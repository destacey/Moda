'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import { Card, MenuProps, Spin, Typography } from 'antd'
import { use, useCallback, useMemo, useState } from 'react'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { notFound, useRouter } from 'next/navigation'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import { useGetRoleQuery } from '@/src/store/features/user-management/roles-api'
import { DeleteRoleForm, EditRoleForm, Permissions } from '../_components'
import { ItemType } from 'antd/es/menu/interface'

const { Text } = Typography

enum RoleDetailsTabs {
  Permissions = 'permissions',
}

const getRoleTabs = (canViewPermissions: boolean) => [
  canViewPermissions && {
    key: RoleDetailsTabs.Permissions,
    tab: 'Permissions',
  },
]

const RoleDetailsPage = (props: { params: Promise<{ id: string }> }) => {
  const { id } = use(props.params)

  const [activeTab, setActiveTab] = useState(RoleDetailsTabs.Permissions)
  const [permissionsDirty, setPermissionsDirty] = useState(false)
  const [openEditRoleForm, setOpenEditRoleForm] = useState<boolean>(false)
  const [openDeleteRoleForm, setOpenDeleteRoleForm] = useState<boolean>(false)

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()

  //const canUpdateRole = hasPermissionClaim('Permissions.Roles.Update')
  const canViewPermissions = hasPermissionClaim('Permissions.Permissions.View')

  const {
    data: roleData,
    isLoading: isLoading,
    error: error,
    refetch: refetch,
  } = useGetRoleQuery(id)

  const tabs = useMemo(
    () => getRoleTabs(canViewPermissions),
    [canViewPermissions],
  )

  const editableRole =
    roleData && roleData.name !== 'Admin' && roleData.name !== 'Basic'
  const canUpdateRole =
    hasPermissionClaim('Permissions.Roles.Update') && editableRole
  const canDeleteRole =
    hasPermissionClaim('Permissions.Roles.Delete') && editableRole

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canUpdateRole) {
      items.push({
        key: 'edit',
        label: 'Edit',
        onClick: () => setOpenEditRoleForm(true),
      })
    }
    if (canDeleteRole) {
      items.push({
        key: 'delete',
        label: 'Delete',
        onClick: () => setOpenDeleteRoleForm(true),
      })
    }
    return items
  }, [canDeleteRole, canUpdateRole])

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case RoleDetailsTabs.Permissions:
        return (
          <Permissions
            role={roleData}
            permissions={roleData?.permissions}
            onDirtyChange={setPermissionsDirty}
          />
        )
      default:
        return null
    }
  }, [activeTab, roleData])

  const onTabChange = useCallback(
    (tabKey: string) => {
      if (
        permissionsDirty &&
        !window.confirm(
          'You have unsaved permission changes. Are you sure you want to leave?',
        )
      )
        return
      setPermissionsDirty(false)
      setActiveTab(tabKey as RoleDetailsTabs)
    },
    [permissionsDirty],
  )

  const onDeleteRoleFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteRoleForm(false)
      if (wasDeleted) {
        router.push('/settings/user-management/roles')
      }
    },
    [router],
  )

  if (!isLoading && !roleData) {
    return notFound()
  }

  return (
    <>
      <BasicBreadcrumb
        items={[
          { title: 'Settings' },
          { title: 'User Management' },
          { title: 'Roles', href: './' },
          { title: 'Details' },
        ]}
      />
      <PageTitle
        title={roleData?.name}
        subtitle="Role Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      {roleData?.description && (
        <div style={{ marginBottom: 16 }}>
          <Text type="secondary">{roleData?.description}</Text>
        </div>
      )}
      <Card tabList={tabs} activeTabKey={activeTab} onTabChange={onTabChange}>
        <Spin spinning={isLoading}>{!isLoading && renderTabContent()}</Spin>
      </Card>
      {openEditRoleForm && (
        <EditRoleForm
          role={roleData}
          showForm={openEditRoleForm}
          onFormComplete={() => {
            setOpenEditRoleForm(false)
            refetch()
          }}
          onFormCancel={() => setOpenEditRoleForm(false)}
        />
      )}
      {openDeleteRoleForm && (
        <DeleteRoleForm
          role={roleData}
          showForm={openDeleteRoleForm}
          onFormComplete={() => onDeleteRoleFormClosed(true)}
          onFormCancel={() => onDeleteRoleFormClosed(false)}
        />
      )}
    </>
  )
}

const RoleDetailsPageWithAuthorization = authorizePage(
  RoleDetailsPage,
  'Permission',
  'Permissions.Roles.View',
)

export default RoleDetailsPageWithAuthorization
