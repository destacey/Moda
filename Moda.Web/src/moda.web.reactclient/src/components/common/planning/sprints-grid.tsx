'use client'

import { ModaGrid } from '@/src/components/common'
import { TeamNameLinkCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import { SprintListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import dayjs from 'dayjs'
import { useCallback, useMemo } from 'react'

export interface SprintsGridProps {
  sprints: SprintListDto[]
  isLoading: boolean
  refetch: () => void
  hideTeam?: boolean
  gridHeight?: number | undefined
}

const SprintsGrid: React.FC<SprintsGridProps> = (props: SprintsGridProps) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<SprintListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', width: 300 },
      {
        field: 'team.name',
        headerName: 'Team',
        width: 200,
        hide: props.hideTeam,
        cellRenderer: (params) =>
          TeamNameLinkCellRenderer({ data: params.data.team }),
      },
      { field: 'state.name', headerName: 'State', width: 125 },
      {
        field: 'start',
        headerName: 'Start',
        width: 150,
        sort: 'desc',
        filter: 'agDateColumnFilter',
        filterParams: {
          includeTime: false,
        },
        valueFormatter: (params) =>
          params.value && dayjs(params.value).format('M/D/YYYY'),
      },
      {
        field: 'end',
        headerName: 'End',
        width: 150,
        filter: 'agDateColumnFilter',
        filterParams: {
          includeTime: false,
        },
        valueFormatter: (params) =>
          params.value && dayjs(params.value).format('M/D/YYYY'),
      },
    ],
    [props.hideTeam],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <ModaGrid
        columnDefs={columnDefs}
        rowData={props.sprints}
        loadData={refresh}
        loading={props.isLoading}
        height={props.gridHeight}
        emptyMessage="No sprints found."
      />
    </>
  )
}

export default SprintsGrid
