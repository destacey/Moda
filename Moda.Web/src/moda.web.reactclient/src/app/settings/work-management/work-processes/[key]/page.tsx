'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound } from 'next/navigation'
import { useEffect, useMemo, useState } from 'react'
import { Card } from 'antd'
import BasicBreadcrumb from '@/src/app/components/common/basic-breadcrumb'
import WorkProcessDetailsLoading from './loading'
import WorkProcessDetails from './work-process-details'
import { PageActions } from '@/src/app/components/common'
import useAuth from '@/src/app/components/contexts/auth'
import ChangeWorkProcessIsActiveForm from '../components/change-work-process-isactive-form'
import { useGetWorkProcessQuery } from '@/src/store/features/work-management/work-process-api'
import { ItemType } from 'antd/es/menu/interface'

const WorkProcessDetailsPage = ({ params }) => {
  const [activeTab, setActiveTab] = useState('details')
  const [
    openChangeWorkProcessIsActiveForm,
    setOpenChangeWorkProcessIsActiveForm,
  ] = useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canUpdateWorkProcess = hasClaim(
    'Permission',
    'Permissions.WorkProcesses.Update',
  )

  const {
    data: workProcessData,
    isLoading,
    error,
    refetch,
  } = useGetWorkProcessQuery(params.key)

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const actionsMenuItems = useMemo(() => {
    if (!workProcessData?.isActive === undefined) return [] as ItemType[]

    const items = [] as ItemType[]
    if (canUpdateWorkProcess) {
      const activationManagementLabel = workProcessData?.isActive
        ? 'Deactivate'
        : 'Activate'
      items.push({
        key: 'activate-menu-item',
        label: activationManagementLabel,
        onClick: () => setOpenChangeWorkProcessIsActiveForm(true),
      })
    }
    return items
  }, [canUpdateWorkProcess, workProcessData?.isActive])

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

  const onChangeWorkProcessIsActiveFormClosed = (wasSaved: boolean) => {
    if (wasSaved) {
      refetch()
    }
    setOpenChangeWorkProcessIsActiveForm(false)
  }

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
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {openChangeWorkProcessIsActiveForm && (
        <ChangeWorkProcessIsActiveForm
          showForm={openChangeWorkProcessIsActiveForm}
          workProcessId={workProcessData?.id}
          workProcessName={workProcessData?.name}
          isActive={!!workProcessData?.isActive}
          onFormSave={() => onChangeWorkProcessIsActiveFormClosed(true)}
          onFormCancel={() => onChangeWorkProcessIsActiveFormClosed(false)}
        />
      )}
    </>
  )
}

const WorkProcessDetailsPageWithAuthorization = authorizePage(
  WorkProcessDetailsPage,
  'Permission',
  'Permissions.WorkProcesses.View',
)

export default WorkProcessDetailsPageWithAuthorization
