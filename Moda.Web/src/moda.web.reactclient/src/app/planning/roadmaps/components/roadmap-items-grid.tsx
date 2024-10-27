'use client'

import { ModaEmpty } from '@/src/app/components/common'
import { RowMenuCellRenderer } from '@/src/app/components/common/moda-grid-cell-renderers'
import useAuth from '@/src/app/components/contexts/auth'
import {
  RoadmapActivityListDto,
  RoadmapActivityNavigationDto,
  RoadmapItemListDto,
  RoadmapMilestoneListDto,
  RoadmapTimeboxListDto,
} from '@/src/services/moda-api'
import {
  ColorPicker,
  Flex,
  MenuProps,
  Table,
  TableColumnsType,
  Typography,
} from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { MessageInstance } from 'antd/es/message/interface'
import dayjs from 'dayjs'
import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import EditRoadmapActivityForm from './edit-roadmap-activity-form'
import DeleteRoadmapItemForm from './delete-roadmap-item-form'

const { Text } = Typography

export interface RoadmapItemsGridProps {
  roadmapId: string
  roadmapItemsData: RoadmapItemListDto[]
  roadmapItemsIsLoading: boolean
  refreshRoadmapItems: () => void
  messageApi: MessageInstance
  gridHeight?: number | undefined
  viewSelector?: React.ReactNode | undefined
  enableRowDrag?: boolean | undefined
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

interface RowMenuProps extends MenuProps {
  itemId: string
  canUpdateRoadmap: boolean
  onEditItemMenuClicked: (id: string) => void
  onDeleteItemMenuClicked: (id: string) => void
}

const getRowMenuItems = (props: RowMenuProps) => {
  if (
    !props.canUpdateRoadmap ||
    !props.itemId ||
    !props.onEditItemMenuClicked
  ) {
    return null
  }
  return [
    {
      key: 'editItem',
      label: 'Edit',
      onClick: () => props.onEditItemMenuClicked(props.itemId),
    },
    {
      key: 'deleteItem',
      label: 'Delete',
      onClick: () => props.onDeleteItemMenuClicked(props.itemId),
    },
  ] as ItemType[]
}

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

const RoadmapItemsGrid: React.FC<RoadmapItemsGridProps> = (
  props: RoadmapItemsGridProps,
) => {
  const [data, setData] = useState<RoadmapItemDataType[]>([])
  const [openUpdateRoadmapActivityForm, setOpenUpdateRoadmapActivityForm] =
    useState<boolean>(false)
  const [openDeleteRoadmapItemForm, setOpenDeleteRoadmapItemForm] =
    useState<boolean>(false)
  const [selectedItemId, setSelectedItemId] = useState<string | null>(null)
  const tblRef: Parameters<typeof Table>[0]['ref'] = useRef(null)

  const { hasPermissionClaim } = useAuth()
  const canManageRoadmapItems = hasPermissionClaim(
    'Permissions.Roadmaps.Update',
  )

  const onEditItemMenuClicked = useCallback((id: string) => {
    setSelectedItemId(id)
    setOpenUpdateRoadmapActivityForm(true)
  }, [])

  const onDeleteItemMenuClicked = useCallback((id: string) => {
    setSelectedItemId(id)
    setOpenDeleteRoadmapItemForm(true)
  }, [])

  const columnDefs = useMemo(() => {
    return [
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
        dataIndex: '',
        key: 'x',
        width: 25,
        render: (_: any, record: RoadmapItemDataType) => {
          const menuItems = getRowMenuItems({
            itemId: record.id,
            canUpdateRoadmap: canManageRoadmapItems,
            onEditItemMenuClicked: () => onEditItemMenuClicked(record.id),
            onDeleteItemMenuClicked: () => onDeleteItemMenuClicked(record.id),
          })

          return RowMenuCellRenderer({ menuItems })
        },
      },
      {
        key: 'type',
        dataIndex: 'type',
        title: 'Type',
        sorter: (a, b) => a.type.localeCompare(b.type),
        width: 100,
      },
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
    ] as TableColumnsType<RoadmapItemDataType>
  }, [canManageRoadmapItems, onDeleteItemMenuClicked, onEditItemMenuClicked])

  useEffect(() => {
    if (props.roadmapItemsIsLoading) return

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
  }, [props.roadmapItemsData, props.roadmapItemsIsLoading])

  const onUpdateRoadmapActivityFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRoadmapActivityForm(false)
    setSelectedItemId(null)
    if (wasSaved) {
      props.refreshRoadmapItems()
    }
  }

  const onDeleteRoadmapItemFormClosed = (wasSaved: boolean) => {
    setOpenDeleteRoadmapItemForm(false)
    setSelectedItemId(null)
    if (wasSaved) {
      props.refreshRoadmapItems()
    }
  }

  return (
    <>
      <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
        {props.viewSelector}
      </Flex>
      <Table<RoadmapItemDataType>
        ref={tblRef}
        columns={columnDefs}
        dataSource={data}
        size="small"
        pagination={false}
        locale={{ emptyText: <ModaEmpty message="No Roadmap Items" /> }}
      />
      {openUpdateRoadmapActivityForm && (
        <EditRoadmapActivityForm
          showForm={openUpdateRoadmapActivityForm}
          activityId={selectedItemId}
          roadmapId={props.roadmapId}
          onFormComplete={() => onUpdateRoadmapActivityFormClosed(true)}
          onFormCancel={() => onUpdateRoadmapActivityFormClosed(false)}
          messageApi={props.messageApi}
        />
      )}
      {openDeleteRoadmapItemForm && (
        <DeleteRoadmapItemForm
          roadmapId={props.roadmapId}
          roadmapItemId={selectedItemId}
          showForm={openDeleteRoadmapItemForm}
          onFormComplete={() => onDeleteRoadmapItemFormClosed(true)}
          onFormCancel={() => onDeleteRoadmapItemFormClosed(false)}
          messageApi={props.messageApi}
        />
      )}
    </>
  )
}

export default RoadmapItemsGrid
