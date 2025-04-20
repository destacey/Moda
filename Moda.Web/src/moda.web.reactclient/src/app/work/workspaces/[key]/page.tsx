'use client'

import { PageTitle } from '@/src/components/common'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import {
  useGetWorkItemsQuery,
  useGetWorkspaceQuery,
} from '@/src/store/features/work-management/workspace-api'
import { Button, Card } from 'antd'
import { notFound, usePathname } from 'next/navigation'
import { use, useCallback, useEffect, useState } from 'react'
import WorkspaceDetailsLoading from './loading'
import WorkspaceDetails from './workspace-details'
import useAuth from '@/src/components/contexts/auth'
import SetWorkspaceExternalUrlTemplatesForm from './set-workspace-external-url-templates-form'
import { WorkItemsGrid } from '@/src/components/common/work'

enum WorkspaceTabs {
  Details = 'details',
  WorkItems = 'workItems',
}

const WorkspaceDetailsPage = (props: { params: Promise<{ key: string }> }) => {
  const { key } = use(props.params)

  const workspaceKey = key.toUpperCase()

  useDocumentTitle('Workspace Details')

  const [activeTab, setActiveTab] = useState(WorkspaceTabs.Details)
  const [
    openSetWorkspaceExternalUrlTemplatesForm,
    setOpenSetWorkspaceExternalUrlTemplatesForm,
  ] = useState<boolean>(false)

  const { hasPermissionClaim } = useAuth()
  const canUpdateWorkspace = hasPermissionClaim('Permissions.Workspaces.Update')

  const {
    data: workspaceData,
    isLoading,
    error,
    refetch,
  } = useGetWorkspaceQuery(workspaceKey)

  const workItemsQuery = useGetWorkItemsQuery(workspaceKey)

  const dispatch = useAppDispatch()
  const pathname = usePathname()

  useEffect(() => {
    dispatch(setBreadcrumbTitle({ title: workspaceKey, pathname }))
  }, [dispatch, pathname, workspaceKey])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  useEffect(() => {
    workItemsQuery.error && console.error(workItemsQuery.error)
  }, [workItemsQuery.error])

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

  const actions = () => {
    if (!workspaceData) return null

    return (
      <>
        {canUpdateWorkspace && (
          <Button
            onClick={() => setOpenSetWorkspaceExternalUrlTemplatesForm(true)}
          >
            Set External URLs
          </Button>
        )}
      </>
    )
  }

  const tabs = [
    {
      key: WorkspaceTabs.Details,
      tab: 'Details',
      content: <WorkspaceDetails workspace={workspaceData} />,
    },
    {
      key: WorkspaceTabs.WorkItems,
      tab: 'Work Items',
      content: (
        <WorkItemsGrid
          workItems={workItemsQuery.data}
          isLoading={workItemsQuery.isLoading}
          refetch={workItemsQuery.refetch}
        />
      ),
    },
  ]

  const onSetWorkspaceExternalUrlTemplatesFormClosed = (wasSaved: boolean) => {
    setOpenSetWorkspaceExternalUrlTemplatesForm(false)
    if (wasSaved) {
      refetch()
    }
  }

  return (
    <>
      <PageTitle
        title={workspaceData.name}
        subtitle="Workspace Details"
        actions={actions()}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {openSetWorkspaceExternalUrlTemplatesForm && (
        <SetWorkspaceExternalUrlTemplatesForm
          showForm={openSetWorkspaceExternalUrlTemplatesForm}
          workspaceId={workspaceData.id}
          onFormUpdate={() =>
            onSetWorkspaceExternalUrlTemplatesFormClosed(true)
          }
          onFormCancel={() =>
            onSetWorkspaceExternalUrlTemplatesFormClosed(false)
          }
        />
      )}
    </>
  )
}

const WorkspaceDetailsPageWithAuthorization = authorizePage(
  WorkspaceDetailsPage,
  'Permission',
  'Permissions.Workspaces.View',
)

export default WorkspaceDetailsPageWithAuthorization
