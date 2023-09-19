'use client'

import React, { useEffect, useState } from 'react'
import { Card } from 'antd'
import { usePathname } from 'next/navigation'
import PageTitle from '../../components/common/page-title'
import ProfileForm from './profile-form'
import ClaimsGrid from './claims-grid'
import useAuth from '../../components/contexts/auth'
import { useDocumentTitle } from '../../hooks/use-document-title'
import { getProfileClient } from '@/src/services/clients'
import { UserDetailsDto } from '@/src/services/moda-api'
import { useAppDispatch } from '@/src/app/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'

const AccountProfilePage = () => {
  useDocumentTitle('Account Profile')
  const { user, isLoading, refreshUser } = useAuth()
  const [profile, setProfile] = useState<UserDetailsDto>()
  const [activeTab, setActiveTab] = useState('profile')
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const tabs = [
    {
      key: 'profile',
      tab: 'Profile',
      content: React.createElement(ProfileForm, profile),
    },
    { key: 'claims', tab: 'Claims', content: React.createElement(ClaimsGrid) },
  ]

  useEffect(() => {
    const loadProfile = async () => {
      const profileClient = await getProfileClient()
      setProfile(await profileClient.get())
    }
    loadProfile()

    const reloadUserPermissions = async () => {
      await refreshUser()
    }
    reloadUserPermissions()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  useEffect(() => {
    if (!isLoading && user) {
      dispatch(setBreadcrumbTitle({title: user.name, pathname}))
    }
  }, [user, isLoading, pathname, dispatch])

  return (
    <>
      <PageTitle title="Account" subtitle="Manage your account" />
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

export default AccountProfilePage
