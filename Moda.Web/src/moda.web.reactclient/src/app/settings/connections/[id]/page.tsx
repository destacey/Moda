'use client'

import PageTitle from '@/src/app/components/common/page-title'
import AzdoBoardsConnectionDetails from './azdo-boards-connection-details'
import { useEffect, useState } from 'react'
import { Button, Card, Space, message } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import {
  useGetAzdoBoardsConnectionById,
  useImportAzdoBoardsConnectionOrganizationMutation,
} from '@/src/services/queries/app-integration-queries'
import { notFound, usePathname } from 'next/navigation'
import EditConnectionForm from '../components/edit-connection-form'
import AzdoBoardsOrganization from './azdo-boards-organization'
import { ExportOutlined } from '@ant-design/icons'
import Link from 'next/link'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import InitWorkspaceIntegrationForm from '../components/init-workspace-integration-form'

enum ConnectionTabs {
  Details = 'details',
  OrganizationConfiguration = 'organization-configuration',
}

const ConnectionDetailsPage = ({ params }) => {
  useDocumentTitle('Connection Details')
  const [activeTab, setActiveTab] = useState(ConnectionTabs.Details)
  const [isImportingOrganization, setIsImportingOrganization] = useState(false)
  const [openEditConnectionForm, setOpenEditConnectionForm] =
    useState<boolean>(false)
  // const [openInitWorkspaceForm, setOpenInitWorkspaceForm] =
  //   useState<boolean>(false)
  // const [initWorkspaceId, setInitWorkspaceId] = useState<string>(null)
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

  const importOrganizationConfigurationMutation =
    useImportAzdoBoardsConnectionOrganizationMutation()

  const initWorkspace = (workspaceId: string) => {
    // setInitWorkspaceId(workspaceId)
    // setOpenInitWorkspaceForm(true)
  }

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
          organizationUrl={connectionData?.configuration?.organizationUrl}
          initWorkspace={initWorkspace}
          //initWorkspace={() => null}
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

  // const onInitWorkspaceFormClosed = (wasSaved: boolean) => {
  //   setOpenInitWorkspaceForm(false)
  //   setInitWorkspaceId(null)
  //   refetch()
  // }

  const importOrganizationConfiguration = async () => {
    try {
      await importOrganizationConfigurationMutation.mutateAsync(params.id)
      messageApi.success(
        'Successfully imported organization processes and projects.',
      )
    } catch (error) {
      console.error(error)
      messageApi.error(
        `Failed to initialize organization. Error: ${error.supportMessage}`,
      )
    }
    setIsImportingOrganization(false)
  }

  const actions = () => {
    return (
      <>
        {canUpdateConnections && (
          <Space>
            <Button onClick={() => setOpenEditConnectionForm(true)}>
              Edit
            </Button>
            <Button
              disabled={!connectionData?.isValidConfiguration ?? true}
              loading={isImportingOrganization}
              onClick={() => {
                setIsImportingOrganization(true)
                importOrganizationConfiguration()
              }}
            >
              Import Organization
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
        actions={showActions && actions()}
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
      {/* {openInitWorkspaceForm && (
        <InitWorkspaceIntegrationForm
          showForm={openInitWorkspaceForm}
          connectionId={connectionData.id}
          externalId={initWorkspaceId}
          onFormCreate={() => onInitWorkspaceFormClosed(false)}
          onFormCancel={() => onInitWorkspaceFormClosed(false)}
        />
      )} */}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  ConnectionDetailsPage,
  'Permission',
  'Permissions.Connections.View',
)

export default PageWithAuthorization
