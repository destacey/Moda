import { ColDef, GetRowIdParams, GridReadyEvent, ICellRendererParams, RowDragEndEvent } from 'ag-grid-community'
import { DeleteFilled } from '@ant-design/icons'
import useTheme from '@/src/app/components/contexts/theme'
import React, { useCallback, useRef, useState } from 'react'
import { AgGridReact } from 'ag-grid-react'
import { Flex } from 'antd'

export const asDraggableColDefs = <TData extends object>(colDefs: ColDef<TData>[]): ColDef<TData>[] => [
  {
    rowDrag: true,
    maxWidth: 50,
    suppressHeaderMenuButton: true,
    rowDragText: (params, dragItemCount) => {
      if (dragItemCount > 1) {
        return dragItemCount + ' items'
      }
      return params.rowNode!.data.key
    }
  },
  ...colDefs
]

export const asDeletableColDefs = <TData extends object>(colDefs: ColDef<TData>[]): ColDef<TData>[] => [
  ...colDefs,
  {
    maxWidth: 50,
    suppressHeaderMenuButton: true,
    cellStyle: { textAlign: 'center' },
    cellRenderer: (props: ICellRendererParams<TData>) => (
      <DeleteFilled
        onClick={() => {
          props.api.applyTransaction({ remove: [props.data] })
        }}
      />
    )
  }
]

interface GridTransferProps<TData extends object> {
  leftGridData?: TData[];
  rightGridData?: TData[];
  leftColumnDef: ColDef<TData>[];
  rightColumnDef: ColDef<TData>[];
  getRowId: (params: GetRowIdParams<TData>) => string;
  removeRowFromSource?: boolean;
  leftGridRef?: React.MutableRefObject<AgGridReact<TData>>;
  rightGridRef?: React.MutableRefObject<AgGridReact<TData>>;
}

const defaultColDef: ColDef = {
  flex: 1,
  minWidth: 50,
  filter: false
}

export const AgGridTransfer = <TData extends object>(props: GridTransferProps<TData>): React.ReactNode => {
  const { agGridTheme } = useTheme()

  const localLeftGridRef = useRef<AgGridReact<TData>>(null)
  const localRightGridRef = useRef<AgGridReact<TData>>(null)

  const leftGridRef = props.leftGridRef ?? localLeftGridRef
  const rightGridRef = props.rightGridRef ?? localRightGridRef

  const onDragStop = useCallback((params: RowDragEndEvent) => {
    if (props.removeRowFromSource)
      leftGridRef.current.api?.applyTransaction({ remove: [params.node.data] })
    else
      leftGridRef.current.api?.setNodesSelected({ nodes: params.nodes, newValue: false })
  }, [leftGridRef, props.removeRowFromSource])

  const onGridReady = useCallback((event: GridReadyEvent<TData>) => {

    if (leftGridRef.current === null || rightGridRef.current === null) return

    const dropZoneParams = rightGridRef.current.api.getRowDropZoneParams({ onDragStop })

    leftGridRef.current.api.removeRowDropZone(dropZoneParams)
    leftGridRef.current.api.addRowDropZone(dropZoneParams)
  }, [leftGridRef, onDragStop, rightGridRef])


  const getGrid = useCallback((isLeft: boolean) => (
    <div className={agGridTheme} style={{ minHeight: 250, width: '100%' }}>
      <AgGridReact
        ref={isLeft ? leftGridRef : rightGridRef}
        defaultColDef={defaultColDef}
        getRowId={props.getRowId}
        rowDragManaged={true}
        rowSelection={isLeft ? 'multiple' : undefined}
        rowDragMultiRow={isLeft}
        suppressRowClickSelection={isLeft}
        suppressMoveWhenRowDragging={isLeft}

        rowData={isLeft ? props.leftGridData : props.rightGridData}
        columnDefs={isLeft ? props.leftColumnDef : props.rightColumnDef}
        onGridReady={onGridReady}
      />
    </div>
  ), [agGridTheme, leftGridRef, rightGridRef, props.getRowId, props.leftGridData, props.rightGridData, props.leftColumnDef, props.rightColumnDef, onGridReady])


  return (
    <Flex gap="middle" style={{ width: '100%' }}>
      {getGrid(true)}
      {getGrid(false)}
    </Flex>
  )

}