'use client'

import { ModaGrid, PageTitle } from '@/src/components/common'
import { RowMenuCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage, requireFeatureFlag } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { EstimationScaleDto } from '@/src/services/wayd-api'
import {
  useGetEstimationScalesQuery,
  useSetEstimationScaleActiveStatusMutation,
} from '@/src/store/features/planning/estimation-scales-api'
import { ColDef } from 'ag-grid-community'
import { Button } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import Link from 'next/link'
import { useEffect, useMemo, useState } from 'react'
import {
  CreateEstimationScaleForm,
  DeleteEstimationScaleForm,
  EditEstimationScaleForm,
} from './_components'
import { useMessage } from '@/src/components/contexts/messaging'

const EstimationScaleCellRenderer = ({ value, data }) => {
  return <Link href={`./estimation-scales/${data.id}`}>{value}</Link>
}

interface RowMenuProps {
  scale: EstimationScaleDto
  canUpdate: boolean
  canDelete: boolean
  onEditClicked: (id: number) => void
  onToggleActiveClicked: (scale: EstimationScaleDto) => void
  onDeleteClicked: (scale: EstimationScaleDto) => void
}

const getRowMenuItems = (props: RowMenuProps): ItemType[] => {
  if (!props.scale) return []

  const items: ItemType[] = []

  if (props.canUpdate) {
    items.push({
      key: 'edit',
      label: 'Edit',
      onClick: () => props.onEditClicked(props.scale.id),
    })
    items.push({
      key: 'toggle-active',
      label: props.scale.isActive ? 'Deactivate' : 'Activate',
      onClick: () => props.onToggleActiveClicked(props.scale),
    })
  }

  if (props.canDelete) {
    items.push({
      key: 'delete',
      label: 'Delete',
      danger: true,
      onClick: () => props.onDeleteClicked(props.scale),
    })
  }

  return items
}

const EstimationScalesPage = () => {
  useDocumentTitle('Planning - Estimation Scales')
  const [openCreateForm, setOpenCreateForm] = useState<boolean>(false)
  const [editScaleId, setEditScaleId] = useState<number | null>(null)
  const [deleteScale, setDeleteScale] =
    useState<EstimationScaleDto | null>(null)

  const messageApi = useMessage()

  const {
    data: scaleData,
    isLoading,
    error,
    refetch,
  } = useGetEstimationScalesQuery(true)
  const [setActiveStatus] = useSetEstimationScaleActiveStatusMutation()

  const { hasPermissionClaim } = useAuth()
  const canCreateEstimationScale = hasPermissionClaim(
    'Permissions.EstimationScales.Create',
  )
  const canUpdateEstimationScale = hasPermissionClaim(
    'Permissions.EstimationScales.Update',
  )
  const canDeleteEstimationScale = hasPermissionClaim(
    'Permissions.EstimationScales.Delete',
  )
  const showRowMenu = canUpdateEstimationScale || canDeleteEstimationScale

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ?? 'An error occurred while loading estimation scales',
      )
      console.error(error)
    }
  }, [error, messageApi])

  const columnDefs = useMemo<ColDef<EstimationScaleDto>[]>(() => {
    const handleEdit = (id: number) => {
      setEditScaleId(id)
    }

    const handleToggleActive = async (scale: EstimationScaleDto) => {
      try {
        const response = await setActiveStatus({
          id: scale.id,
          isActive: !scale.isActive,
        })
        if (response.error) {
          throw response.error
        }
        messageApi.success(
          `Estimation scale ${scale.isActive ? 'deactivated' : 'activated'} successfully.`,
        )
      } catch (error) {
        messageApi.error(
          'An error occurred while updating the estimation scale status.',
        )
        console.error(error)
      }
    }

    const handleDelete = (scale: EstimationScaleDto) => {
      setDeleteScale(scale)
    }

    return [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        hide: !showRowMenu,
        suppressHeaderMenuButton: true,
        cellRenderer: (params) => {
          const menuItems = getRowMenuItems({
            scale: params.data,
            canUpdate: canUpdateEstimationScale,
            canDelete: canDeleteEstimationScale,
            onEditClicked: handleEdit,
            onToggleActiveClicked: handleToggleActive,
            onDeleteClicked: handleDelete,
          })
          if (menuItems.length === 0) return null
          return RowMenuCellRenderer({ ...params, menuItems })
        },
      },
      { field: 'id', hide: true },
      { field: 'name', cellRenderer: EstimationScaleCellRenderer },
      {
        field: 'isActive',
        headerName: 'Active',
        width: 100,
        cellRenderer: (params) => (params.value ? 'Yes' : 'No'),
      },
      {
        field: 'values',
        headerName: 'Values',
        width: 100,
        valueGetter: (params) => params.data?.values?.length ?? 0,
      },
    ]}, [showRowMenu, canUpdateEstimationScale, canDeleteEstimationScale, setActiveStatus, messageApi])

  const refresh = async () => {
    refetch()
  }

  const actions = !canCreateEstimationScale ? null : (
      <Button onClick={() => setOpenCreateForm(true)}>
        Create Estimation Scale
      </Button>
    )

  const onCreateFormClosed = (wasCreated: boolean) => {
    setOpenCreateForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  const onEditFormClosed = (wasSaved: boolean) => {
    setEditScaleId(null)
    if (wasSaved) {
      refetch()
    }
  }

  const onDeleteFormClosed = (wasDeleted: boolean) => {
    setDeleteScale(null)
    if (wasDeleted) {
      refetch()
    }
  }

  return (
    <>
      <PageTitle title="Estimation Scales" actions={actions} />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        rowData={scaleData}
        loadData={refresh}
        loading={isLoading}
      />
      {openCreateForm && (
        <CreateEstimationScaleForm
          onFormComplete={() => onCreateFormClosed(true)}
          onFormCancel={() => onCreateFormClosed(false)}
        />
      )}
      {editScaleId && (
        <EditEstimationScaleForm
          estimationScaleId={editScaleId}
          onFormComplete={() => onEditFormClosed(true)}
          onFormCancel={() => onEditFormClosed(false)}
        />
      )}
      {deleteScale && (
        <DeleteEstimationScaleForm
          estimationScale={deleteScale}
          onFormComplete={() => onDeleteFormClosed(true)}
          onFormCancel={() => onDeleteFormClosed(false)}
        />
      )}
    </>
  )
}

const EstimationScalesPageWithAuthorization = requireFeatureFlag(
  authorizePage(EstimationScalesPage, 'Permission', 'Permissions.EstimationScales.View'),
  'planning-poker',
)

export default EstimationScalesPageWithAuthorization
