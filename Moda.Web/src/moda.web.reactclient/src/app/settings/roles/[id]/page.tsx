'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Card, Result } from 'antd'
import { useEffect, useState } from 'react'
import RoleDetails from './components/role-details'
import Permissions from './components/permissions'
import useAuth from '@/src/app/components/contexts/auth'
import { useGetRoleById } from '@/src/services/queries/user-management-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { useAppDispatch } from '@/src/app/hooks'

const RoleDetailsPage = ({ params }) => {
  const { data: roleData, isLoading, isFetching } = useGetRoleById(params.id)

  const [activeTab, setActiveTab] = useState('details')
  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const { hasClaim } = useAuth()

  const canUpdateRole = hasClaim('Permission', 'Permissions.Roles.Update')
  const canViewPermissions = hasClaim(
    'Permission',
    'Permissions.Permissions.View',
  )


  useEffect(() => {
    roleData && dispatch(setBreadcrumbTitle({title: roleData?.name, pathname}))
  }, [roleData, dispatch, pathname])

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
