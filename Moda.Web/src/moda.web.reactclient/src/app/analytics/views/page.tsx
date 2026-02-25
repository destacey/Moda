'use client'

import ModaGrid from '@/src/components/common/moda-grid'
import PageTitle from '@/src/components/common/page-title'
import { ControlItemSwitch } from '@/src/components/common/control-items-menu'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { AnalyticsViewListDto, Visibility } from '@/src/services/moda-api'
import { useGetAnalyticsViewsQuery } from '@/src/store/features/analytics/analytics-views-api'
import { ColDef } from 'ag-grid-community'
import { Button } from 'antd'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { useCallback, useMemo, useState } from 'react'
import { ItemType } from 'antd/es/menu/interface'
import CreateAnalyticsViewForm from './_components/create-analytics-view-form'

const AnalyticsViewLinkCellRenderer = ({ value, data }) => {
  return <Link href={`/analytics/views/${data.id}`}>{value}</Link>
}

const AnalyticsViewsPage = () => {
  useDocumentTitle('Analytics Views')
  const router = useRouter()
  const [includeInactive, setIncludeInactive] = useState(false)
  const [openCreateForm, setOpenCreateForm] = useState(false)

  const {
    data: viewsData,
    isLoading,
    refetch,
  } = useGetAnalyticsViewsQuery(includeInactive)

  const { hasPermissionClaim } = useAuth()
  const canCreate = hasPermissionClaim('Permissions.AnalyticsViews.Create')

  const columnDefs = useMemo<ColDef<AnalyticsViewListDto>[]>(
    () => [
      {
        field: 'name',
        cellRenderer: AnalyticsViewLinkCellRenderer,
        width: 300,
      },
      { field: 'dataset', width: 200 },
      {
        field: 'visibility',
        width: 130,
        cellRenderer: ({ value }: { value: Visibility }) =>
          value === Visibility.Public ? 'Public' : 'Private',
      },
      {
        field: 'isActive',
        headerName: 'Active',
        width: 110,
        cellRenderer: ({ value }: { value: boolean }) =>
          value ? 'Yes' : 'No',
      },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const onIncludeInactiveChange = useCallback((checked: boolean) => {
    setIncludeInactive(checked)
  }, [])

  const controlItems: ItemType[] = [
    {
      label: (
        <ControlItemSwitch
          label="Include Inactive"
          checked={includeInactive}
          onChange={onIncludeInactiveChange}
        />
      ),
      key: 'include-inactive',
      onClick: () => onIncludeInactiveChange(!includeInactive),
    },
  ]

  const actions = () => {
    return (
      <>
        {canCreate && (
          <Button onClick={() => setOpenCreateForm(true)}>
            Create Analytics View
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      <PageTitle title="Analytics Views" actions={canCreate && actions()} />
      <ModaGrid
        columnDefs={columnDefs}
        rowData={viewsData}
        loading={isLoading}
        loadData={refresh}
        gridControlMenuItems={controlItems}
      />
      {openCreateForm && (
        <CreateAnalyticsViewForm
          showForm={openCreateForm}
          onFormCreate={(id: string) => {
            setOpenCreateForm(false)
            router.push(`/analytics/views/${id}`)
          }}
          onFormCancel={() => setOpenCreateForm(false)}
        />
      )}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  AnalyticsViewsPage,
  'Permission',
  'Permissions.AnalyticsViews.View',
)

export default PageWithAuthorization
