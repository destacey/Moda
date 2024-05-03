'use client'

import { useAppDispatch, useDocumentTitle } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { useGetWorkItemQuery } from '@/src/store/features/work-management/workspace-api'
import { notFound, usePathname } from 'next/navigation'
import { useCallback, useEffect, useState } from 'react'
import WorkItemDetailsLoading from './loading'
import { PageTitle } from '@/src/app/components/common'
import { Card } from 'antd'
import { authorizePage } from '@/src/app/components/hoc'
import WorkItemDetails from './work-item-details'
import Link from 'next/link'
import { ExportOutlined } from '@ant-design/icons'
import ExternalIconLink from '@/src/app/components/common/external-icon-link'

enum WorkItemTabs {
  Details = 'details',
}

const WorkItemDetailsPage = ({ params }) => {
  const workspaceKey = params.key.toUpperCase()
  const workItemKey = params.workItemKey.toUpperCase()
  useDocumentTitle('Work Item Details')
  const [activeTab, setActiveTab] = useState(WorkItemTabs.Details)

  const {
    data: workItemData,
    isLoading,
    error,
    refetch,
  } = useGetWorkItemQuery({ idOrKey: workspaceKey, workItemKey: workItemKey })

  const dispatch = useAppDispatch()
  const pathname = usePathname()

  useEffect(() => {
    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Work',
      },
      {
        title: 'Workspaces',
        href: '/work/workspaces',
      },
      {
        title: workspaceKey,
        href: `/work/workspaces/${workspaceKey}`,
      },
      {
        title: workItemKey,
        href: null,
      },
    ]

    dispatch(setBreadcrumbRoute({ pathname, route: breadcrumbRoute }))
  }, [dispatch, pathname, workItemKey, workspaceKey])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  // doesn't trigger on first render
  const onTabChange = useCallback((tabKey) => {
    setActiveTab(tabKey)
  }, [])

  if (isLoading) {
    return <WorkItemDetailsLoading />
  }

  if (!isLoading && !workItemData) {
    notFound()
  }

  const tabs = [
    {
      key: WorkItemTabs.Details,
      tab: 'Details',
      content: <WorkItemDetails workItem={workItemData} />,
    },
  ]

  return (
    <>
      <PageTitle
        title={
          <ExternalIconLink
            content={workItemData?.title}
            url={workItemData.externalViewWorkItemUrl}
            tooltip="Open in external system"
          />
        }
        subtitle="Work Item Details"
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
    </>
  )
}

const WorkItemDetailsPageWithAuthorization = authorizePage(
  WorkItemDetailsPage,
  'Permission',
  'Permissions.WorkItems.View',
)

export default WorkItemDetailsPageWithAuthorization
