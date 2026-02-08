'use client'

import { ModaGrid } from '@/src/components/common'
import { renderTeamLinkHelper } from '@/src/components/common/moda-grid-cell-renderers'
import { SprintListDto } from '@/src/services/moda-api'
import {
  ColDef,
  ICellRendererParams,
  ValueFormatterParams,
} from 'ag-grid-community'
import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import Link from 'next/link'
import { FC, useMemo } from 'react'

dayjs.extend(utc)

export interface SprintsGridProps {
  sprints: SprintListDto[]
  isLoading: boolean
  refetch: () => void
  hideTeam?: boolean
  gridHeight?: number | undefined
}

const sprintLinkCellRenderer = (params: ICellRendererParams<SprintListDto>) => (
  <Link href={`/planning/sprints/${params.data.key}`}>{params.value}</Link>
)

const teamCellRenderer = (params: ICellRendererParams<SprintListDto>) =>
  renderTeamLinkHelper(params.data?.team)

const utcAsCalendarDateValueFormatter = (
  params: ValueFormatterParams<SprintListDto>,
) => params.value && dayjs.utc(params.value).format('M/D/YYYY')

const SprintsGrid: FC<SprintsGridProps> = (props: SprintsGridProps) => {
  const { refetch, sprints = [] } = props

  const columnDefs = useMemo<ColDef<SprintListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', width: 250, cellRenderer: sprintLinkCellRenderer },
      {
        field: 'team.name',
        headerName: 'Team',
        width: 200,
        hide: props.hideTeam,
        cellRenderer: teamCellRenderer,
      },
      { field: 'state.name', headerName: 'State', width: 125 },
      {
        field: 'start',
        headerName: 'Start',
        width: 150,
        sort: 'desc',
        cellDataType: 'date',
        filterParams: {
          includeTime: false,
        },
        valueFormatter: utcAsCalendarDateValueFormatter,
      },
      {
        field: 'end',
        headerName: 'End',
        width: 150,
        cellDataType: 'date',
        filterParams: {
          includeTime: false,
        },
        valueFormatter: utcAsCalendarDateValueFormatter,
      },
    ],
    [props.hideTeam],
  )

  return (
    <ModaGrid
      columnDefs={columnDefs}
      rowData={sprints}
      loadData={refetch}
      loading={props.isLoading}
      height={props.gridHeight ?? 650}
      emptyMessage="No sprints found."
    />
  )
}

export default SprintsGrid
