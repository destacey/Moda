'use client'

import React, { useContext, useEffect, useState } from "react"
import { Card } from "antd"
import PageTitle from "../../components/common/page-title";
import ProfileForm from "./profile-form"
import ClaimsGrid from "./claims-grid"
import useAuth from "../../components/contexts/auth";

const tabs = [
  {key: 'profile', tab: 'Profile', content: React.createElement(ProfileForm)},
  {key: 'claims', tab: 'Claims', content: React.createElement(ClaimsGrid)},
]

const Page = () => {
  const {refreshUser} = useAuth()
  const [activeTab, setActiveTab] = useState('profile')

  useEffect(() => {
    const reloadUserPermissions = async () => {
      await refreshUser()
    }
    reloadUserPermissions()
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <>
      <PageTitle title="Account" subtitle="Manage your account" />
      <Card style={{ width:'100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={key => setActiveTab(key)}
      >
        {tabs.find(t => t.key === activeTab)?.content}
      </Card>
    </>
  )
}

export default Page;