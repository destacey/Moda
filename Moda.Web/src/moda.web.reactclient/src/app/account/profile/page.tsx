'use client'

import React, { useCallback, useEffect, useState } from 'react'
import { Card } from 'antd'
import { usePathname } from 'next/navigation'
import PageTitle from '../../../components/common/page-title'
import ProfileForm from './profile-form'
import ClaimsGrid from './claims-grid'
import useAuth from '../../../components/contexts/auth'
import { useDocumentTitle } from '../../../hooks/use-document-title'
import { useAppDispatch } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { useGetProfileQuery } from '@/src/store/features/user-management/profile-api'
import { useMessage } from '@/src/components/contexts/messaging'

enum AccountTabs {
  Profile = 'profile',
  Claims = 'claims',
}

const tabs = [
  {
    key: AccountTabs.Profile,
    tab: 'Profile',
  },
  { key: AccountTabs.Claims, tab: 'Claims' },
]

const AccountProfilePage = () => {
  useDocumentTitle('Account Profile')
  const [activeTab, setActiveTab] = useState(AccountTabs.Profile)

  const messageApi = useMessage()
  const { user, isLoading, refreshUser } = useAuth()

  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const {
    data: profileData,
    isLoading: profileLoading,
    error: profileError,
  } = useGetProfileQuery(null, { skip: !user })

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case AccountTabs.Profile:
        return React.createElement(ProfileForm, profileData)
      case AccountTabs.Claims:
        return <ClaimsGrid />
      default:
        return null
    }
  }, [activeTab, profileData])

  const onTabChange = useCallback((tabKey: string) => {
    setActiveTab(tabKey as AccountTabs)
  }, [])

  useEffect(() => {
    if (!isLoading && user) {
      dispatch(setBreadcrumbTitle({ title: user.name, pathname }))
    }
  }, [user, isLoading, pathname, dispatch])

  useEffect(() => {
    if (profileError) {
      console.error(profileError)
      messageApi.error('Failed to load user profile.')
    }
  }, [profileError, messageApi])

  return (
    <>
      <PageTitle title="Account" subtitle="Manage your account" />
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

export default AccountProfilePage
