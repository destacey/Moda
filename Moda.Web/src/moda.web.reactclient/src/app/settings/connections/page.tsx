'use client'

import { useCallback, useMemo, useState } from 'react'
import { ModaGrid, PageTitle } from '../../components/common'
import { authorizePage } from '../../components/hoc'
import { useDocumentTitle } from '../../hooks'
import useAuth from '../../components/contexts/auth'
import { useGetAzdoBoardsConnections } from '@/src/services/queries/app-integration-queries'
import { Button, Space, Switch } from 'antd'
import CreateConnectionForm from './components/create-connection-form'
import Link from 'next/link'
import { ConnectionListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'

const ConnectionLinkCellRenderer = ({ value, data }) => {
  return <Link href={`/settings/connections/${data.id}`}>{value}</Link>
}

const ConnectionsPage = () => {
  useDocumentTitle('Connections')
  const [openCreateConnectionForm, setOpenCreateConnectionForm] =
    useState(false)
  const [includeDisabled, setIncludeDisabled] = useState(false)
  const {
    data: connectionsData,
    isLoading,
    refetch,
  } = useGetAzdoBoardsConnections(includeDisabled)

  const { hasClaim } = useAuth()
  const canCreateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Create',
  )
  const showActions = canCreateConnection

  const columnDefs = useMemo<ColDef<ConnectionListDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'name', cellRenderer: ConnectionLinkCellRenderer },
      { field: 'connector' },
      { field: 'isActive' },
      { field: 'isValidConfiguration' },
    ],
    [],
  )

  const actions = () => {
    if (!showActions) return null
    return (
      <>
        {canCreateConnection && (
          <Button onClick={() => setOpenCreateConnectionForm(true)}>
            Create Connection
          </Button>
        )}
      </>
    )
  }

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const onIncludeDisabledChange = (checked: boolean) => {
    setIncludeDisabled(checked)
  }

  const controlItems = [
    {
      label: (
        <Space>
          <Switch
            size="small"
            checked={includeDisabled}
            onChange={onIncludeDisabledChange}
          />
          Include Disabled
        </Space>
      ),
      key: '0',
    },
  ]

  return (
    <>
      <PageTitle title="Connections" actions={actions()} />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={connectionsData}
        loadData={refresh}
        isDataLoading={isLoading}
      />

      {openCreateConnectionForm && (
        <CreateConnectionForm
          showForm={openCreateConnectionForm}
          onFormCreate={() => setOpenCreateConnectionForm(false)}
          onFormCancel={() => setOpenCreateConnectionForm(false)}
        />
      )}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  ConnectionsPage,
  'Permission',
  'Permissions.Connections.View',
)

export default PageWithAuthorization
