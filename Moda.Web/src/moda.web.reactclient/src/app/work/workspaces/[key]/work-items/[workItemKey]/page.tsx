'use client'

import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import {
  useGetChildWorkItemsQuery,
  useGetWorkItemQuery,
} from '@/src/store/features/work-management/workspace-api'
import { notFound, usePathname } from 'next/navigation'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import WorkItemDetailsLoading from './loading'
import { PageActions, PageTitle } from '@/src/components/common'
import { Card, MenuProps } from 'antd'
import { authorizePage } from '@/src/components/hoc'
import WorkItemDetails from './work-item-details'
import ExternalIconLink from '@/src/components/common/external-icon-link'
import { WorkItemsGrid } from '@/src/components/common/work'
import WorkItemDashboard from './work-item-dashboard'
import WorkItemDependencies from './work-item-dependencies'
import useAuth from '@/src/components/contexts/auth'
import { ItemType } from 'antd/es/menu/interface'
import EditWorkItemProjectForm from '../../../_components/edit-workitem-project-form'

enum WorkItemTabs {
  Details = 'details',
  WorkItems = 'workItems',
  Dashboard = 'dashboard',
  Dependencies = 'dependencies',
}

const WorkItemDetailsPage = (props: {
  params: Promise<{ key: string; workItemKey: string }>
}) => {
  const { key, workItemKey } = use(props.params)

  const upperWorkspaceKey = key.toUpperCase()
  const upperWorkItemKey = workItemKey.toUpperCase()

  useDocumentTitle('Work Item Details')

  const [activeTab, setActiveTab] = useState(WorkItemTabs.Details)
  const [openEditWorkItemProjectForm, setOpenEditWorkItemProjectForm] =
    useState<boolean>(false)

  const { hasPermissionClaim } = useAuth()
  const canManageProjectWorkItems = hasPermissionClaim(
    'Permissions.Projects.ManageProjectWorkItems',
  )

  const {
    data: workItemData,
    error,
    isLoading,
  } = useGetWorkItemQuery({
    idOrKey: upperWorkspaceKey,
    workItemKey: upperWorkItemKey,
  })

  const childWorkItemsQuery = useGetChildWorkItemsQuery(
    {
      idOrKey: upperWorkspaceKey,
      workItemKey: upperWorkItemKey,
    },
    { skip: !workItemData },
  )

  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []

    if (canManageProjectWorkItems && workItemData?.tier === 'Portfolio') {
      items.push({
        key: 'edit-project',
        label: 'Edit Project',
        onClick: () => setOpenEditWorkItemProjectForm(true),
      })
    }

    return items
  }, [canManageProjectWorkItems, workItemData?.tier])

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
        title: upperWorkspaceKey,
        href: `/work/workspaces/${upperWorkspaceKey}`,
      },
      {
        title: upperWorkItemKey,
        href: null,
      },
    ]

    dispatch(setBreadcrumbRoute({ pathname, route: breadcrumbRoute }))
  }, [dispatch, pathname, upperWorkItemKey, upperWorkspaceKey])

  useEffect(() => {
    // TODO: this isn't getting called on hook error
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
    ...(workItemData?.statusCategory.name !== 'Removed'
      ? [
          {
            key: WorkItemTabs.Dashboard,
            tab: 'Dashboard',
            content: <WorkItemDashboard workItem={workItemData} />,
          },
        ]
      : []),
    ...(workItemData?.tier === 'Portfolio'
      ? [
          {
            key: WorkItemTabs.WorkItems,
            tab: 'Work Items',
            content: (
              <WorkItemsGrid
                workItems={childWorkItemsQuery.data}
                isLoading={childWorkItemsQuery.isLoading}
                refetch={childWorkItemsQuery.refetch}
              />
            ),
          },
        ]
      : []),
    {
      key: WorkItemTabs.Dependencies,
      tab: 'Dependencies',
      content: <WorkItemDependencies workItem={workItemData} />,
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
        subtitle={`${workItemData?.type ?? 'Work Item'} Details`}
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {openEditWorkItemProjectForm && (
        <EditWorkItemProjectForm
          workItemId={workItemData.id}
          workItemKey={workItemData.key}
          workspaceId={workItemData.workspace.id}
          onFormCancel={() => setOpenEditWorkItemProjectForm(false)}
          onFormComplete={() => {
            setOpenEditWorkItemProjectForm(false)
            childWorkItemsQuery.refetch()
          }}
        />
      )}
    </>
  )
}

const WorkItemDetailsPageWithAuthorization = authorizePage(
  WorkItemDetailsPage,
  'Permission',
  'Permissions.WorkItems.View',
)

export default WorkItemDetailsPageWithAuthorization
