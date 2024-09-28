'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Card } from 'antd'
import { useState } from 'react'
import RoleDetails from './components/role-details'
import Permissions from './components/permissions'
import useAuth from '@/src/app/components/contexts/auth'
import { useGetRoleById } from '@/src/services/queries/user-management-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound } from 'next/navigation'
import BasicBreadcrumb from '@/src/app/components/common/basic-breadcrumb'

const RoleDetailsPage = ({ params }) => {
  const { data: roleData, isLoading, isFetching } = useGetRoleById(params.id)

  const [activeTab, setActiveTab] = useState('details')

  const { hasPermissionClaim } = useAuth()

  //const canUpdateRole = hasPermissionClaim('Permissions.Roles.Update')
  const canViewPermissions = hasPermissionClaim('Permissions.Permissions.View')

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

  if (!isLoading && !isFetching && !roleData) {
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
