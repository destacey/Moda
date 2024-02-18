'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetUserById } from '@/src/services/queries/user-management-queries'
import { notFound } from 'next/navigation'
import UserDetailsLoading from './loading'
import { useState } from 'react'
import UserDetails from './user-details'
import { Card } from 'antd'
import BasicBreadcrumb from '@/src/app/components/common/basic-breadcrumb'

const UserDetailsPage = ({ params }) => {
  const [activeTab, setActiveTab] = useState('details')
  const { data: userData, isLoading } = useGetUserById(params.id)

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <UserDetails user={userData} canEdit={true} />,
    },
  ]

  if (isLoading) {
    return <UserDetailsLoading />
  }

  if (!userData) {
    notFound()
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
      <PageTitle title={fullName} subtitle="User Details" />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
    </>
  )
}

const UserDetailsPageWithAuthorization = authorizePage(
  UserDetailsPage,
  'Permission',
  'Permissions.Users.View',
)

export default UserDetailsPageWithAuthorization
