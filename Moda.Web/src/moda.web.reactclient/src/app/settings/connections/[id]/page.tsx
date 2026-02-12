'use client'

import PageTitle from '@/src/components/common/page-title'
import AzdoConnectionDetails from './azdo-connection-details'
import { use, useCallback, useEffect, useMemo, useState } from 'react'
import { Card } from 'antd'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname, useRouter } from 'next/navigation'
import EditConnectionForm from '../_components/edit-connection-form'
import AzdoOrganization from './azdo-organization'
import { ExportOutlined } from '@ant-design/icons'
import Link from 'next/link'
import { useAppDispatch } from '@/src/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { AzdoConnectionContext } from './azdo-connection-context'
import DeleteAzdoConnectionForm from '../_components/delete-azdo-connection-form'
import BasicBreadcrumb from '@/src/components/common/basic-breadcrumb'
import { PageActions } from '@/src/components/common'
import { ItemType } from 'antd/es/menu/interface'
import {
  useSyncAzdoConnectionOrganizationMutation,
  useUpdateAzdoConnectionSyncStateMutation,
} from '@/src/store/features/app-integration/azdo-integration-api'
import { useGetConnectionQuery } from '@/src/store/features/app-integration/connections-api'
import { useMessage } from '@/src/components/contexts/messaging'
import { AzureDevOpsConnectionDetailsDto } from '@/src/services/moda-api'

enum ConnectionTabs {
  Details = 'details',
  OrganizationConfiguration = 'organization-configuration',
}

const tabs = [
  {
    key: ConnectionTabs.Details,
    tab: 'Details',
  },
  {
    key: ConnectionTabs.OrganizationConfiguration,
    tab: 'Organization Configuration',
  },
]

const ConnectionDetailsPage = (props: { params: Promise<{ id: string }> }) => {
  const { id } = use(props.params)

  useDocumentTitle('Connection Details')

  const [activeTab, setActiveTab] = useState(ConnectionTabs.Details)
  const [isSyncingOrganization, setIsSyncingOrganization] = useState(false)
  const [openEditConnectionForm, setOpenEditConnectionForm] =
    useState<boolean>(false)
  const [openDeleteConnectionForm, setOpenDeleteConnectionForm] =
    useState<boolean>(false)

  const messageApi = useMessage()
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
  } = useGetConnectionQuery(id)

  // Type narrow to AzureDevOpsConnectionDetailsDto
  // Note: Using connector.name instead of $type because System.Text.Json doesn't add $type for root-level objects
  const azdoConnection =
    connectionData?.connector?.name === 'Azure DevOps'
      ? (connectionData as AzureDevOpsConnectionDetailsDto)
      : null
  const azdoOrgUrl = azdoConnection?.configuration?.organizationUrl

  const [
    updateAzdoConnectionSyncState,
    { error: updateAzdoConnectionSyncStateError },
  ] = useUpdateAzdoConnectionSyncStateMutation()

  const [
    syncAzdoConnectionOrganization,
    { error: syncAzdoConnectionOrganizationError },
  ] = useSyncAzdoConnectionOrganizationMutation()

  const renderTabContent = useCallback(() => {
    switch (activeTab) {
      case ConnectionTabs.Details:
        return <AzdoConnectionDetails connection={azdoConnection} />
      case ConnectionTabs.OrganizationConfiguration:
        return (
          <AzdoOrganization
            workProcesses={azdoConnection?.configuration?.workProcesses}
            workspaces={azdoConnection?.configuration?.workspaces}
          />
        )
      default:
        return null
    }
  }, [activeTab, azdoConnection])

  const onTabChange = useCallback((tabKey: string) => {
    setActiveTab(tabKey as ConnectionTabs)
  }, [])

  useEffect(() => {
    if (!azdoConnection) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Settings',
      },
      {
        href: `/settings/connections`,
        title: 'Connections',
      },
      {
        title: azdoConnection.name,
      },
    ]
    // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [azdoConnection, dispatch, pathname])

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
        connectionId: id,
        isSyncEnabled: !azdoConnection?.isSyncEnabled,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        `Sync setting has been ${
          azdoConnection?.isSyncEnabled ? 'disabled' : 'enabled'
        }`,
      )
    } catch (error) {
      console.error(error)
      messageApi.error(`Failed to change sync setting. Error: ${error.detail}`)
    }
  }, [
    azdoConnection?.isSyncEnabled,
    messageApi,
    id,
    updateAzdoConnectionSyncState,
  ])

  const syncOrganizationConfiguration = useCallback(async () => {
    try {
      const response = await syncAzdoConnectionOrganization(id)
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
  }, [syncAzdoConnectionOrganization, messageApi, id])

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
          label: azdoConnection?.isSyncEnabled ? 'Disable Sync' : 'Enable Sync',
          disabled: azdoConnection && !azdoConnection.isValidConfiguration,
          onClick: () => updateSyncState(),
        },
        {
          key: 'sync-organization',
          label: 'Sync Organization Configuration',
          disabled: azdoConnection && !azdoConnection.isValidConfiguration,
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
    azdoConnection,
    syncOrganizationConfiguration,
    updateSyncState,
  ])

  if (!isLoading && !azdoConnection) {
    return notFound()
  }

  return (
    <>
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
            {azdoConnection?.name}{' '}
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
      <AzdoConnectionContext.Provider
        value={{
          connectionId: id,
          organizationUrl: azdoOrgUrl,
          reloadConnectionData: refetch,
        }}
      >
        <Card tabList={tabs} activeTabKey={activeTab} onTabChange={onTabChange}>
          {renderTabContent()}
        </Card>
      </AzdoConnectionContext.Provider>
      {openEditConnectionForm && (
        <EditConnectionForm
          showForm={openEditConnectionForm}
          id={azdoConnection?.id}
          onFormUpdate={() => onEditConnectionFormClosed(true)}
          onFormCancel={() => onEditConnectionFormClosed(false)}
        />
      )}
      {openDeleteConnectionForm && (
        <DeleteAzdoConnectionForm
          showForm={openDeleteConnectionForm}
          connection={azdoConnection}
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
