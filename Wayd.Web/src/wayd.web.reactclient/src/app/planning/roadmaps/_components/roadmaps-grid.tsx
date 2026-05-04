'use client'

import { WaydGrid } from '@/src/components/common'
import { RoadmapListDto } from '@/src/services/wayd-api'
import { ColDef, ICellRendererParams, ValueFormatterParams } from 'ag-grid-community'
import dayjs from 'dayjs'
import Link from 'next/link'
import { FC, ReactNode, useMemo } from 'react'

export interface RoadmapsGridProps {
  roadmapsData: RoadmapListDto[]
  roadmapsLoading: boolean
  refreshRoadmaps: () => void
  gridHeight?: number | undefined
  viewSelector?: ReactNode | undefined
  parentRoadmapId?: string | undefined
}

type RoadmapGridRow = RoadmapListDto & {
  roadmapManagersDisplay: string
}

const RoadmapCellRenderer = ({ value, data }: ICellRendererParams<RoadmapGridRow>) => {
  return <Link href={`/planning/roadmaps/${data!.key}`}>{value}</Link>
}

const dateOnlyValueFormatter = (params: ValueFormatterParams<RoadmapGridRow>) =>
  params.value && dayjs(params.value).format('M/D/YYYY')

const RoadmapsGrid: FC<RoadmapsGridProps> = (props: RoadmapsGridProps) => {
  const rowData: RoadmapGridRow[] = props.roadmapsData.map((roadmap) => ({
    ...roadmap,
    roadmapManagersDisplay: roadmap.roadmapManagers
      .slice()
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((m) => m.name)
      .join(', '),
  }))

  const columnDefs = useMemo<ColDef<RoadmapGridRow>[]>(
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
        field: 'roadmapManagersDisplay',
        headerName: 'Roadmap Managers',
      },
      {
        field: 'visibility.name',
        headerName: 'Visibility',
        width: 125,
      },
      {
        field: 'state.name',
        headerName: 'State',
        width: 120,
      },
    ],
    [],
  )

  return (
    <WaydGrid
      height={props.gridHeight ?? 650}
      columnDefs={columnDefs}
      rowData={rowData}
      loadData={props.refreshRoadmaps}
      loading={props.roadmapsLoading}
      toolbarActions={props.viewSelector}
    />
  )
}

export default RoadmapsGrid
