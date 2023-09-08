'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect, useState } from 'react'
import AzdoBoardsConnectionDetails from './azdo-boards-connection-details'
import { Button, Card } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useAuth from '@/src/app/components/contexts/auth'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetAzdoBoardsConnectionById } from '@/src/services/queries/app-integration-queries'
import { notFound, usePathname } from 'next/navigation'
import EditConnectionForm from '../components/edit-connection-form'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'

enum ConnectionTabs {
  Details = 'details',
}

const ConnectionDetailsPage = ({ params }) => {
  useDocumentTitle('Connection Details')
  const [activeTab, setActiveTab] = useState(ConnectionTabs.Details)
  const [openEditConnectionForm, setOpenEditConnectionForm] =
    useState<boolean>(false)
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

  const tabs = [
    {
      key: ConnectionTabs.Details,
      tab: 'Details',
      content: <AzdoBoardsConnectionDetails connection={connectionData} />,
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

  const Actions = () => {
    return (
      <>
        {canUpdateConnections && (
          <Button onClick={() => setOpenEditConnectionForm(true)}>Edit</Button>
        )}
      </>
    )
  }

  if (!isLoading && !isFetching && !connectionData) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title={connectionData?.name}
        subtitle="Connection Details"
        actions={showActions && <Actions />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key as ConnectionTabs)}
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
