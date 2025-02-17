'use client'

import PageTitle from '@/src/components/common/page-title'
import { Card } from 'antd'
import { useState } from 'react'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { notFound } from 'next/navigation'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import { useGetRoleQuery } from '@/src/store/features/user-management/roles-api'
import { Permissions, RoleDetails } from '../_components'

const RoleDetailsPage = ({ params }) => {
  const [activeTab, setActiveTab] = useState('details')

  const { hasPermissionClaim } = useAuth()

  //const canUpdateRole = hasPermissionClaim('Permissions.Roles.Update')
  const canViewPermissions = hasPermissionClaim('Permissions.Permissions.View')

  const {
    data: roleData,
    isLoading: isLoading,
    error: error,
    refetch: refetch,
  } = useGetRoleQuery(params.id)

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <RoleDetails role={roleData} />,
    },
    canViewPermissions && {
      key: 'permissions',
      tab: 'Permissions',
      content: (
        <Permissions
          roleId={roleData?.id}
          permissions={roleData?.permissions}
        />
      ),
    },
  ]

  if (!isLoading && !roleData) {
    notFound()
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
      <Card
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
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
