'use client'

import { ModaGrid } from '@/src/app/components/common'
import { RoadmapListDto } from '@/src/services/moda-api'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useMemo } from 'react'

export interface RoadmapsGridProps {
  roadmapsData: RoadmapListDto[]
  roadmapsLoading: boolean
  refreshRoadmaps: () => void
}

const RoadmapLinkCellRenderer = ({ value, data }) => {
  return <Link href={`/planning/roadmaps/${data.key}`}>{value}</Link>
}

const RoadmapsGrid: React.FC<RoadmapsGridProps> = (
  props: RoadmapsGridProps,
) => {
  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: RoadmapLinkCellRenderer },
      {
        field: 'start',
        valueGetter: (params) => dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        valueGetter: (params) => dayjs(params.data.end).format('M/D/YYYY'),
      },
      { field: 'isPublic' },
    ],
    [],
  )
  return (
    <ModaGrid
      columnDefs={columnDefs}
      rowData={props.roadmapsData}
      loadData={props.refreshRoadmaps}
      isDataLoading={props.roadmapsLoading}
    />
  )
}

export default RoadmapsGrid
