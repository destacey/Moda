'use client'

import { ModaGrid } from '@/src/components/common'
import { StrategicThemeListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { MessageInstance } from 'antd/es/message/interface'
import Link from 'next/link'
import { useMemo } from 'react'

export interface StrategicThemesGridProps {
  strategicThemesData: StrategicThemeListDto[]
  strategicThemesLoading: boolean
  refreshStrategicThemes: () => void
  messageApi: MessageInstance
  gridHeight?: number | undefined
}

const StrategicThemeCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/strategic-management/strategic-themes/${data.key}`}>
      {value}
    </Link>
  )
}

const StrategicThemesGrid: React.FC<StrategicThemesGridProps> = (
  props: StrategicThemesGridProps,
) => {
  const columnDefs = useMemo<ColDef<StrategicThemeListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', width: 350, cellRenderer: StrategicThemeCellRenderer },
      {
        field: 'state.name',
        headerName: 'State',
        width: 125,
      },
    ],
    [],
  )

  return (
    <ModaGrid
      height={props.gridHeight ?? 650}
      columnDefs={columnDefs}
      rowData={props.strategicThemesData}
      loadData={props.refreshStrategicThemes}
      loading={props.strategicThemesLoading}
    />
  )
}

export default StrategicThemesGrid
