'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect, useState } from 'react'
import ConnectionDetails from './connection-details'
import { Button, Card } from 'antd'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import useAuth from '@/src/app/components/contexts/auth'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetConnectionById } from '@/src/services/queries/app-integration-queries'
import { notFound } from 'next/navigation'

const ConnectionDetailsPage = ({ params }) => {
  useDocumentTitle('Connection Details')
  const {
    data: connectionData,
    isLoading,
    isFetching,
    refetch,
  } = useGetConnectionById(params.id)
  const { setBreadcrumbRoute } = useBreadcrumbs()
  const [activeTab, setActiveTab] = useState('details')
  const [openUpdateConnectionForm, setOpenUpdateConnectionForm] =
    useState<boolean>(false)

  const { hasClaim } = useAuth()
  const canUpdateConnections = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )
  const showActions = canUpdateConnections

  const tabs = [
    {
      key: 'details',
      tab: 'Details',
      content: <ConnectionDetails connection={connectionData} />,
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

  // const onUpdateConnectionFormClosed = (wasSaved: boolean) => {
  //   setOpenUpdateConnectionForm(false)
  //   if (wasSaved) {
  //     setLastRefresh(Date.now())
  //   }
  // }

  // const Actions = () => {
  //   return (
  //     <>
  //       {canUpdateConnections && (
  //         <Button onClick={() => setOpenUpdateConnectionForm(true)}>
  //           Edit
  //         </Button>
  //       )}
  //     </>
  //   )
  // }

  if (!isLoading && !isFetching && !connectionData) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title={connectionData?.name}
        subtitle="Connection Details"
        //actions={showActions && <Actions />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
    </>
  )
}

const PageWithAuthorization = authorizePage(
  ConnectionDetailsPage,
  'Permission',
  'Permissions.Connections.View',
)

export default PageWithAuthorization
