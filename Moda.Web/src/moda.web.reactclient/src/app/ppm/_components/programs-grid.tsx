'use client'

import { ModaGrid } from '@/src/components/common'
import {
  LifecycleStatusTagCellRenderer,
  PortfolioLinkCellRenderer,
  ProgramLinkCellRenderer,
} from '@/src/components/common/moda-grid-cell-renderers'
import { ProgramListDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { ColDef } from 'ag-grid-community'
import dayjs from 'dayjs'
import { FC, useCallback, useMemo } from 'react'

export interface ProgramsGridProps {
  programs: ProgramListDto[]
  isLoading: boolean
  refetch: () => void
  hidePortfolio?: boolean
  gridHeight?: number | undefined
  viewSelector?: React.ReactNode | undefined
}

const ProgramsGrid: FC<ProgramsGridProps> = (props: ProgramsGridProps) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<ProgramListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      {
        field: 'name',
        cellRenderer: ProgramLinkCellRenderer,
        width: 300,
        initialSort: 'asc',
      },
      {
        field: 'status.name',
        headerName: 'Status',
        width: 125,
        cellRenderer: LifecycleStatusTagCellRenderer,
      },
      {
        field: 'portfolio.name',
        headerName: 'Portfolio',
        width: 200,
        hide: props.hidePortfolio,
        cellRenderer: (params) =>
          PortfolioLinkCellRenderer({ ...params, data: params.data.portfolio }),
      },
      {
        field: 'start',
        width: 125,
        valueGetter: (params) =>
          params.data.start && dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        width: 125,
        valueGetter: (params) =>
          params.data.end && dayjs(params.data.end).format('M/D/YYYY'),
      },
      {
        field: 'programSponsors',
        headerName: 'Sponsors',
        valueGetter: (params) => getSortedNames(params.data.programSponsors),
      },
      {
        field: 'programOwners',
        headerName: 'Owners',
        valueGetter: (params) => getSortedNames(params.data.programOwners),
      },
      {
        field: 'programManagers',
        headerName: 'Managers',
        valueGetter: (params) => getSortedNames(params.data.programManagers),
      },
      {
        field: 'strategicThemes',
        headerName: 'Strategic Themes',
        valueGetter: (params) => getSortedNames(params.data.strategicThemes),
      },
    ],
    [props.hidePortfolio],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <ModaGrid
        columnDefs={columnDefs}
        rowData={props.programs}
        loadData={refresh}
        loading={props.isLoading}
        toolbarActions={props.viewSelector}
        height={props.gridHeight}
        emptyMessage="No programs found."
      />
    </>
  )
}

export default ProgramsGrid
