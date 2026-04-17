'use client'

import { WaydGrid } from '@/src/components/common'
import { PortfolioLinkCellRenderer } from '@/src/components/common/wayd-grid-cell-renderers'
import { ProjectPortfolioListDto } from '@/src/services/wayd-api'
import { getSortedNames } from '@/src/utils'
import { ColDef } from 'ag-grid-community'
import { ReactElement, useMemo } from 'react'

export interface PortfoliosGridProps {
  portfolios: ProjectPortfolioListDto[]
  viewSelector: ReactElement
  isLoading: boolean
  refetch: () => void
}

const PortfoliosGrid: React.FC<PortfoliosGridProps> = (
  props: PortfoliosGridProps,
) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<ProjectPortfolioListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      {
        field: 'name',
        cellRenderer: PortfolioLinkCellRenderer,
        width: 200,
        initialSort: 'asc',
      },
      { field: 'status.name', headerName: 'Status' },
      {
        field: 'portfolioManagers',
        headerName: 'PMs',
        valueGetter: (params) => getSortedNames(params.data.portfolioManagers),
      },
      {
        field: 'portfolioOwners',
        headerName: 'Owners',
        valueGetter: (params) => getSortedNames(params.data.portfolioOwners),
      },
      {
        field: 'portfolioSponsors',
        headerName: 'Sponsors',
        valueGetter: (params) => getSortedNames(params.data.portfolioSponsors),
      },
    ],
    [],
  )

  const refresh = async () => {
    refetch()
  }

  return (
    <>
      <WaydGrid
        columnDefs={columnDefs}
        rowData={props.portfolios}
        loadData={refresh}
        loading={props.isLoading}
        toolbarActions={props.viewSelector}
        emptyMessage="No portolios found."
      />
    </>
  )
}

export default PortfoliosGrid
