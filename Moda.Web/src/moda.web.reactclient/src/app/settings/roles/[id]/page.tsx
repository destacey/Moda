'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Card } from 'antd'
import { createElement, useEffect, useState } from 'react'
import RoleDetails from './components/role-details'
import Permissions from './components/permissions'
import useAuth from '@/src/app/components/contexts/auth'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import { useGetRoleById } from '@/src/services/queries/user-management-queries'

const RoleDetailsPage = ({ params }) => {
  const { data: roleData } = useGetRoleById(params.id)

  const [activeTab, setActiveTab] = useState('details')
  const { hasClaim } = useAuth()

  const canUpdateRole = hasClaim('Permission', 'Permissions.Roles.Update')
  const canViewPermissions = hasClaim(
    'Permission',
    'Permissions.Permissions.View',
  )

  const { setBreadcrumbTitle } = useBreadcrumbs()

  useEffect(() => {
    setBreadcrumbTitle(roleData?.name ?? 'New Role')
  }, [roleData, setBreadcrumbTitle])

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: createElement(RoleDetails, {
        role: roleData,
      }),
    },
    canViewPermissions && {
      key: 'permissions',
      tab: 'Permissions',
      content: createElement(Permissions, {
        roleId: roleData?.id,
        permissions: roleData?.permissions,
      }),
    },
  ]

  return (
    <>
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

export default RoleDetailsPage
