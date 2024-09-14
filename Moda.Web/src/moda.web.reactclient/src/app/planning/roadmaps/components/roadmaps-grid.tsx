'use client'

import { ModaGrid } from '@/src/app/components/common'
import {
  RoadmapListDto,
  UpdateRoadmapChildOrderRequest,
} from '@/src/services/moda-api'
import { useUpdateChildOrderMutation } from '@/src/store/features/planning/roadmaps-api'
import { ColDef, RowDragEndEvent } from 'ag-grid-community'
import { MessageInstance } from 'antd/es/message/interface'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'

export interface RoadmapsGridProps {
  roadmapsData: RoadmapListDto[]
  roadmapsLoading: boolean
  refreshRoadmaps: () => void
  messageApi: MessageInstance
  gridHeight?: number | undefined
  viewSelector?: React.ReactNode | undefined
  enableRowDrag?: boolean | undefined
  parentRoadmapId?: string | undefined
}

const RoadmapCellRenderer = ({ value, data }) => {
  return <Link href={`/planning/roadmaps/${data.key}`}>{value}</Link>
}

const RoadmapsGrid: React.FC<RoadmapsGridProps> = (
  props: RoadmapsGridProps,
) => {
  const [enableRowDrag, setEnableRowDrag] = useState(
    props.enableRowDrag ?? false,
  )

  const [updateChildOrder, { error: mutationError }] =
    useUpdateChildOrderMutation()

  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo<ColDef<RoadmapListDto>[]>(
    () => [
      // rowDrag is typically set on the first column
      { field: 'key', width: enableRowDrag ? 110 : 90, rowDrag: enableRowDrag },
      { field: 'name', width: 350, cellRenderer: RoadmapCellRenderer },
      {
        field: 'start',
        width: 125,
        valueGetter: (params) => dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        width: 125,
        valueGetter: (params) => dayjs(params.data.end).format('M/D/YYYY'),
      },
      {
        field: 'roadmapManagers',
        // conver the list to a string of names
        valueGetter: (params) =>
          params.data.roadmapManagers
            .slice()
            .sort((a, b) => a.name.localeCompare(b.name))
            .map((m) => m.name)
            .join(', '),
      },
      {
        field: 'visibility.name',
        headerName: 'Visibility',
        width: 125,
      },
    ],
    [enableRowDrag],
  )

  const onRowDragEnd = useCallback(
    async (e: RowDragEndEvent) => {
      updateChildOrder({
        roadmapId: props.parentRoadmapId,
        childRoadmapId: e.api.getRowNode(e.node.id).data.id,
        order: e.node.rowIndex + 1,
      } as UpdateRoadmapChildOrderRequest)
        .unwrap()
        .then(() => {
          props.messageApi.success(`Roadmap order updated successfully.`)
        })
        .catch((error) => {
          props.messageApi.success(`Error updating roadmap order.`)
          console.error('Error updating roadmap order:', error)
        })
    },
    [props, updateChildOrder],
  )

  return (
    <ModaGrid
      height={props.gridHeight ?? 650}
      columnDefs={columnDefs}
      rowData={props.roadmapsData}
      loadData={props.refreshRoadmaps}
      loading={props.roadmapsLoading}
      toolbarActions={props.viewSelector}
      rowDragManaged={true}
      onRowDragEnd={onRowDragEnd}
    />
  )
}

export default RoadmapsGrid
