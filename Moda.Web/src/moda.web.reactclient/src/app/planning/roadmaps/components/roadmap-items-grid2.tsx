'use client'

import { ModaEmpty } from '@/src/app/components/common'
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
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'

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
  start: string // TODO: Change to Date
  end?: string // TODO: Change to Date
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
    sorter: (a, b) => a.name.localeCompare(b.name),
    width: 350,
  },
  {
    key: 'type',
    dataIndex: 'type',
    title: 'Type',
    sorter: (a, b) => a.type.localeCompare(b.type),
    width: 100,
  },
  // {
  //   key: 'parent',
  //   dataIndex: 'parent',
  //   title: 'Parent',
  //   render: (value) => value?.name && <Text>{value.name}</Text>,
  // },
  {
    key: 'start',
    dataIndex: 'start',
    title: 'Start',
    sorter: (a, b) => dayjs(a.start).unix() - dayjs(b.start).unix(),
    width: 100,
  },
  {
    key: 'end',
    dataIndex: 'end',
    title: 'End',
    sorter: (a, b) => dayjs(a.end).unix() - dayjs(b.end).unix(),
    width: 100,
  },
  {
    key: 'color',
    dataIndex: 'color',
    title: 'Color',
    sorter: (a, b) => a.color.localeCompare(b.color),
    render: (value) =>
      value && (
        <ColorPicker defaultValue={value} size="small" showText disabled />
      ),
    width: 100,
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
      'start' in item && item.start // activity and timebox
        ? dayjs(item.start).format('M/D/YYYY')
        : 'date' in item && item.date // milestone
          ? dayjs(item.date).format('M/D/YYYY')
          : undefined,
    end: 'end' in item && item.end ? dayjs(item.start).format('M/D/YYYY') : '',
    color: item.color,
  }
}

const RoadmapItemsGrid2: React.FC<RoadmapItemsGrid2Props> = (
  props: RoadmapItemsGrid2Props,
) => {
  const [data, setData] = useState<RoadmapItemDataType[]>([])
  const tblRef: Parameters<typeof Table>[0]['ref'] = useRef(null)

  useEffect(() => {
    if (props.roadmapItemsLoading) return

    const roadmapItemsData: RoadmapItemDataType[] = props.roadmapItemsData.map(
      (item: RoadmapItemUnion) => {
        const mapItemWithChildren = (
          item: RoadmapItemUnion,
        ): RoadmapItemDataType => {
          const roadmapItem = MapRoadmapItem(item)
          if ('children' in item && item.children.length > 0) {
            roadmapItem.children = item.children.map((child) =>
              mapItemWithChildren(child),
            )
          }
          return roadmapItem
        }

        return mapItemWithChildren(item)
      },
    )

    setData(roadmapItemsData)
  }, [props.roadmapItemsData, props.roadmapItemsLoading])

  return (
    <>
      <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
        {props.viewSelector}
      </Flex>
      <Table<RoadmapItemDataType>
        virtual
        ref={tblRef}
        //scroll={{ x: 2000, y: 400 }}
        columns={columnDefs}
        dataSource={data}
        size="small"
        pagination={false}
        locale={{ emptyText: <ModaEmpty message="No Roadmap Items" /> }}
      />
    </>
  )
}

export default RoadmapItemsGrid2
