'use client'

import { ModaGrid } from '@/src/components/common'
import { PokerSessionListDto } from '@/src/services/moda-api'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import Link from 'next/link'
import { FC, useMemo } from 'react'

export interface PokerSessionsGridProps {
  sessions: PokerSessionListDto[]
  isLoading: boolean
  refetch: () => void
}

const sessionLinkCellRenderer = (
  params: ICellRendererParams<PokerSessionListDto>,
) => (
  <Link href={`/planning/poker-sessions/${params.data.key}`}>
    {params.value}
  </Link>
)

const PokerSessionsGrid: FC<PokerSessionsGridProps> = ({
  sessions = [],
  isLoading,
  refetch,
}) => {
  const columnDefs = useMemo<ColDef<PokerSessionListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', width: 300, cellRenderer: sessionLinkCellRenderer },
      { field: 'status', width: 125 },
      { field: 'facilitator.name', headerName: 'Facilitator', width: 200 },
      { field: 'roundCount', headerName: 'Rounds', width: 110 },
    ],
    [],
  )

  return (
    <ModaGrid
      columnDefs={columnDefs}
      rowData={sessions}
      loadData={refetch}
      loading={isLoading}
      height={650}
      emptyMessage="No poker sessions found."
    />
  )
}

export default PokerSessionsGrid
