'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound } from 'next/navigation'
import UserDetailsLoading from './loading'
import { useEffect, useState } from 'react'
import UserDetails from './user-details'
import { Card, message } from 'antd'
import BasicBreadcrumb from '@/src/app/components/common/basic-breadcrumb'
import { useGetUserQuery } from '@/src/store/features/user-management/users-api'

const UserDetailsPage = ({ params }) => {
  const [activeTab, setActiveTab] = useState('details')
  const [messageApi, contextHolder] = message.useMessage()

  const {
    data: userData,
    isLoading,
    error,
    refetch,
  } = useGetUserQuery(params.id)

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <UserDetails user={userData} canEdit={true} />,
    },
  ]

  useEffect(() => {
    error && console.error(error)
  }, [error])

  if (isLoading) {
    return <UserDetailsLoading />
  }

  if (!userData) {
    notFound()
  }

  const fullName = `${userData?.firstName} ${userData?.lastName}`

  return (
    <>
      {contextHolder}
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
