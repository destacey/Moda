'use client'

import { PageActions } from '@/src/components/common'
import PageTitle from '@/src/components/common/page-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound } from 'next/navigation'
import UserDetailsLoading from './loading'
import { use, useEffect, useState } from 'react'
import { App, Card, MenuProps } from 'antd'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import useAuth from '@/src/components/contexts/auth'
import {
  useCancelTenantMigrationMutation,
  useGetUserQuery,
} from '@/src/store/features/user-management/users-api'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  EditUserForm,
  ManageUserRolesForm,
  ResetPasswordForm,
  StageTenantMigrationForm,
  useUserAccountActions,
  UserDetails,
  UserIdentityHistory,
} from '../_components'
import { ItemType } from 'antd/es/menu/interface'
import { useDocumentTitle } from '@/src/hooks'

enum UserDetailsTabs {
  Details = 'details',
  IdentityHistory = 'identity-history',
}

const UserDetailsPage = (props: { params: Promise<{ id: string }> }) => {
  const { id } = use(props.params)

  const [activeTab, setActiveTab] = useState(UserDetailsTabs.Details)
  const [openEditUserForm, setOpenEditUserForm] = useState(false)
  const [openManageUserRolesForm, setOpenManageUserRolesForm] = useState(false)
  const [openResetPasswordForm, setOpenResetPasswordForm] = useState(false)
  const [openStageMigrationForm, setOpenStageMigrationForm] = useState(false)

  const { hasPermissionClaim } = useAuth()
  const canUpdateUser = hasPermissionClaim('Permissions.Users.Update')
  const canUpdateUserRoles = hasPermissionClaim('Permissions.UserRoles.Update')

  const { getAccountActionMenuItems } = useUserAccountActions()
  const { data: userData, isLoading, error } = useGetUserQuery(id)
  const [cancelTenantMigration] = useCancelTenantMigrationMutation()
  const { modal } = App.useApp()
  const messageApi = useMessage()

  const isLocalUser = userData?.loginProvider === 'Wayd'
  const isEntraUser = userData?.loginProvider === 'MicrosoftEntraId'
  const hasPendingMigration = !!userData?.pendingMigrationTenantId

  const tabs = [
    { key: UserDetailsTabs.Details, tab: 'Details' },
    ...(isEntraUser
      ? [
          {
            key: UserDetailsTabs.IdentityHistory,
            tab: 'Identity History',
          },
        ]
      : []),
  ]

  const fullName = userData
    ? `${userData?.firstName} ${userData?.lastName}`
    : ''
  const title = userData ? `${fullName} - User Details` : 'User Details'
  useDocumentTitle(title)

  const actionsMenuItems: MenuProps['items'] = (() => {
    if (!userData) return []

    const items: ItemType[] = []
    if (canUpdateUser) {
      items.push({
        key: 'edit',
        label: 'Edit',
        onClick: () => setOpenEditUserForm(true),
      })
      items.push(
        ...getAccountActionMenuItems({
          id: userData.id,
          userName: userData.userName!,
          firstName: userData.firstName!,
          lastName: userData.lastName!,
          isActive: userData.isActive,
          isLockedOut:
            !!userData.lockoutEnd && new Date(userData.lockoutEnd) > new Date(),
        }),
      )
    }
    const secondaryItems: ItemType[] = []
    if (canUpdateUser && isLocalUser) {
      secondaryItems.push({
        key: 'reset-password',
        label: 'Reset Password',
        onClick: () => setOpenResetPasswordForm(true),
      })
    }
    if (canUpdateUser && isEntraUser) {
      secondaryItems.push({
        key: 'stage-migration',
        label: hasPendingMigration
          ? 'Replace Pending Migration'
          : 'Migrate to New Tenant',
        onClick: () => setOpenStageMigrationForm(true),
      })
      if (hasPendingMigration) {
        secondaryItems.push({
          key: 'cancel-migration',
          label: 'Cancel Pending Migration',
          onClick: () => {
            modal.confirm({
              title: 'Cancel Pending Migration',
              content: `Cancel the pending tenant migration for ${fullName}?`,
              okText: 'Cancel Migration',
              okButtonProps: { danger: true },
              cancelText: 'Keep Pending',
              onOk: async () => {
                try {
                  const result = await cancelTenantMigration(userData.id)
                  if ('error' in result) {
                    throw result.error
                  }
                  messageApi.success('Pending migration canceled.')
                } catch (err: any) {
                  messageApi.error(
                    err?.data?.detail ??
                      'Failed to cancel the pending migration.',
                  )
                }
              },
            })
          },
        })
      }
    }
    if (canUpdateUserRoles) {
      secondaryItems.push({
        key: 'manage-roles',
        label: 'Manage Roles',
        onClick: () => setOpenManageUserRolesForm(true),
      })
    }
    if (secondaryItems.length > 0 && items.length > 0) {
      items.push({ key: 'divider', type: 'divider' })
    }
    items.push(...secondaryItems)
    return items
  })()

  const renderTabContent = () => {
    switch (activeTab) {
      case UserDetailsTabs.Details:
        return <UserDetails user={userData!} />
      case UserDetailsTabs.IdentityHistory:
        return <UserIdentityHistory userId={userData!.id} />
      default:
        return null
    }
  }

  const onTabChange = (tabKey: string) => {
    setActiveTab(tabKey as UserDetailsTabs)
  }

  useEffect(() => {
    error && console.error(error)
  }, [error])

  if (isLoading) {
    return <UserDetailsLoading />
  }

  if (!userData) {
    return notFound()
  }

  return (
    <>
      <BasicBreadcrumb
        items={[
          { title: 'Settings' },
          { title: 'User Management' },
          { title: 'Users', href: './' },
          { title: 'Details' },
        ]}
      />
      <PageTitle
        title={fullName}
        subtitle="User Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {renderTabContent()}
      </Card>
      {openEditUserForm && (
        <EditUserForm
          user={userData}
          onFormUpdate={() => setOpenEditUserForm(false)}
          onFormCancel={() => setOpenEditUserForm(false)}
        />
      )}
      {openManageUserRolesForm && (
        <ManageUserRolesForm
          userId={userData.id}
          onFormComplete={() => setOpenManageUserRolesForm(false)}
          onFormCancel={() => setOpenManageUserRolesForm(false)}
        />
      )}
      {openResetPasswordForm && (
        <ResetPasswordForm
          userId={userData.id}
          userName={fullName}
          onFormComplete={() => setOpenResetPasswordForm(false)}
          onFormCancel={() => setOpenResetPasswordForm(false)}
        />
      )}
      {openStageMigrationForm && (
        <StageTenantMigrationForm
          userId={userData.id}
          userName={fullName}
          currentPendingTenantId={userData.pendingMigrationTenantId}
          onFormComplete={() => setOpenStageMigrationForm(false)}
          onFormCancel={() => setOpenStageMigrationForm(false)}
        />
      )}
    </>
  )
}

const UserDetailsPageWithAuthorization = authorizePage(
  UserDetailsPage,
  'Permission',
  'Permissions.Users.View',
)

export default UserDetailsPageWithAuthorization
