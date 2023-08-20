'use client'

import { useCallback, useMemo, useState } from 'react'
import { ModaGrid, PageTitle } from '../../components/common'
import { authorizePage } from '../../components/hoc'
import { useDocumentTitle } from '../../hooks'
import useAuth from '../../components/contexts/auth'
import { useGetConnections } from '@/src/services/queries/app-integration-queries'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { Space, Switch } from 'antd'

const ConnectionsPage = () => {
  useDocumentTitle('Connections')
  const [includeDisabled, setIncludeDisabled] = useState(false)
  const { data: connectionsData, refetch } = useGetConnections(includeDisabled)

  const { hasClaim } = useAuth()
  const canCreateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Create',
  )

  const columnDefs = useMemo(
    () => [
      { field: 'id', hide: true },
      { field: 'name' },
      { field: 'connector' },
      { field: 'isActive' },
      { field: 'isValidConfiguration' },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const onIncludeDisabledChange = (checked: boolean) => {
    setIncludeDisabled(checked)
  }

  const controlItems: ItemType[] = [
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
      <PageTitle title="Connections" />

      <ModaGrid
        columnDefs={columnDefs}
        gridControlMenuItems={controlItems}
        rowData={connectionsData}
        loadData={refresh}
      />
    </>
  )
}

const PageWithAuthorization = authorizePage(
  ConnectionsPage,
  'Permission',
  'Permissions.Connections.View',
)

export default PageWithAuthorization
