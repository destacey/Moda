'use client'

import { ModaGrid } from '@/src/components/common'
import { PortfolioLinkCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import {
  NavigationDto,
  StrategicInitiativeListDto,
} from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { ColDef } from 'ag-grid-community'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useCallback, useMemo } from 'react'

export interface StrategicInitiativesGridProps {
  strategicInitiatives: StrategicInitiativeListDto[]
  isLoading: boolean
  refetch: () => void
  hidePortfolio?: boolean
  gridHeight?: number | undefined
  viewSelector?: React.ReactNode | undefined
}

export interface NavigationLinkCellRendererProps {
  data: NavigationDto
}
export const StrategicInitiativeLinkCellRenderer = ({
  data,
}: NavigationLinkCellRendererProps) => {
  if (!data) return null
  return (
    <Link href={`/ppm/strategic-initiatives/${data.key}`}>{data.name}</Link>
  )
}

const StrategicInitiativesGrid: React.FC<StrategicInitiativesGridProps> = (
  props: StrategicInitiativesGridProps,
) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<StrategicInitiativeListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      {
        field: 'name',
        cellRenderer: StrategicInitiativeLinkCellRenderer,
        width: 300,
      },
      {
        field: 'status.name',
        headerName: 'Status',
        width: 125,
      },
      {
        field: 'portfolio.name',
        headerName: 'Portfolio',
        width: 200,
        hide: props.hidePortfolio,
        cellRenderer: (params) =>
          PortfolioLinkCellRenderer({ data: params.data.portfolio }),
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
        field: 'strategicInitiativeSponsors',
        headerName: 'Sponsors',
        valueGetter: (params) =>
          getSortedNames(params.data.strategicInitiativeSponsors),
      },
      {
        field: 'strategicInitiativeOwners',
        headerName: 'Owners',
        valueGetter: (params) =>
          getSortedNames(params.data.strategicInitiativeOwners),
      },
    ],
    [props.hidePortfolio],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <ModaGrid
      columnDefs={columnDefs}
      rowData={props.strategicInitiatives}
      loadData={refresh}
      loading={props.isLoading}
      toolbarActions={props.viewSelector}
      height={props.gridHeight}
      emptyMessage="No strategic initiatives found."
    />
  )
}

export default StrategicInitiativesGrid
