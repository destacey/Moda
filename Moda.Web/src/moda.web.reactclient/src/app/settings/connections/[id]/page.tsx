'use client'

import PageTitle from '@/src/components/common/page-title'
import AzdoBoardsConnectionDetails from './azdo-boards-connection-details'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Card, message } from 'antd'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname, useRouter } from 'next/navigation'
import EditConnectionForm from '../components/edit-connection-form'
import AzdoBoardsOrganization from './azdo-boards-organization'
import { ExportOutlined } from '@ant-design/icons'
import Link from 'next/link'
import { useAppDispatch } from '@/src/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { AzdoBoardsConnectionContext } from './azdo-boards-connection-context'
import DeleteAzdoBoardsConnectionForm from '../components/delete-azdo-boards-connection-form'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import { PageActions } from '@/src/components/common'
import { ItemType } from 'antd/es/menu/interface'
import {
  useGetAzdoConnectionQuery,
  useSyncAzdoConnectionOrganizationMutation,
  useUpdateAzdoConnectionSyncStateMutation,
} from '@/src/store/features/app-integration/azdo-integration-api'

enum ConnectionTabs {
  Details = 'details',
  OrganizationConfiguration = 'organization-configuration',
}

const ConnectionDetailsPage = ({ params }) => {
  useDocumentTitle('Connection Details')
  const [activeTab, setActiveTab] = useState(ConnectionTabs.Details)
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const [isSyncingOrganization, setIsSyncingOrganization] = useState(false)
  const [openEditConnectionForm, setOpenEditConnectionForm] =
    useState<boolean>(false)
  const [openDeleteConnectionForm, setOpenDeleteConnectionForm] =
    useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const router = useRouter()
  const { hasClaim } = useAuth()
  const canUpdateConnections = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )
  const canDeleteConnections = hasClaim(
    'Permission',
    'Permissions.Connections.Delete',
  )

  const {
    data: connectionData,
    isLoading,
    refetch,
  } = useGetAzdoConnectionQuery(params.id)
  const azdoOrgUrl = connectionData?.configuration?.organizationUrl

  const [
    updateAzdoConnectionSyncState,
    { error: updateAzdoConnectionSyncStateError },
  ] = useUpdateAzdoConnectionSyncStateMutation()

  const [
    syncAzdoConnectionOrganization,
    { error: syncAzdoConnectionOrganizationError },
  ] = useSyncAzdoConnectionOrganizationMutation()

  const tabs = [
    {
      key: ConnectionTabs.Details,
      tab: 'Details',
      content: <AzdoBoardsConnectionDetails connection={connectionData} />,
    },
    {
      key: ConnectionTabs.OrganizationConfiguration,
      tab: 'Organization Configuration',
      content: (
        <AzdoBoardsOrganization
          workProcesses={connectionData?.configuration?.workProcesses}
          workspaces={connectionData?.configuration?.workspaces}
        />
      ),
    },
  ]

  useEffect(() => {
    if (!connectionData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Settings',
      },
      {
        href: `/settings/connections`,
        title: 'Connections',
      },
      {
        title: connectionData.name,
      },
    ]
    // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [connectionData, dispatch, pathname])

  const onEditConnectionFormClosed = (wasSaved: boolean) => {
    setOpenEditConnectionForm(false)
    if (wasSaved) {
      refetch()
    }
  }

  const onDeleteConnectionFormClosed = (wasSaved: boolean) => {
    setOpenEditConnectionForm(false)
    if (wasSaved) {
      // redirect to the connections list page
      router.push('/settings/connections')
    }
  }

  const updateSyncState = useCallback(async () => {
    try {
      const response = await updateAzdoConnectionSyncState({
        connectionId: params.id,
        isSyncEnabled: !connectionData?.isSyncEnabled,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        `Sync setting has been ${
          connectionData?.isSyncEnabled ? 'disabled' : 'enabled'
        }`,
      )
    } catch (error) {
      console.error(error)
      messageApi.error(`Failed to change sync setting. Error: ${error.detail}`)
    }
  }, [
    connectionData?.isSyncEnabled,
    messageApi,
    params.id,
    updateAzdoConnectionSyncState,
  ])

  const syncOrganizationConfiguration = useCallback(async () => {
    try {
      const response = await syncAzdoConnectionOrganization(params.id)
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        'Successfully imported organization processes and projects.',
      )
    } catch (error) {
      console.error(error)
      messageApi.error(
        `Failed to initialize organization. Error: ${error.detail}`,
      )
    }
    setIsSyncingOrganization(false)
  }, [syncAzdoConnectionOrganization, messageApi, params.id])

  const actionsMenuItems = useMemo(() => {
    const items = [] as ItemType[]
    if (canUpdateConnections) {
      items.push({
        key: 'edit',
        label: 'Edit',
        onClick: () => setOpenEditConnectionForm(true),
      })
    }
    if (canDeleteConnections) {
      items.push({
        key: 'delete',
        label: 'Delete',
        onClick: () => setOpenDeleteConnectionForm(true),
      })
    }
    if (canUpdateConnections) {
      items.push({
        key: 'divider',
        type: 'divider',
      })
    }
    if (canUpdateConnections) {
      items.push(
        {
          key: 'toggle-sync-setting',
          label: connectionData?.isSyncEnabled ? 'Disable Sync' : 'Enable Sync',
          disabled: connectionData && !connectionData.isValidConfiguration,
          onClick: () => updateSyncState(),
        },
        {
          key: 'sync-organization',
          label: 'Sync Organization Configuration',
          disabled: connectionData && !connectionData.isValidConfiguration,
          onClick: () => {
            setIsSyncingOrganization(true)
            syncOrganizationConfiguration()
          },
        },
      )
    }
    return items
  }, [
    canDeleteConnections,
    canUpdateConnections,
    connectionData,
    syncOrganizationConfiguration,
    updateSyncState,
  ])

  if (!isLoading && !connectionData) {
    notFound()
  }

  return (
    <>
      {contextHolder}
      <BasicBreadcrumb
        items={[
          { title: 'Settings' },
          { title: 'Connections', href: '/settings/connections' },
          { title: 'Details' },
        ]}
      />
      <PageTitle
        title={
          <>
            {connectionData?.name}{' '}
            {azdoOrgUrl && (
              <Link
                href={azdoOrgUrl}
                target="_blank"
                title="Open in Azure DevOps"
              >
                <ExportOutlined style={{ width: '12px' }} />
              </Link>
            )}
          </>
        }
        subtitle="Connection Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <AzdoBoardsConnectionContext.Provider
        value={{
          connectionId: params.id,
          organizationUrl: azdoOrgUrl,
          reloadConnectionData: refetch,
        }}
      >
        <Card
          tabList={tabs}
          activeTabKey={activeTab}
          onTabChange={(key: ConnectionTabs) => setActiveTab(key)}
        >
          {tabs.find((t) => t.key === activeTab)?.content}
        </Card>
      </AzdoBoardsConnectionContext.Provider>
      {openEditConnectionForm && (
        <EditConnectionForm
          showForm={openEditConnectionForm}
          id={connectionData?.id}
          onFormUpdate={() => onEditConnectionFormClosed(true)}
          onFormCancel={() => onEditConnectionFormClosed(false)}
        />
      )}
      {openDeleteConnectionForm && (
        <DeleteAzdoBoardsConnectionForm
          showForm={openDeleteConnectionForm}
          connection={connectionData}
          onFormSave={() => onDeleteConnectionFormClosed(true)}
          onFormCancel={() => onDeleteConnectionFormClosed(false)}
        />
      )}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  ConnectionDetailsPage,
  'Permission',
  'Permissions.Connections.View',
)

export default PageWithAuthorization
