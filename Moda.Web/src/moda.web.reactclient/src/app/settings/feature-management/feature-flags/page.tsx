'use client'

import PageTitle from '@/src/components/common/page-title'
import ModaGrid from '../../../../components/common/moda-grid'
import { useCallback, useMemo, useState } from 'react'
import { authorizePage } from '../../../../components/hoc'
import useAuth from '../../../../components/contexts/auth'
import { MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { useDocumentTitle } from '../../../../hooks'
import { PageActions } from '../../../../components/common'
import { ControlItemSwitch } from '../../../../components/common/control-items-menu'
import { RowMenuCellRenderer } from '../../../../components/common/moda-grid-cell-renderers'
import { FeatureFlagListDto } from '@/src/services/moda-api'
import { useGetFeatureFlagsQuery } from '@/src/store/features/admin/feature-flags-api'
import CreateFeatureFlagForm from './_components/create-feature-flag-form'
import EditFeatureFlagForm from './_components/edit-feature-flag-form'
import FeatureFlagDetailsDrawer from './_components/feature-flag-details-drawer'
import useFeatureFlagActions from './_components/use-feature-flag-actions'
import { ColDef } from 'ag-grid-community'
const FeatureFlagsListPage = () => {
  useDocumentTitle('Feature Flags')
  const [openCreateForm, setOpenCreateForm] = useState(false)
  const [editingFlagId, setEditingFlagId] = useState<number | null>(null)
  const [viewingFlagId, setViewingFlagId] = useState<number | null>(null)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [includeArchived, setIncludeArchived] = useState(false)

  const { hasClaim } = useAuth()
  const canCreate = hasClaim(
    'Permission',
    'Permissions.FeatureFlags.Create',
  )
  const canUpdate = hasClaim(
    'Permission',
    'Permissions.FeatureFlags.Update',
  )
  const canDelete = hasClaim(
    'Permission',
    'Permissions.FeatureFlags.Delete',
  )
  const showRowActions = canUpdate || canDelete

  const {
    data: featureFlags = [],
    isLoading,
    refetch,
  } = useGetFeatureFlagsQuery({ includeArchived })

  const { handleToggle, handleArchive } = useFeatureFlagActions()

  const openDetailsDrawer = useCallback((id: number) => {
    setViewingFlagId(id)
    setDrawerOpen(true)
  }, [])

  const closeDetailsDrawer = useCallback(() => {
    setDrawerOpen(false)
    setViewingFlagId(null)
  }, [])

  const columnDefs = useMemo<ColDef<FeatureFlagListDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        hide: !showRowActions,
        suppressHeaderMenuButton: true,
        cellRenderer: (params: { data: FeatureFlagListDto }) => {
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

          if (
            canDelete &&
            !flag.isSystem &&
            !flag.isArchived
          ) {
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
        cellRenderer: ({ data }: { data: FeatureFlagListDto }) =>
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
    ],
    [canUpdate, canDelete, showRowActions, openDetailsDrawer, handleToggle, handleArchive, includeArchived],
  )

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []
    if (canCreate) {
      items.push({
        label: 'Create Feature Flag',
        key: 'create-feature-flag',
        onClick: () => setOpenCreateForm(true),
      })
    }
    return items
  }, [canCreate])

  const controlItems = useMemo<ItemType[]>(
    () => [
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
    ],
    [includeArchived],
  )

  return (
    <>
      <PageTitle
        title="Feature Flags"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        rowData={featureFlags}
        loadData={refetch}
        loading={isLoading}
        gridControlMenuItems={controlItems}
      />
      {openCreateForm && (
        <CreateFeatureFlagForm
          onFormCreate={() => setOpenCreateForm(false)}
          onFormCancel={() => setOpenCreateForm(false)}
        />
      )}
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
