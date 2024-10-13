'use client'

import {
  RoadmapActivityListDto,
  RoadmapActivityNavigationDto,
  RoadmapItemListDto,
  RoadmapMilestoneListDto,
  RoadmapTimeboxListDto,
} from '@/src/services/moda-api'
import {
  ColorPicker,
  Divider,
  Flex,
  Space,
  Table,
  TableColumnsType,
  TableProps,
  Typography,
} from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import dayjs from 'dayjs'
import { useCallback, useEffect, useMemo, useState } from 'react'

const { Text } = Typography

export interface RoadmapItemsGrid2Props {
  roadmapItemsData: RoadmapItemListDto[]
  roadmapItemsLoading: boolean
  isRoadmapItemsLoading: () => void
  messageApi: MessageInstance
  gridHeight?: number | undefined
  viewSelector?: React.ReactNode | undefined
  enableRowDrag?: boolean | undefined
  parentRoadmapId?: string | undefined
}

type RoadmapItemUnion =
  | RoadmapItemListDto
  | RoadmapActivityListDto
  | RoadmapMilestoneListDto
  | RoadmapTimeboxListDto

interface RoadmapItemDataType {
  id: string
  key: React.ReactNode
  name: string
  type: string
  parent?: RoadmapActivityNavigationDto
  start: string
  end?: string
  color?: string
  children?: RoadmapItemDataType[]
}

const columnDefs: TableColumnsType<RoadmapItemDataType> = [
  {
    key: 'id',
    dataIndex: 'id',
    title: 'Id',
    hidden: true,
  },
  {
    key: 'name',
    dataIndex: 'name',
    title: 'Name',
    width: 350,
  },
  {
    key: 'type',
    dataIndex: 'type',
    title: 'Type',
  },
  {
    key: 'parent',
    dataIndex: 'parent',
    title: 'Parent',
    render: (value) => value?.name && <Text>{value.name}</Text>,
  },
  {
    key: 'start',
    dataIndex: 'start',
    title: 'Start',
  },
  {
    key: 'end',
    dataIndex: 'end',
    title: 'End',
  },
  {
    key: 'color',
    dataIndex: 'color',
    title: 'Color',
    render: (value) =>
      value && (
        <ColorPicker defaultValue={value} size="small" showText disabled />
      ),
  },
]

const MapRoadmapItem = (item: RoadmapItemUnion): RoadmapItemDataType => {
  return {
    id: item.id,
    key: item.id,
    name: item.name,
    type: item.type.name,
    parent: item.parent,
    start:
      'start' in item && item.start ? dayjs(item.start).format('M/D/YYYY') : '',
    end: 'end' in item && item.end ? dayjs(item.start).format('M/D/YYYY') : '',
    color: item.color,
  }
}

const RoadmapItemsGrid2: React.FC<RoadmapItemsGrid2Props> = (
  props: RoadmapItemsGrid2Props,
) => {
  const [data, setData] = useState<RoadmapItemDataType[]>([])

  useEffect(() => {
    if (props.roadmapItemsLoading) return

    const roadmapItemsData: RoadmapItemDataType[] = props.roadmapItemsData.map(
      (item: RoadmapItemUnion) => {
        const roadmapItem = MapRoadmapItem(item)
        if ('children' in item && item.children.length > 0) {
          roadmapItem.children = item.children.map((child) =>
            MapRoadmapItem(child),
          )
        }
        return roadmapItem
      },
    )

    setData(roadmapItemsData)
  }, [props.roadmapItemsData, props.roadmapItemsLoading])

  return (
    <>
      <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
        {props.viewSelector}
      </Flex>
      <Table<RoadmapItemDataType> columns={columnDefs} dataSource={data} />
    </>
  )
}

export default RoadmapItemsGrid2
