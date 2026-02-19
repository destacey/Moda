'use client'

import { ModaGrid } from '@/src/components/common'
import { ModaStatisticNumber } from '@/src/components/common/metrics'
import { RowMenuCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import { StrategicInitiativeKpiListDto } from '@/src/services/moda-api'
import { ColDef, GetRowIdParams } from 'ag-grid-community'
import { MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { FC, useCallback, useMemo, useState } from 'react'
import {
  AddStrategicInitiativeKpiMeasurementForm,
  DeleteStrategicInitiativeKpiForm,
  EditStrategicInitiativeKpiForm,
  ManageStrategicInitiativeKpiCheckpointPlanForm,
  StrategicInitiativeKpiDetailsDrawer,
} from '.'

export interface StrategicInitiativeKpisGridProps {
  strategicInitiativeId: string
  kpis: StrategicInitiativeKpiListDto[]
  canManageKpis: boolean
  isLoading: boolean
  refetch: () => void
  gridHeight?: number | undefined
  isReadOnly?: boolean
}

const StatisticNumberCellRenderer = (params) => {
  return <ModaStatisticNumber value={params.value} />
}

interface RowMenuProps extends MenuProps {
  kpiId: string
  strategicInitiativeId: string
  canManageKpis: boolean
  onViewDetailsMenuClicked: (id: string) => void
  onEditKpiMenuClicked: (id: string) => void
  onDeleteKpiMenuClicked: (id: string) => void
  onAddMeasurementMenuClicked: (id: string) => void
  onManageCheckpointPlanMenuClicked: (id: string) => void
}

const getRowMenuItems = (props: RowMenuProps): ItemType[] => {
  if (!props.kpiId || !props.onViewDetailsMenuClicked) {
    return null
  }

  const items: ItemType[] = [
    {
      key: 'view-details',
      label: 'View Details',
      onClick: () => props.onViewDetailsMenuClicked(props.kpiId),
    },
  ]

  if (
    props.canManageKpis &&
    props.onEditKpiMenuClicked &&
    props.onDeleteKpiMenuClicked &&
    props.onAddMeasurementMenuClicked &&
    props.onManageCheckpointPlanMenuClicked
  ) {
    items.push(
      { key: 'divider', type: 'divider' },
      {
        key: 'edit-kpi',
        label: 'Edit KPI',
        onClick: () => props.onEditKpiMenuClicked(props.kpiId),
      },
      {
        key: 'delete-kpi',
        label: 'Delete KPI',
        onClick: () => props.onDeleteKpiMenuClicked(props.kpiId),
      },
      { key: 'divider2', type: 'divider' },
      {
        key: 'manage-checkpoint-plan',
        label: 'Manage Checkpoint Plan',
        onClick: () => props.onManageCheckpointPlanMenuClicked(props.kpiId),
      },
      {
        key: 'add-measurement',
        label: 'Add Measurement',
        onClick: () => props.onAddMeasurementMenuClicked(props.kpiId),
      },
    )
  }

  return items
}

const StrategicInitiativeKpisGrid: FC<StrategicInitiativeKpisGridProps> = (
  props,
) => {
  const {
    strategicInitiativeId,
    kpis,
    canManageKpis,
    isLoading,
    refetch,
    gridHeight,
    isReadOnly,
  } = props

  const [selectedKpiId, setSelectedKpiId] = useState<string | null>(null)
  const [openKpiDetailsDrawer, setOpenKpiDetailsDrawer] =
    useState<boolean>(false)
  const [openEditKpiForm, setOpenEditKpiForm] = useState<boolean>(false)
  const [openDeleteKpiForm, setOpenDeleteKpiForm] = useState<boolean>(false)
  const [openAddMeasurementForm, setOpenAddMeasurementForm] =
    useState<boolean>(false)
  const [openManageCheckpointPlanForm, setOpenManageCheckpointPlanForm] =
    useState<boolean>(false)

  const onViewDetailsMenuClicked = useCallback((id: string) => {
    setSelectedKpiId(id)
    setOpenKpiDetailsDrawer(true)
  }, [])

  const onEditKpiMenuClicked = useCallback((id: string) => {
    setSelectedKpiId(id)
    setOpenEditKpiForm(true)
  }, [])

  const onEditKpiFormClosed = (wasSaved: boolean) => {
    setOpenEditKpiForm(false)
    setSelectedKpiId(null)
    if (wasSaved) {
      refresh()
    }
  }

  const onDeleteKpiMenuClicked = useCallback((id: string) => {
    setSelectedKpiId(id)
    setOpenDeleteKpiForm(true)
  }, [])

  const onDeleteKpiFormClosed = (wasSaved: boolean) => {
    setOpenDeleteKpiForm(false)
    setSelectedKpiId(null)
    if (wasSaved) {
      refresh()
    }
  }

  const onAddMeasurementMenuClicked = useCallback((id: string) => {
    setSelectedKpiId(id)
    setOpenAddMeasurementForm(true)
  }, [])

  const onAddMeasurementFormClosed = (wasSaved: boolean) => {
    setOpenAddMeasurementForm(false)
    setSelectedKpiId(null)
    if (wasSaved) {
      refresh()
    }
  }

  const onManageCheckpointPlanMenuClicked = useCallback((id: string) => {
    setSelectedKpiId(id)
    setOpenManageCheckpointPlanForm(true)
  }, [])

  const onManageCheckpointPlanFormClosed = (wasSaved: boolean) => {
    setOpenManageCheckpointPlanForm(false)
    setSelectedKpiId(null)
    if (wasSaved) {
      refresh()
    }
  }

  const columnDefs = useMemo<ColDef<StrategicInitiativeKpiListDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        cellRenderer: (params) => {
          const menuItems = getRowMenuItems({
            kpiId: params.data.id,
            strategicInitiativeId: strategicInitiativeId,
            canManageKpis,
            onViewDetailsMenuClicked,
            onEditKpiMenuClicked,
            onDeleteKpiMenuClicked,
            onAddMeasurementMenuClicked,
            onManageCheckpointPlanMenuClicked,
          })

          return RowMenuCellRenderer({ ...params, menuItems })
        },
      },
      { field: 'key', width: 90 },
      {
        field: 'name',
        width: 300,
      },
      {
        field: 'targetValue',
        headerName: 'Target Value',
        width: 125,
        type: 'numericColumn',
        cellRenderer: StatisticNumberCellRenderer,
      },
      {
        field: 'actualValue',
        headerName: 'Actual Value',
        width: 125,
        type: 'numericColumn',
        cellRenderer: StatisticNumberCellRenderer,
      },
      {
        field: 'unit',
        headerName: 'Unit',
        width: 125,
      },
      {
        field: 'targetDirection',
        headerName: 'Target Direction',
        width: 125,
      },
    ],
    [
      canManageKpis,
      strategicInitiativeId,
      onViewDetailsMenuClicked,
      onEditKpiMenuClicked,
      onDeleteKpiMenuClicked,
      onAddMeasurementMenuClicked,
      onManageCheckpointPlanMenuClicked,
    ],
  )

  const getRowId = useCallback(
    (params: GetRowIdParams) => String(params.data.id),
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <ModaGrid
        columnDefs={columnDefs}
        rowData={kpis}
        loadData={refresh}
        loading={isLoading}
        height={gridHeight}
        emptyMessage="No KPIs found."
        getRowId={getRowId}
      />
      {selectedKpiId && (
        <StrategicInitiativeKpiDetailsDrawer
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          drawerOpen={openKpiDetailsDrawer}
          onDrawerClose={() => {
            setOpenKpiDetailsDrawer(false)
            setSelectedKpiId(null)
          }}
          canManageKpis={canManageKpis && !isReadOnly}
          onRefresh={refresh}
        />
      )}
      {openEditKpiForm && selectedKpiId && (
        <EditStrategicInitiativeKpiForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          showForm={openEditKpiForm}
          onFormComplete={() => onEditKpiFormClosed(true)}
          onFormCancel={() => onEditKpiFormClosed(false)}
        />
      )}
      {openDeleteKpiForm && selectedKpiId && (
        <DeleteStrategicInitiativeKpiForm
          strategicInitiativeId={strategicInitiativeId}
          kpi={kpis.find((kpi) => kpi.id === selectedKpiId)}
          showForm={openDeleteKpiForm}
          onFormComplete={() => onDeleteKpiFormClosed(true)}
          onFormCancel={() => onDeleteKpiFormClosed(false)}
        />
      )}
      {openAddMeasurementForm && selectedKpiId && (
        <AddStrategicInitiativeKpiMeasurementForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          showForm={openAddMeasurementForm}
          onFormComplete={() => onAddMeasurementFormClosed(true)}
          onFormCancel={() => onAddMeasurementFormClosed(false)}
        />
      )}
      {openManageCheckpointPlanForm && selectedKpiId && (
        <ManageStrategicInitiativeKpiCheckpointPlanForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          showForm={openManageCheckpointPlanForm}
          onFormComplete={() => onManageCheckpointPlanFormClosed(true)}
          onFormCancel={() => onManageCheckpointPlanFormClosed(false)}
        />
      )}
    </>
  )
}

export default StrategicInitiativeKpisGrid
