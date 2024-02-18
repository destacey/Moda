'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound } from 'next/navigation'
import { useState } from 'react'
import { Card } from 'antd'
import BasicBreadcrumb from '@/src/app/components/common/basic-breadcrumb'
import WorkProcessDetailsLoading from './loading'
import { useGetWorkProcessesByIdOrKey } from '@/src/services/queries/work-management-queries'
import WorkProcessDetails from './work-process-details'

const WorkProcessDetailsPage = ({ params }) => {
  const [activeTab, setActiveTab] = useState('details')

  const { data: workProcessData, isLoading } = useGetWorkProcessesByIdOrKey(
    params.key,
  )

  if (isLoading) {
    return <WorkProcessDetailsLoading />
  }

  if (!workProcessData) {
    notFound()
  }

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <WorkProcessDetails workProcess={workProcessData} />,
    },
  ]

  return (
    <>
      <BasicBreadcrumb
        items={[
          { title: 'Settings' },
          { title: 'Work Management' },
          { title: 'Work Processes', href: './' },
          { title: 'Details' },
        ]}
      />
      <PageTitle
        title={`${workProcessData?.key} - ${workProcessData?.name}`}
        subtitle="Work Process Details"
      />
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

const WorkProcessDetailsPageWithAuthorization = authorizePage(
  WorkProcessDetailsPage,
  'Permission',
  'Permissions.WorkProcesses.View',
)

export default WorkProcessDetailsPageWithAuthorization
