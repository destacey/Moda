'use client'

import { ModaGrid } from '@/src/components/common'
import { PortfolioLinkCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import { ProjectPortfolioListDto } from '@/src/services/moda-api'
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
      // { field: 'description', width: 300 },
      { field: 'status.name', headerName: 'Status' },
      {
        field: 'portfolioSponsors',
        headerName: 'Sponsors',
        valueGetter: (params) =>
          params.data.portfolioSponsors
            .slice()
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((m) => m.name)
            .join(', '),
      },
      {
        field: 'portfolioOwners',
        headerName: 'Owners',
        valueGetter: (params) =>
          params.data.portfolioOwners
            .slice()
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((m) => m.name)
            .join(', '),
      },
      {
        field: 'portfolioManagers',
        headerName: 'Managers',
        valueGetter: (params) =>
          params.data.portfolioManagers
            .slice()
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((m) => m.name)
            .join(', '),
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
      />
    </>
  )
}

export default PortfoliosGrid
