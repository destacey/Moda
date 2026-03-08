'use client'

import PageTitle from '@/src/components/common/page-title'
import ModaGrid from '../../../../components/common/moda-grid'
import { useCallback, useMemo, useState } from 'react'
import { authorizePage } from '../../../../components/hoc'
import useAuth from '../../../../components/contexts/auth'
import { MenuProps, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { useDocumentTitle } from '../../../../hooks'
import { PageActions } from '../../../../components/common'
import { ControlItemSwitch } from '../../../../components/common/control-items-menu'
import { FeatureFlagListDto } from '@/src/services/moda-api'
import {
  useGetFeatureFlagsQuery,
  useToggleFeatureFlagMutation,
} from '@/src/store/features/admin/feature-flags-api'
import CreateFeatureFlagForm from './_components/create-feature-flag-form'
import EditFeatureFlagForm from './_components/edit-feature-flag-form'
import { ColDef } from 'ag-grid-community'
import { useMessage } from '@/src/components/contexts/messaging'

const FeatureFlagsListPage = () => {
  useDocumentTitle('Feature Flags')
  const messageApi = useMessage()
  const [openCreateForm, setOpenCreateForm] = useState(false)
  const [editingFlagId, setEditingFlagId] = useState<number | null>(null)
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

  const {
    data: featureFlags = [],
    isLoading,
    refetch,
  } = useGetFeatureFlagsQuery({ includeArchived })

  const [toggleFeatureFlag] = useToggleFeatureFlagMutation()

  const handleToggle = useCallback(
    async (id: number, isEnabled: boolean) => {
      try {
        await toggleFeatureFlag({ id, isEnabled }).unwrap()
        messageApi.success(
          `Feature flag ${isEnabled ? 'enabled' : 'disabled'}.`,
        )
      } catch {
        messageApi.error('Failed to toggle feature flag.')
      }
    },
    [toggleFeatureFlag, messageApi],
  )

  const columnDefs = useMemo<ColDef<FeatureFlagListDto>[]>(
    () => [
      {
        field: 'name',
        headerName: 'Name',
        width: 250,
        cellRenderer: canUpdate
          ? ({ data }: { data: FeatureFlagListDto }) => (
              <a onClick={() => setEditingFlagId(data.id)}>{data.name}</a>
            )
          : undefined,
      },
      { field: 'displayName', headerName: 'Display Name', width: 250 },
      {
        field: 'isEnabled',
        headerName: 'Enabled',
        width: 120,
        cellRenderer: ({ data }: { data: FeatureFlagListDto }) => (
          <Switch
            checked={data.isEnabled}
            onChange={(checked) => handleToggle(data.id, checked)}
            disabled={!canUpdate || data.isArchived}
            size="small"
          />
        ),
      },
      {
        field: 'isArchived',
        headerName: 'Archived',
        width: 120,
        hide: !includeArchived,
      },
    ],
    [canUpdate, handleToggle, includeArchived],
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
    </>
  )
}

const PageWithAuthorization = authorizePage(
  FeatureFlagsListPage,
  'Permission',
  'Permissions.FeatureFlags.View',
)

export default PageWithAuthorization
