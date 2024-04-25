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

const workItemKeyComparator = (key1, key2) => {
  const [str1, num1] = key1.split('-')
  const [str2, num2] = key2.split('-')

  if (str1 < str2) return -1
  if (str1 > str2) return 1

  return parseInt(num1) - parseInt(num2)
}

const WorkItemsGrid = (props: WorkItemsGridProps) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<WorkItemListDto>[]>(
    () => [
      { field: 'key', comparator: workItemKeyComparator },
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
        height={550}
        columnDefs={columnDefs}
        rowData={props.workItems}
        loadData={refresh}
        isDataLoading={props.isLoading}
      />
    </>
  )
}

export default WorkItemsGrid