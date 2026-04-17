'use client'

import { ModaGrid } from '@/src/components/common'
import { WorkspaceLinkCellRenderer } from '@/src/components/common/moda-grid-cell-renderers'
import { WorkspaceListDto } from '@/src/services/wayd-api'
import { ColDef } from 'ag-grid-community'
import { ReactElement, useMemo } from 'react'

export interface WorkspacesGridProps {
  workspaces: WorkspaceListDto[]
  viewSelector: ReactElement
  isLoading: boolean
  refetch: () => void
}

const WorkspacesGrid = (props: WorkspacesGridProps) => {
  const { refetch } = props

  const columnDefs = useMemo<ColDef<WorkspaceListDto>[]>(() => [
    { field: 'key', width: 90 },
    { field: 'name', cellRenderer: WorkspaceLinkCellRenderer },
    { field: 'description', width: 300 },
    { field: 'ownership.name', headerName: 'Ownership' },
    { field: 'isActive' },
  ], [])

  const refresh = async () => {
    refetch()
  }

  return (
    <>
      <ModaGrid
        columnDefs={columnDefs}
        rowData={props.workspaces}
        loadData={refresh}
        loading={props.isLoading}
        toolbarActions={props.viewSelector}
      />
    </>
  )
}

export default WorkspacesGrid
