'use client'

import PageTitle from '@/src/components/common/page-title'
import ModaGrid from '../../../../components/common/moda-grid'
import { useMemo, useState } from 'react'
import { authorizePage } from '../../../../components/hoc'
import useAuth from '../../../../components/contexts/auth'
import { ItemType } from 'antd/es/menu/interface'
import { useDocumentTitle } from '../../../../hooks'
import { ControlItemSwitch } from '../../../../components/common/control-items-menu'
import { RowMenuCellRenderer } from '../../../../components/common/moda-grid-cell-renderers'
import { FeatureFlagListDto } from '@/src/services/wayd-api'
import { useGetFeatureFlagsQuery } from '@/src/store/features/admin/feature-flags-api'
import EditFeatureFlagForm from './_components/edit-feature-flag-form'
import FeatureFlagDetailsDrawer from './_components/feature-flag-details-drawer'
import useFeatureFlagActions from './_components/use-feature-flag-actions'
import { ColDef } from 'ag-grid-community'
import { CustomCellRendererProps } from 'ag-grid-react'
const FeatureFlagsListPage = () => {
  useDocumentTitle('Feature Flags')
  const [editingFlagId, setEditingFlagId] = useState<number | null>(null)
  const [viewingFlagId, setViewingFlagId] = useState<number | null>(null)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [includeArchived, setIncludeArchived] = useState(false)

  const { hasPermissionClaim } = useAuth()
  const canUpdate = hasPermissionClaim('Permissions.FeatureFlags.Update')
  const canDelete = hasPermissionClaim('Permissions.FeatureFlags.Delete')
  const showRowActions = canUpdate || canDelete

  const {
    data: featureFlags = [],
    isLoading,
    refetch,
  } = useGetFeatureFlagsQuery({ includeArchived })

  const { handleToggle, handleArchive } = useFeatureFlagActions()

  const refresh = () => {
    refetch()
  }

  const closeDetailsDrawer = () => {
    setDrawerOpen(false)
    setViewingFlagId(null)
  }

  const columnDefs = useMemo<ColDef<FeatureFlagListDto>[]>(() => {
    const openDetailsDrawer = (id: number) => {
      setViewingFlagId(id)
      setDrawerOpen(true)
    }

    return [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        hide: !showRowActions,
        suppressHeaderMenuButton: true,
        cellRenderer: (params: CustomCellRendererProps<FeatureFlagListDto>) => {
          const flag = params.data
          if (!flag) return null
          const items: ItemType[] = []

          if (canUpdate) {
            items.push({
              key: 'edit',
              label: 'Edit',
              onClick: () => setEditingFlagId(flag.id),
            })
            items.push({
              key: 'toggle',
              label: flag.isEnabled ? 'Disable' : 'Enable',
              onClick: () => handleToggle(flag),
            })
          }

          if (canDelete && !flag.isSystem && !flag.isArchived) {
            if (items.length > 0) {
              items.push({ key: 'divider', type: 'divider' })
            }
            items.push({
              key: 'archive',
              label: 'Archive',
              danger: true,
              onClick: () => handleArchive(flag),
            })
          }

          return <RowMenuCellRenderer {...params} menuItems={items} />
        },
      },
      {
        field: 'name',
        headerName: 'Name',
        width: 250,
        cellRenderer: ({
          data,
        }: CustomCellRendererProps<FeatureFlagListDto>) =>
          data ? (
            <a onClick={() => openDetailsDrawer(data.id)}>{data.name}</a>
          ) : null,
      },
      { field: 'displayName', headerName: 'Display Name', width: 250 },
      {
        field: 'isSystem',
        headerName: 'Type',
        width: 120,
        cellDataType: false,
        valueFormatter: ({ value }) => (value ? 'System' : 'User'),
      },
      {
        field: 'isEnabled',
        headerName: 'Enabled',
        width: 120,
        cellDataType: false,
        valueFormatter: ({ value }) => (value ? 'true' : 'false'),
      },
      {
        field: 'isArchived',
        headerName: 'Archived',
        width: 120,
        hide: !includeArchived,
      },
    ]}, [showRowActions, canUpdate, canDelete, includeArchived, handleToggle, handleArchive])

  const controlItems: ItemType[] = [
      {
        label: (
          <ControlItemSwitch
            label="Include Archived"
            checked={includeArchived}
            onChange={setIncludeArchived}
          />
        ),
        key: 'include-archived',
        onClick: () => setIncludeArchived((prev) => !prev),
      },
    ]

  return (
    <>
      <PageTitle title="Feature Flags" />
      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        rowData={featureFlags}
        loadData={refresh}
        loading={isLoading}
        gridControlMenuItems={controlItems}
      />
      {editingFlagId !== null && (
        <EditFeatureFlagForm
          featureFlagId={editingFlagId}
          onFormSave={() => setEditingFlagId(null)}
          onFormCancel={() => setEditingFlagId(null)}
        />
      )}
      {viewingFlagId !== null && (
        <FeatureFlagDetailsDrawer
          featureFlagId={viewingFlagId}
          drawerOpen={drawerOpen}
          onDrawerClose={closeDetailsDrawer}
        />
      )}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  FeatureFlagsListPage,
  'Permission',
  'Permissions.FeatureFlags.View',
)

export default PageWithAuthorization
