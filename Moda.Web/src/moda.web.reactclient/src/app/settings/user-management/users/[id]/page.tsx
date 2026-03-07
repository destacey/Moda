'use client'

import { PageActions } from '@/src/components/common'
import PageTitle from '@/src/components/common/page-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound } from 'next/navigation'
import UserDetailsLoading from './loading'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import { Card, MenuProps, Modal } from 'antd'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useGetUserQuery,
  useUnlockUserMutation,
} from '@/src/store/features/user-management/users-api'
import {
  EditUserForm,
  ManageUserRolesForm,
  ResetPasswordForm,
  UserDetails,
} from '../_components'
import { ItemType } from 'antd/es/menu/interface'

enum UserDetailsTabs {
  Details = 'details',
}

const tabs = [
  {
    key: UserDetailsTabs.Details,
    tab: 'Details',
  },
]

const UserDetailsPage = (props: { params: Promise<{ id: string }> }) => {
  const { id } = use(props.params)

  const [activeTab, setActiveTab] = useState(UserDetailsTabs.Details)
  const [openEditUserForm, setOpenEditUserForm] = useState(false)
  const [openManageUserRolesForm, setOpenManageUserRolesForm] = useState(false)
  const [openResetPasswordForm, setOpenResetPasswordForm] = useState(false)

  const { hasPermissionClaim } = useAuth()
  const canUpdateUser = hasPermissionClaim('Permissions.Users.Update')
  const canUpdateUserRoles = hasPermissionClaim('Permissions.UserRoles.Update')

  const messageApi = useMessage()
  const { data: userData, isLoading, error } = useGetUserQuery(id)
  const [unlockUser] = useUnlockUserMutation()

  const isLocalUser = userData?.loginProvider === 'Moda'
  const isLockedOut =
    !!userData?.lockoutEnd && new Date(userData.lockoutEnd) > new Date()

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canUpdateUser) {
      items.push({
        key: 'edit',
        label: 'Edit',
        onClick: () => setOpenEditUserForm(true),
      })
      if (isLocalUser) {
        items.push({
          key: 'reset-password',
          label: 'Reset Password',
          onClick: () => setOpenResetPasswordForm(true),
        })
      }
      if (isLockedOut) {
        items.push({
          key: 'unlock',
          label: 'Unlock Account',
          onClick: () => {
            Modal.confirm({
              title: 'Unlock Account',
              content: `Are you sure you want to unlock ${userData?.firstName} ${userData?.lastName}?`,
              okText: 'Unlock',
              onOk: async () => {
                try {
                  const result = await unlockUser(id)
                  if ('error' in result) {
                    throw result.error
                  }
                  messageApi.success('Account unlocked successfully.')
                } catch (error: any) {
                  messageApi.error(
                    error?.data?.detail ?? 'Failed to unlock the account.',
                  )
                }
              },
            })
          },
        })
      }
    }
    if (canUpdateUserRoles) {
      if (items.length > 0) {
        items.push({ key: 'divider', type: 'divider' })
      }
      items.push({
        key: 'manage-roles',
        label: 'Manage Roles',
        onClick: () => setOpenManageUserRolesForm(true),
      })
    }
    return items
  }, [canUpdateUser, canUpdateUserRoles, isLocalUser, isLockedOut, id, userData, messageApi, unlockUser])

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case UserDetailsTabs.Details:
        return <UserDetails user={userData} />
      default:
        return null
    }
  }, [activeTab, userData])

  const onTabChange = useCallback((tabKey: string) => {
    setActiveTab(tabKey as UserDetailsTabs)
  }, [])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  if (isLoading) {
    return <UserDetailsLoading />
  }

  if (!userData) {
    return notFound()
  }

  const fullName = `${userData?.firstName} ${userData?.lastName}`

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
    </>
  )
}

const UserDetailsPageWithAuthorization = authorizePage(
  UserDetailsPage,
  'Permission',
  'Permissions.Users.View',
)

export default UserDetailsPageWithAuthorization
