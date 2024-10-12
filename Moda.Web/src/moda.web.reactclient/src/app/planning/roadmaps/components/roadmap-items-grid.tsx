'use client'

import { ModaGrid } from '@/src/app/components/common'
import { RoadmapItemDto } from '@/src/services/moda-api'
import { ColDef, RowDragEndEvent } from 'ag-grid-community'
import { ColorPicker } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'

export interface RoadmapItemsGridProps {
  roadmapItemsData: RoadmapItemDto[]
  roadmapItemsLoading: boolean
  isRoadmapItemsLoading: () => void
  messageApi: MessageInstance
  gridHeight?: number | undefined
  viewSelector?: React.ReactNode | undefined
  enableRowDrag?: boolean | undefined
  parentRoadmapId?: string | undefined
}

const RoadmapItemsGrid: React.FC<RoadmapItemsGridProps> = (
  props: RoadmapItemsGridProps,
) => {
  // const [enableRowDrag, setEnableRowDrag] = useState(
  //   props.enableRowDrag ?? false,
  // )

  //const [updateChildOrder, { error: mutationError }] = useUpdateChildOrderMutation()

  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo<ColDef<RoadmapItemDto>[]>(
    () => [
      // rowDrag is typically set on the first column
      // { field: 'id', width: enableRowDrag ? 110 : 90, rowDrag: enableRowDrag },
      { field: 'name', width: 350 },
      { field: 'type.name' },
      { field: 'parent.name' },
      // {
      //   field: 'start',
      //   width: 125,
      //   valueGetter: (params) => dayjs(params.data.start).format('M/D/YYYY'),
      // },
      // {
      //   field: 'end',
      //   width: 125,
      //   valueGetter: (params) => dayjs(params.data.end).format('M/D/YYYY'),
      // },
      {
        field: 'color',
        width: 125,
        cellRenderer: (params) =>
          params.value && (
            <ColorPicker
              defaultValue={params.value}
              size="small"
              showText
              disabled
            />
          ),
      },
    ],
    [],
  )

  // const onRowDragEnd = useCallback(
  //   async (e: RowDragEndEvent) => {
  //     updateChildOrder({
  //       roadmapId: props.parentRoadmapId,
  //       childRoadmapId: e.api.getRowNode(e.node.id).data.id,
  //       order: e.node.rowIndex + 1,
  //     } as UpdateRoadmapItemOrderRequest)
  //       .unwrap()
  //       .then(() => {
  //         props.messageApi.success(`Roadmap activity order updated successfully.`)
  //       })
  //       .catch((error) => {
  //         props.messageApi.success(`Error updating roadmap activity order.`)
  //         console.error('Error updating roadmap activity order:', error)
  //       })
  //   },
  //   [props, updateChildOrder],
  // )

  return (
    <ModaGrid
      height={props.gridHeight ?? 650}
      columnDefs={columnDefs}
      rowData={props.roadmapItemsData}
      loadData={props.isRoadmapItemsLoading}
      loading={props.roadmapItemsLoading}
      toolbarActions={props.viewSelector}
      rowDragManaged={true}
      //onRowDragEnd={onRowDragEnd}
    />
  )
}

export default RoadmapItemsGrid
