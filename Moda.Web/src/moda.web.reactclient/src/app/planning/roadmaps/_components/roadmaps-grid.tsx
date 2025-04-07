'use client'

import { ModaGrid } from '@/src/components/common'
import { RoadmapListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useMemo } from 'react'

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

// TODO: split this into two grids sharing one column definition
const RoadmapsGrid: React.FC<RoadmapsGridProps> = (
  props: RoadmapsGridProps,
) => {
  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo<ColDef<RoadmapListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', width: 350, cellRenderer: RoadmapCellRenderer },
      {
        field: 'start',
        width: 125,
        valueGetter: (params) => dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        width: 125,
        valueGetter: (params) => dayjs(params.data.end).format('M/D/YYYY'),
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
