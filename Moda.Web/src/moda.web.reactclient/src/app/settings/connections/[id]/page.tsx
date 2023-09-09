'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect, useState } from 'react'
import AzdoBoardsConnectionDetails from './azdo-boards-connection-details'
import { Button, Divider, Space, Tabs, message } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import useAuth from '@/src/app/components/contexts/auth'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { authorizePage } from '@/src/app/components/hoc'
import {
  useGetAzdoBoardsConnectionById,
  useImportAzdoBoardsConnectionWorkspacesMutation,
} from '@/src/services/queries/app-integration-queries'
import { notFound } from 'next/navigation'
import EditConnectionForm from '../components/edit-connection-form'

enum ConnectionTabs {
  Details = 'details',
  WorkspaceConfiguration = 'workspace-configuration',
}

const ConnectionDetailsPage = ({ params }) => {
  useDocumentTitle('Connection Details')
  const [activeTab, setActiveTab] = useState(ConnectionTabs.Details)
  const [isImportingWorkspaces, setIsImportingWorkspaces] = useState(false)
  const { setBreadcrumbRoute } = useBreadcrumbs()
  const [openEditConnectionForm, setOpenEditConnectionForm] =
    useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdateConnections = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )
  const showActions = canUpdateConnections

  const {
    data: connectionData,
    isLoading,
    isFetching,
    refetch,
  } = useGetAzdoBoardsConnectionById(params.id)

  const importWorkspacesMutation =
    useImportAzdoBoardsConnectionWorkspacesMutation()

  const tabs = [
    {
      key: ConnectionTabs.Details,
      label: 'Details',
      children: <AzdoBoardsConnectionDetails connection={connectionData} />,
    },
    {
      key: ConnectionTabs.WorkspaceConfiguration,
      label: 'Workspace Configuration',
      children: null,
    },
  ]

  useEffect(() => {
    if (!connectionData) return

    const breadcrumbRoute: ItemType[] = [
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
    setBreadcrumbRoute(breadcrumbRoute)
  }, [connectionData, setBreadcrumbRoute])

  const onEditConnectionFormClosed = (wasSaved: boolean) => {
    setOpenEditConnectionForm(false)
    if (wasSaved) {
      refetch()
    }
  }

  const importWorkspaces = async () => {
    try {
      await importWorkspacesMutation.mutateAsync(params.id)
      setIsImportingWorkspaces(false)
      messageApi.success('Successfully imported workspaces.')
    } catch (error) {
      console.error(error)
    }
  }

  const Actions = () => {
    return (
      <>
        {canUpdateConnections && (
          <Space>
            <Button onClick={() => setOpenEditConnectionForm(true)}>
              Edit
            </Button>
            <Button
              disabled={!connectionData?.isValidConfiguration ?? true}
              loading={isImportingWorkspaces}
              onClick={() => {
                setIsImportingWorkspaces(true)
                importWorkspaces()
              }}
            >
              Import Workspaces
            </Button>
          </Space>
        )}
      </>
    )
  }

  if (!isLoading && !isFetching && !connectionData) {
    notFound()
  }

  return (
    <>
      {contextHolder}
      <PageTitle
        title={connectionData?.name}
        subtitle="Connection Details"
        actions={showActions && <Actions />}
      />
      <Divider />
      <Tabs
        tabPosition="left"
        size="small"
        tabBarGutter={0}
        tabBarStyle={{ minHeight: 400 }}
        defaultActiveKey={activeTab}
        items={tabs}
      />
      {openEditConnectionForm && (
        <EditConnectionForm
          showForm={openEditConnectionForm}
          id={connectionData?.id}
          onFormUpdate={() => onEditConnectionFormClosed(true)}
          onFormCancel={() => onEditConnectionFormClosed(false)}
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
