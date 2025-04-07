'use client'

import PageTitle from '@/src/components/common/page-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound } from 'next/navigation'
import UserDetailsLoading from './loading'
import { useEffect, useState } from 'react'
import { Card } from 'antd'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import { useGetUserQuery } from '@/src/store/features/user-management/users-api'
import { UserDetails } from '../_components'

const UserDetailsPage = ({ params }) => {
  const [activeTab, setActiveTab] = useState('details')

  const {
    data: userData,
    isLoading,
    error,
    refetch,
  } = useGetUserQuery(params.id)

  useEffect(() => {
    error && console.error(error)
  }, [error])

  if (isLoading) {
    return <UserDetailsLoading />
  }

  if (!userData) {
    notFound()
  }

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <UserDetails user={userData} />,
    },
  ]

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
