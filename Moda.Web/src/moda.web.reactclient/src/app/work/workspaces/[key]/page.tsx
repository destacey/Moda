'use client'

import { PageTitle } from '@/src/app/components/common'
import { authorizePage } from '@/src/app/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/app/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { useGetWorkspaceQuery } from '@/src/store/features/work-management/workspace-api'
import { Card } from 'antd'
import { notFound, usePathname } from 'next/navigation'
import { useCallback, useEffect, useState } from 'react'
import WorkspaceDetailsLoading from './loading'
import WorkspaceDetails from './workspace-details'

enum WorkspaceTabs {
  Details = 'details',
}

const WorkspaceDetailsPage = ({ params }) => {
  const workspaceKey = params.key.toUpperCase()
  useDocumentTitle('Workspace Details')
  const [activeTab, setActiveTab] = useState(WorkspaceTabs.Details)

  const {
    data: workspaceData,
    isLoading,
    error,
    refetch,
  } = useGetWorkspaceQuery(workspaceKey)

  const dispatch = useAppDispatch()
  const pathname = usePathname()

  useEffect(() => {
    workspaceData &&
      dispatch(setBreadcrumbTitle({ title: workspaceData.name, pathname }))
  }, [dispatch, pathname, workspaceData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  // doesn't trigger on first render
  const onTabChange = useCallback((tabKey) => {
    setActiveTab(tabKey)
  }, [])

  if (isLoading) {
    return <WorkspaceDetailsLoading />
  }

  if (!isLoading && !workspaceData) {
    notFound()
  }

  const tabs = [
    {
      key: WorkspaceTabs.Details,
      tab: 'Details',
      content: <WorkspaceDetails workspace={workspaceData} />,
    },
  ]

  return (
    <>
      <PageTitle title={workspaceData.name} subtitle="Workspace Details" />
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

const WorkspaceDetailsPageWithAuthorization = authorizePage(
  WorkspaceDetailsPage,
  'Permission',
  'Permissions.Workspaces.View',
)

export default WorkspaceDetailsPageWithAuthorization
