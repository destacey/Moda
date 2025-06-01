'use client'

import PageTitle from '@/src/components/common/page-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound } from 'next/navigation'
import UserDetailsLoading from './loading'
import { use, useCallback, useEffect, useState } from 'react'
import { Card } from 'antd'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import { useGetUserQuery } from '@/src/store/features/user-management/users-api'
import { UserDetails } from '../_components'

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

  const { data: userData, isLoading, error, refetch } = useGetUserQuery(id)

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
      <PageTitle title={fullName} subtitle="User Details" />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {renderTabContent()}
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
