import {
  ColDef,
  GetRowIdParams,
  GridReadyEvent,
  ICellRendererParams,
  RowDragEndEvent,
  RowSelectionOptions,
} from 'ag-grid-community'
import { DeleteOutlined } from '@ant-design/icons'
import useTheme from '@/src/components/contexts/theme'
import React, { ReactNode, useCallback, useRef } from 'react'
import { AgGridReact } from 'ag-grid-react'
import { Flex } from 'antd'
import { AgGridReactProps } from 'ag-grid-react/dist/types/src/shared/interfaces'

export const asDraggableColDefs = <TData extends object>(
  colDefs: ColDef<TData>[],
): ColDef<TData>[] => [
  {
    rowDrag: true,
    maxWidth: 40,
    filter: false,
    sortable: false,
    suppressHeaderMenuButton: true,
    rowDragText: (params, dragItemCount) => {
      if (dragItemCount > 1) {
        return dragItemCount + ' items'
      }
      return params.rowNode!.data.key
    },
  },
  ...colDefs,
]

export const asDeletableColDefs = <TData extends object>(
  colDefs: ColDef<TData>[],
  onDelete: (item: TData) => void,
): ColDef<TData>[] => [
  {
    maxWidth: 50,
    suppressHeaderMenuButton: true,
    cellStyle: { textAlign: 'center' },
    filter: false,
    sortable: false,
    cellRenderer: (props: ICellRendererParams<TData>) => (
      <DeleteOutlined
        onClick={() => {
          onDelete(props.data)
        }}
      />
    ),
  },
  ...colDefs,
]

interface GridTransferProps<TData extends object> {
  leftGridData?: TData[]
  rightGridData?: TData[]
  leftColumnDef: ColDef<TData>[]
  rightColumnDef: ColDef<TData>[]
  getRowId: (params: GetRowIdParams<TData>) => string
  onDragStop?: (items: TData[]) => void
  leftGridRef?: React.MutableRefObject<AgGridReact<TData>>
  rightGridRef?: React.MutableRefObject<AgGridReact<TData>>
  GridProps?: AgGridReactProps<TData>
  leftGridRowSelection?: RowSelectionOptions<TData> | 'single' | 'multiple'
  rightGridRowSelection?: RowSelectionOptions<TData> | 'single' | 'multiple'
}

export const AgGridTransfer = <TData extends object>(
  props: GridTransferProps<TData>,
): ReactNode => {
  const {
    leftGridData,
    rightGridData,
    leftColumnDef,
    rightColumnDef,
    getRowId,
    onDragStop,
    GridProps = {},
    leftGridRowSelection = {
      mode: 'multiRow',
      checkboxes: true,
      headerCheckbox: true,
      enableClickSelection: false,
    } as RowSelectionOptions<TData>,
    rightGridRowSelection,
  } = props

  const { agGridTheme } = useTheme()

  const localLeftGridRef = useRef<AgGridReact<TData>>(null)
  const localRightGridRef = useRef<AgGridReact<TData>>(null)

  const leftGridRef = props.leftGridRef ?? localLeftGridRef
  const rightGridRef = props.rightGridRef ?? localRightGridRef

  const onDragStopInternal = useCallback(
    (params: RowDragEndEvent<TData>) => {
      if (onDragStop) {
        onDragStop(params.nodes.map((node) => node.data))
      } else
        leftGridRef.current.api?.setNodesSelected({
          nodes: params.nodes,
          newValue: false,
        })
    },
    [leftGridRef, onDragStop],
  )

  const onGridReady = useCallback(
    (event: GridReadyEvent<TData>) => {
      if (leftGridRef.current === null || rightGridRef.current === null) return

      const dropZoneParams = rightGridRef.current.api.getRowDropZoneParams({
        onDragStop: onDragStopInternal,
      })

      leftGridRef.current.api.removeRowDropZone(dropZoneParams)
      leftGridRef.current.api.addRowDropZone(dropZoneParams)
    },
    [leftGridRef, onDragStopInternal, rightGridRef],
  )

  const getGrid = useCallback(
    (isLeft: boolean) => (
      <div style={{ minHeight: 400, width: '100%' }}>
        <AgGridReact
          ref={isLeft ? leftGridRef : rightGridRef}
          getRowId={getRowId}
          rowDragManaged={true}
          rowSelection={isLeft ? leftGridRowSelection : rightGridRowSelection}
          theme={agGridTheme}
          rowDragMultiRow={isLeft}
          suppressMoveWhenRowDragging={isLeft}
          rowData={isLeft ? leftGridData : rightGridData}
          columnDefs={isLeft ? leftColumnDef : rightColumnDef}
          onGridReady={onGridReady}
          context={
            isLeft
              ? { rightGridRef: rightGridRef }
              : { leftGridRef: leftGridRef }
          }
          {...GridProps}
          defaultColDef={{
            ...(GridProps?.defaultColDef ?? {}),
          }}
        />
      </div>
    ),
    [
      agGridTheme,
      leftGridRef,
      rightGridRef,
      getRowId,
      leftGridRowSelection,
      rightGridRowSelection,
      leftGridData,
      rightGridData,
      leftColumnDef,
      rightColumnDef,
      GridProps,
      onGridReady,
    ],
  )

  return (
    <Flex gap="middle" style={{ width: '100%' }}>
      {getGrid(true)}
      {getGrid(false)}
    </Flex>
  )
}
