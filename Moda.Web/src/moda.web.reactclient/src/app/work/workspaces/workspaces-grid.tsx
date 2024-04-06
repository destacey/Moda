'use client'

import { WorkspaceListDto } from '@/src/services/moda-api'
import { ColDef } from 'ag-grid-community'
import { Card, Flex, List } from 'antd'
import { ReactElement, useCallback, useMemo } from 'react'
import { ModaGrid } from '../../components/common'
import { WorkspaceLinkCellRenderer } from '../../components/common/moda-grid-cell-renderers'

export interface WorkspacesGridProps {
  workspaces: WorkspaceListDto[]
  viewSelector: ReactElement
  isLoading: boolean
  refetch: () => void
}

const WorkspacesGrid = (props: WorkspacesGridProps) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<WorkspaceListDto>[]>(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: WorkspaceLinkCellRenderer },
      { field: 'description', width: 300 },
      { field: 'ownership.name', headerName: 'Ownership' },
      { field: 'isActive' },
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
        rowData={props.workspaces}
        loadData={refresh}
        isDataLoading={props.isLoading}
        toolbarActions={props.viewSelector}
      />
    </>
  )
}

export default WorkspacesGrid
