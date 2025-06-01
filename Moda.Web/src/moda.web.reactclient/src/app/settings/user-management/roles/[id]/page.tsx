'use client'

import PageTitle from '@/src/components/common/page-title'
import { Card } from 'antd'
import { use, useCallback, useMemo, useState } from 'react'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { notFound } from 'next/navigation'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import { useGetRoleQuery } from '@/src/store/features/user-management/roles-api'
import { Permissions, RoleDetails } from '../_components'

enum RoleDetailsTabs {
  Details = 'details',
  Permissions = 'permissions',
}

const getRoleTabs = (canViewPermissions: boolean) => [
  {
    key: RoleDetailsTabs.Details,
    tab: 'Details',
  },
  canViewPermissions && {
    key: RoleDetailsTabs.Permissions,
    tab: 'Permissions',
  },
]

const RoleDetailsPage = (props: { params: Promise<{ id: string }> }) => {
  const { id } = use(props.params)

  const [activeTab, setActiveTab] = useState(RoleDetailsTabs.Details)

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

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case RoleDetailsTabs.Details:
        return <RoleDetails role={roleData} />
      case RoleDetailsTabs.Permissions:
        return (
          <Permissions
            roleId={roleData?.id}
            permissions={roleData?.permissions}
          />
        )
      default:
        return null
    }
  }, [activeTab, roleData])

  const onTabChange = useCallback((tabKey: string) => {
    setActiveTab(tabKey as RoleDetailsTabs)
  }, [])

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
      <PageTitle title={roleData?.name} subtitle="Role Details" />
      <Card tabList={tabs} activeTabKey={activeTab} onTabChange={onTabChange}>
        {renderTabContent()}
      </Card>
    </>
  )
}

const RoleDetailsPageWithAuthorization = authorizePage(
  RoleDetailsPage,
  'Permission',
  'Permissions.Roles.View',
)

export default RoleDetailsPageWithAuthorization
