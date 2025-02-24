'use client'

import { ModaGrid } from '@/src/components/common'
import { PortfolioLinkCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import { ProjectPortfolioListDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { ColDef } from 'ag-grid-community'
import { MessageInstance } from 'antd/es/message/interface'
import { ReactElement, useCallback, useMemo } from 'react'

export interface PortfoliosGridProps {
  portfolios: ProjectPortfolioListDto[]
  viewSelector: ReactElement
  isLoading: boolean
  refetch: () => void
  messageApi: MessageInstance
}

const PortfoliosGrid: React.FC<PortfoliosGridProps> = (
  props: PortfoliosGridProps,
) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<ProjectPortfolioListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: PortfolioLinkCellRenderer },
      { field: 'status.name', headerName: 'Status' },
      {
        field: 'portfolioSponsors',
        headerName: 'Sponsors',
        valueGetter: (params) => getSortedNames(params.data.portfolioSponsors),
      },
      {
        field: 'portfolioOwners',
        headerName: 'Owners',
        valueGetter: (params) => getSortedNames(params.data.portfolioOwners),
      },
      {
        field: 'portfolioManagers',
        headerName: 'Managers',
        valueGetter: (params) => getSortedNames(params.data.portfolioManagers),
      },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  return (
    <>
      <ModaGrid
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
