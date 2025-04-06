'use client'

import { ModaGrid } from '@/src/components/common'
import { ModaStatisticNumber } from '@/src/components/common/kpis'
import { RowMenuCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import { StrategicInitiativeKpiListDto } from '@/src/services/moda-api'
import { ColDef, GetRowIdParams } from 'ag-grid-community'
import { MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { MessageInstance } from 'antd/es/message/interface'
import { FC, useCallback, useMemo, useState } from 'react'
import { EditStrategicInitiativeKpiForm } from '.'

export interface StrategicInitiativeKpisGridProps {
  strategicInitiativeId: string
  kpis: StrategicInitiativeKpiListDto[]
  canManageKpis: boolean
  isLoading: boolean
  refetch: () => void
  messageApi: MessageInstance
  gridHeight?: number | undefined
}

const StatisticNumberCellRenderer = (params) => {
  return <ModaStatisticNumber value={params.value} />
}

interface RowMenuProps extends MenuProps {
  kpiId: string
  strategicInitiativeId: string
  canManageKpis: boolean
  onEditKpiMenuClicked: (id: string) => void
}

const getRowMenuItems = (props: RowMenuProps): ItemType[] => {
  if (!props.canManageKpis || !props.kpiId || !props.onEditKpiMenuClicked) {
    return null
  }

  return [
    {
      key: 'edit-kpi',
      label: 'Edit KPI',
      onClick: () => props.onEditKpiMenuClicked(props.kpiId),
    },
  ]
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
    messageApi,
    gridHeight,
  } = props

  const [selectedKpiId, setSelectedKpiId] = useState<string | null>(null)
  const [openEditKpiForm, setOpenEditKpiForm] = useState<boolean>(false)

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

  const columnDefs = useMemo<ColDef<StrategicInitiativeKpiListDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        hide: !canManageKpis,
        cellRenderer: (params) => {
          const menuItems = getRowMenuItems({
            kpiId: params.data.id,
            strategicInitiativeId: strategicInitiativeId,
            canManageKpis,
            onEditKpiMenuClicked,
          })

          return RowMenuCellRenderer({ menuItems })
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
        field: 'unit.name',
        headerName: 'Unit',
        width: 125,
      },
      {
        field: 'targetDirection.name',
        headerName: 'Target Direction',
        width: 125,
      },
    ],
    [canManageKpis, strategicInitiativeId, onEditKpiMenuClicked],
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
      {openEditKpiForm && selectedKpiId && (
        <EditStrategicInitiativeKpiForm
          strategicInitiativeId={strategicInitiativeId}
          kpiId={selectedKpiId}
          showForm={openEditKpiForm}
          onFormComplete={() => onEditKpiFormClosed(true)}
          onFormCancel={() => onEditKpiFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

export default StrategicInitiativeKpisGrid
