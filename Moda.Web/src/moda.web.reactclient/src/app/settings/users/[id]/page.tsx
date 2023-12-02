'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetUserById } from '@/src/services/queries/user-management-queries'
import { notFound, usePathname } from 'next/navigation'
import UserDetailsLoading from './loading'
import { useAppDispatch } from '@/src/app/hooks'
import { useEffect, useState } from 'react'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import UserDetails from './user-details'
import { Card } from 'antd'

const UserDetailsPage = ({ params }) => {
  const [activeTab, setActiveTab] = useState('details')
  const { data: userData, isLoading } = useGetUserById(params.id)
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <UserDetails user={userData} canEdit={true} />,
    },
  ]

  useEffect(() => {
    userData &&
      dispatch(setBreadcrumbTitle({ title: userData.userName, pathname }))
  }, [userData, dispatch, pathname])

  if (isLoading) {
    return <UserDetailsLoading />
  }

  if (!userData) {
    notFound()
  }

  const fullName = `${userData?.firstName} ${userData?.lastName}`

  return (
    <>
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
