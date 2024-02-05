'use client'

import PageTitle from '@/src/app/components/common/page-title'
import AzdoBoardsConnectionDetails from './azdo-boards-connection-details'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Button, Card, Dropdown, MenuProps, Space, message } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import {
  useGetAzdoBoardsConnectionById,
  useSyncAzdoBoardsConnectionOrganizationMutation,
} from '@/src/services/queries/app-integration-queries'
import { notFound, usePathname, useRouter } from 'next/navigation'
import EditConnectionForm from '../components/edit-connection-form'
import AzdoBoardsOrganization from './azdo-boards-organization'
import { DownOutlined, ExportOutlined } from '@ant-design/icons'
import Link from 'next/link'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { AzdoBoardsConnectionContext } from './azdo-boards-connection-context'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import DeleteAzdoBoardsConnectionForm from '../components/delete-azdo-boards-connection-form'

enum ConnectionTabs {
  Details = 'details',
  OrganizationConfiguration = 'organization-configuration',
}

const ConnectionDetailsPage = ({ params }) => {
  useDocumentTitle('Connection Details')
  const [activeTab, setActiveTab] = useState(ConnectionTabs.Details)
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
  const showActions = canUpdateConnections || canDeleteConnections

  const {
    data: connectionData,
    isLoading,
    isFetching,
    refetch,
  } = useGetAzdoBoardsConnectionById(params.id)
  const azdoOrgUrl = connectionData?.configuration?.organizationUrl

  const syncOrganizationConfigurationMutation =
    useSyncAzdoBoardsConnectionOrganizationMutation()

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

  const syncOrganizationConfiguration = useCallback(async () => {
    try {
      await syncOrganizationConfigurationMutation.mutateAsync(params.id)
      messageApi.success(
        'Successfully imported organization processes and projects.',
      )
    } catch (error) {
      console.error(error)
      messageApi.error(
        `Failed to initialize organization. Error: ${error.supportMessage}`,
      )
    }
    setIsSyncingOrganization(false)
  }, [syncOrganizationConfigurationMutation, messageApi, params.id])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (showActions) {
      items.push(
        {
          key: 'edit',
          label: 'Edit',
          disabled: !canUpdateConnections,
          onClick: () => setOpenEditConnectionForm(true),
        },
        {
          key: 'delete',
          label: 'Delete',
          disabled: !canDeleteConnections,
          onClick: () => setOpenDeleteConnectionForm(true),
        },
        {
          key: 'divider',
          type: 'divider',
        },
        {
          key: 'sync-organization',
          label: 'Sync Organization Configuration',
          disabled:
            (connectionData?.isValidConfiguration ?? true) &&
            !canUpdateConnections,
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
    connectionData?.isValidConfiguration,
    syncOrganizationConfiguration,
    showActions,
  ])

  const actions = () => {
    return (
      <Dropdown menu={{ items: actionsMenuItems }}>
        <Button>
          <Space>
            Actions
            <DownOutlined />
          </Space>
        </Button>
      </Dropdown>
    )
  }

  if (!isLoading && !isFetching && !connectionData) {
    notFound()
  }

  return (
    <>
      {contextHolder}
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
        actions={showActions && actions()}
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
