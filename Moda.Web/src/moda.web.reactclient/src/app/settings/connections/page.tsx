'use client'

import { useCallback, useMemo, useState } from 'react'
import { ModaGrid, PageActions, PageTitle } from '../../components/common'
import { authorizePage } from '../../components/hoc'
import { useDocumentTitle } from '../../hooks'
import useAuth from '../../components/contexts/auth'
import { Space, Switch } from 'antd'
import CreateConnectionForm from './components/create-connection-form'
import Link from 'next/link'
import { ConnectionListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { ItemType } from 'antd/es/menu/interface'
import { useGetAzdoConnectionsQuery } from '@/src/store/features/app-integration/azdo-integration-api'

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
    error,
    refetch,
  } = useGetAzdoConnectionsQuery(includeDisabled)

  const { hasClaim } = useAuth()
  const canCreateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Create',
  )

  const columnDefs = useMemo<ColDef<ConnectionListDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'name', cellRenderer: ConnectionLinkCellRenderer },
      { field: 'connector' },
      { field: 'isActive' },
      { field: 'isValidConfiguration' },
      { field: 'isSyncEnabled' },
    ],
    [],
  )

  const actionsMenuItems = useMemo(() => {
    const items = [] as ItemType[]
    if (canCreateConnection) {
      items.push({
        key: 'create-connection-menu-item',
        label: 'Create Connection',
        onClick: () => setOpenCreateConnectionForm(true),
      })
    }
    return items
  }, [canCreateConnection])

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
      <PageTitle
        title="Connections"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={connectionsData}
        loadData={refresh}
        loading={isLoading}
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
