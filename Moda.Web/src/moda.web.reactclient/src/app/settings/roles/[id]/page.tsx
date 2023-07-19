'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useGetRoleById } from '@/src/services/query'
import { Button, Card, Space, message, Layout } from 'antd'
import { createElement, useEffect, useState } from 'react'
import Detail from './components/detail'
import Permissions from './components/permissions'
import useAuth from '@/src/app/components/contexts/auth'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'

const Page = ({ params }) => {
  const roleData = useGetRoleById(params.id)

  const [permissions, setPermissions] = useState<string[]>(
    roleData?.data?.permissions || []
  )
  const [activeTab, setActiveTab] = useState('details')
  const { hasClaim } = useAuth()

  const canUpdateRole = hasClaim('Permission', 'Permissions.Roles.Update')
  const canViewPermissions = hasClaim(
    'Permission',
    'Permissions.Permissions.View'
  )

  const { setBreadcrumbTitle } = useBreadcrumbs()

  useEffect(
    () => setBreadcrumbTitle(roleData.data?.name ?? 'New Role'),
    [roleData.data]
  )

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: createElement(Detail, {
        role: roleData.data
      }),
    },
    canViewPermissions && {
      key: 'permissions',
      tab: 'Permissions',
      content: createElement(Permissions, {
        roleId: roleData.data?.id,
        permissions: roleData.data?.permissions
      }),
    },
  ]

  return (
    <>
      <PageTitle
        title={roleData.data?.name}
        subtitle="Role Details"
      />
      <Card
        style={{ width: '100%', height: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
        
      </Card>
    </>
  )
}

export default Page
