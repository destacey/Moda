'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect, useState } from 'react'
import AzdoBoardsConnectionDetails from './azdo-boards-connection-details'
import { Button, Card, Divider, Space, Tabs, message } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import {
  useGetAzdoBoardsConnectionById,
  useImportAzdoBoardsConnectionWorkspacesMutation,
} from '@/src/services/queries/app-integration-queries'
import { notFound, usePathname } from 'next/navigation'
import EditConnectionForm from '../components/edit-connection-form'
import AzdoBoardsWorkspaces from './azdo-boards-workspaces'
import { ExportOutlined } from '@ant-design/icons'
import Link from 'next/link'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'

enum ConnectionTabs {
  Details = 'details',
  WorkspaceConfiguration = 'workspace-configuration',
}

const ConnectionDetailsPage = ({ params }) => {
  useDocumentTitle('Connection Details')
  const [activeTab, setActiveTab] = useState(ConnectionTabs.Details)
  const [isImportingWorkspaces, setIsImportingWorkspaces] = useState(false)
  const [openEditConnectionForm, setOpenEditConnectionForm] =
    useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()
  const dispatch = useAppDispatch()
  const pathname = usePathname()

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
  const azdoOrgUrl = connectionData?.configuration?.organizationUrl

  const importWorkspacesMutation =
    useImportAzdoBoardsConnectionWorkspacesMutation()

  const tabs = [
    {
      key: ConnectionTabs.Details,
      tab: 'Details',
      content: <AzdoBoardsConnectionDetails connection={connectionData} />,
    },
    {
      key: ConnectionTabs.WorkspaceConfiguration,
      tab: 'Workspace Configuration',
      content: (
        <AzdoBoardsWorkspaces
          workspaces={connectionData?.configuration?.workspaces}
          workProcesses={connectionData?.configuration?.workProcesses}
          organizationUrl={connectionData?.configuration?.organizationUrl}
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

  const importWorkspaces = async () => {
    try {
      await importWorkspacesMutation.mutateAsync(params.id)
      messageApi.success('Successfully imported workspaces.')
    } catch (error) {
      console.error(error)
      messageApi.error('Failed to import workspaces.')
    }
    setIsImportingWorkspaces(false)
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
        actions={showActions && <Actions />}
      />
      <Card
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key: ConnectionTabs) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
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
