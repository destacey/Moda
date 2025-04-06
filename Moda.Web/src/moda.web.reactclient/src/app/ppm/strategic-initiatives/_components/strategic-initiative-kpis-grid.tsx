'use client'

import { ModaGrid } from '@/src/components/common'
import { ModaStatisticNumber } from '@/src/components/common/kpis'
import { StrategicInitiativeKpiListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { MessageInstance } from 'antd/es/message/interface'
import { FC, useCallback, useMemo } from 'react'

export interface StrategicInitiativeKpisGridProps {
  kpis: StrategicInitiativeKpiListDto[]
  isLoading: boolean
  refetch: () => void
  messageApi: MessageInstance
  gridHeight?: number | undefined
}

const StatisticNumberCellRenderer = (params) => {
  return <ModaStatisticNumber value={params.value} />
}

const StrategicInitiativeKpisGrid: FC<StrategicInitiativeKpisGridProps> = (
  props,
) => {
  const { kpis, isLoading, refetch, messageApi, gridHeight } = props

  const columnDefs = useMemo<ColDef<StrategicInitiativeKpiListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      {
        field: 'name',
        width: 300,
      },
      {
        field: 'targetValue',
        headerName: 'Target Value',
        type: 'numericColumn',
        cellRenderer: StatisticNumberCellRenderer,
      },
      {
        field: 'actualValue',
        headerName: 'Actual Value',
        type: 'numericColumn',
        cellRenderer: StatisticNumberCellRenderer,
      },
      {
        field: 'unit.name',
        headerName: 'Unit',
      },
      { field: 'targetDirection.name', headerName: 'Target Direction' },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <ModaGrid
      columnDefs={columnDefs}
      rowData={kpis}
      loadData={refresh}
      loading={isLoading}
      height={gridHeight}
      emptyMessage="No KPIs found."
    />
  )
}

export default StrategicInitiativeKpisGrid
