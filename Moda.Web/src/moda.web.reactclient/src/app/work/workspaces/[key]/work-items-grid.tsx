'use client'

import { ModaGrid } from '@/src/app/components/common'
import { WorkItemListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import Link from 'next/link'
import { useCallback, useMemo } from 'react'

export interface WorkItemsGridProps {
  workItems: WorkItemListDto[]
  isLoading: boolean
  refetch: () => void
}

const AssignedToLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/organizations/employees/${data.assignedTo?.key}`}>
      {value}
    </Link>
  )
}

const WorkItemsGrid = (props: WorkItemsGridProps) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<WorkItemListDto>[]>(
    () => [
      { field: 'key' },
      { field: 'title', width: 400 },
      { field: 'type', width: 125 },
      { field: 'status', width: 125 },
      {
        field: 'assignedTo.name',
        headerName: 'Assigned To',
        cellRenderer: AssignedToLinkCellRenderer,
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
        rowData={props.workItems}
        loadData={refresh}
        isDataLoading={props.isLoading}
      />
    </>
  )
}

export default WorkItemsGrid
