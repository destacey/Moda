'use client'

import { ModaGrid } from '@/src/components/common'
import { RoadmapListDto } from '@/src/services/moda-api'
import { ColDef, ValueFormatterParams } from 'ag-grid-community'
import dayjs from 'dayjs'
import Link from 'next/link'
import { FC, useMemo } from 'react'

export interface RoadmapsGridProps {
  roadmapsData: RoadmapListDto[]
  roadmapsLoading: boolean
  refreshRoadmaps: () => void
  gridHeight?: number | undefined
  viewSelector?: React.ReactNode | undefined
  parentRoadmapId?: string | undefined
}

const RoadmapCellRenderer = ({ value, data }) => {
  return <Link href={`/planning/roadmaps/${data.key}`}>{value}</Link>
}

const dateOnlyValueFormatter = (params: ValueFormatterParams<RoadmapListDto>) =>
  params.value && dayjs(params.value).format('M/D/YYYY')

const RoadmapsGrid: FC<RoadmapsGridProps> = (props: RoadmapsGridProps) => {
  const columnDefs = useMemo<ColDef<RoadmapListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', width: 300, cellRenderer: RoadmapCellRenderer },
      {
        field: 'start',
        width: 150,
        valueFormatter: dateOnlyValueFormatter,
      },
      {
        field: 'end',
        width: 150,
        valueFormatter: dateOnlyValueFormatter,
      },
      {
        field: 'roadmapManagers',
        valueGetter: (params) =>
          params.data.roadmapManagers
            .slice()
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((m) => m.name)
            .join(', '),
      },
      {
        field: 'visibility.name',
        headerName: 'Visibility',
        width: 125,
      },
    ],
    [],
  )

  return (
    <ModaGrid
      height={props.gridHeight ?? 650}
      columnDefs={columnDefs}
      rowData={props.roadmapsData}
      loadData={props.refreshRoadmaps}
      loading={props.roadmapsLoading}
      toolbarActions={props.viewSelector}
    />
  )
}

export default RoadmapsGrid
