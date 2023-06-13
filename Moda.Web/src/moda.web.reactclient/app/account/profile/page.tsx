'use client'

import React from "react"
import { Card } from "antd"
import PageTitle from "@/app/components/common/page-title"
import ProfileForm from "./profile-form"
import ClaimsGrid from "./claims-grid"

const tabs = [
  {key: 'profile', tab: 'Profile', content: React.createElement(ProfileForm)},
  {key: 'claims', tab: 'Claims', content: React.createElement(ClaimsGrid)}
]

const Page = () => {
  const [activeTab, setActiveTab] = React.useState('profile')

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