'use client'

import { WaydGrid } from '@/src/components/common'
import { RowMenuCellRenderer } from '@/src/components/common/wayd-grid-cell-renderers'
import { PokerSessionListDto } from '@/src/services/wayd-api'
import { ColDef, ICellRendererParams } from 'ag-grid-community'
import { ItemType } from 'antd/es/menu/interface'
import Link from 'next/link'
import { FC, useMemo } from 'react'

export interface PokerSessionsGridProps {
  sessions: PokerSessionListDto[]
  isLoading: boolean
  refetch: () => void
  canUpdate: boolean
  canDelete: boolean
  gridControlMenuItems?: ItemType[]
  onEditClicked: (session: PokerSessionListDto) => void
  onCompleteClicked: (session: PokerSessionListDto) => void
  onDeleteClicked: (session: PokerSessionListDto) => void
}

interface RowMenuProps {
  session: PokerSessionListDto
  canUpdate: boolean
  canDelete: boolean
  onEditClicked: (session: PokerSessionListDto) => void
  onCompleteClicked: (session: PokerSessionListDto) => void
  onDeleteClicked: (session: PokerSessionListDto) => void
}

const getRowMenuItems = (props: RowMenuProps): ItemType[] => {
  if (!props.session) return []

  const items: ItemType[] = []

  if (props.canUpdate && props.session.status === 'Active') {
    items.push({
      key: 'edit',
      label: 'Edit',
      onClick: () => props.onEditClicked(props.session),
    })
    items.push({
      key: 'complete',
      label: 'Complete',
      onClick: () => props.onCompleteClicked(props.session),
    })
  }

  if (props.canDelete) {
    items.push({
      key: 'delete',
      label: 'Delete',
      danger: true,
      onClick: () => props.onDeleteClicked(props.session),
    })
  }

  return items
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
  canUpdate,
  canDelete,
  gridControlMenuItems,
  onEditClicked,
  onCompleteClicked,
  onDeleteClicked,
}) => {
  const showRowMenu = canUpdate || canDelete

  const columnDefs = useMemo<ColDef<PokerSessionListDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        resizable: false,
        hide: !showRowMenu,
        suppressHeaderMenuButton: true,
        cellRenderer: (params: ICellRendererParams<PokerSessionListDto>) => {
          const menuItems = getRowMenuItems({
            session: params.data,
            canUpdate,
            canDelete,
            onEditClicked,
            onCompleteClicked,
            onDeleteClicked,
          })
          if (menuItems.length === 0) return null
          return RowMenuCellRenderer({ ...params, menuItems })
        },
      },
      { field: 'key', width: 90 },
      { field: 'name', width: 300, cellRenderer: sessionLinkCellRenderer },
      { field: 'status', width: 125 },
      { field: 'facilitator.name', headerName: 'Facilitator', width: 200 },
      { field: 'roundCount', headerName: 'Rounds', width: 110 },
    ],
    [
      showRowMenu,
      canUpdate,
      canDelete,
      onEditClicked,
      onCompleteClicked,
      onDeleteClicked,
    ],
  )

  return (
    <WaydGrid
      columnDefs={columnDefs}
      gridControlMenuItems={gridControlMenuItems}
      rowData={sessions}
      loadData={refetch}
      loading={isLoading}
      height={650}
      emptyMessage="No poker sessions found."
    />
  )
}

export default PokerSessionsGrid
