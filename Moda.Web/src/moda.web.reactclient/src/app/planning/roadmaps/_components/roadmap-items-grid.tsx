'use client'

import {
  RoadmapActivityListDto,
  RoadmapItemListDto,
  RoadmapMilestoneListDto,
  RoadmapTimeboxListDto,
} from '@/src/services/moda-api'
import { TreeGrid, type TreeNode, type FilterOption } from '@/src/components/common/tree-grid'
import { FC, ReactNode, useCallback, useMemo, useState } from 'react'
import EditRoadmapActivityForm from './edit-roadmap-activity-form'
import DeleteRoadmapItemForm from './delete-roadmap-item-form'
import EditRoadmapTimeboxForm from './edit-roadmap-timebox-form'
import { getRoadmapItemsGridColumns } from './roadmap-items-grid.columns'

export interface RoadmapItemTreeNode extends TreeNode {
  id: string
  children: RoadmapItemTreeNode[]
  parentId?: string | null
  name: string
  type: string
  start?: Date | null
  end?: Date | null
  date?: Date | null
  color?: string | null
}

export interface RoadmapItemsGridProps {
  roadmapId: string
  roadmapItemsData: RoadmapItemListDto[]
  roadmapItemsIsLoading: boolean
  refreshRoadmapItems: () => void
  viewSelector?: ReactNode | undefined
  openRoadmapItemDrawer: (itemId: string) => void
  isRoadmapManager: boolean
}

type RoadmapItemUnion =
  | RoadmapItemListDto
  | RoadmapActivityListDto
  | RoadmapMilestoneListDto
  | RoadmapTimeboxListDto

const TYPE_FILTER_OPTIONS: FilterOption[] = [
  { label: 'Activity', value: 'Activity' },
  { label: 'Milestone', value: 'Milestone' },
  { label: 'Timebox', value: 'Timebox' },
]

function mapToTreeNode(item: RoadmapItemUnion): RoadmapItemTreeNode {
  const node: RoadmapItemTreeNode = {
    id: item.id,
    name: item.name,
    type: item.type.name,
    parentId: item.parent?.id ?? null,
    start:
      'start' in item && item.start ? new Date(item.start as any) : null,
    end: 'end' in item && item.end ? new Date(item.end as any) : null,
    date:
      'date' in item && item.date ? new Date(item.date as any) : null,
    color: item.color ?? null,
    children: [],
  }

  if ('children' in item && Array.isArray(item.children) && item.children.length > 0) {
    node.children = item.children.map((child) => mapToTreeNode(child))
  }

  return node
}

const RoadmapItemsGrid: FC<RoadmapItemsGridProps> = (props) => {
  const [openUpdateRoadmapActivityForm, setOpenUpdateRoadmapActivityForm] =
    useState(false)
  const [openUpdateRoadmapTimeboxForm, setOpenUpdateRoadmapTimeboxForm] =
    useState(false)
  const [openDeleteRoadmapItemForm, setOpenDeleteRoadmapItemForm] =
    useState(false)
  const [selectedItemId, setSelectedItemId] = useState<string | null>(null)

  const onEditItem = useCallback((item: RoadmapItemTreeNode) => {
    setSelectedItemId(item.id)
    if (item.type === 'Activity') {
      setOpenUpdateRoadmapActivityForm(true)
    } else if (item.type === 'Timebox') {
      setOpenUpdateRoadmapTimeboxForm(true)
    }
  }, [])

  const onDeleteItem = useCallback((item: RoadmapItemTreeNode) => {
    setSelectedItemId(item.id)
    setOpenDeleteRoadmapItemForm(true)
  }, [])

  const treeData = useMemo(() => {
    if (props.roadmapItemsIsLoading) return []
    return props.roadmapItemsData.map((item) => mapToTreeNode(item))
  }, [props.roadmapItemsData, props.roadmapItemsIsLoading])

  const columns = useMemo(
    () =>
      getRoadmapItemsGridColumns({
        isRoadmapManager: props.isRoadmapManager,
        onEditItem,
        onDeleteItem,
        openRoadmapItemDrawer: props.openRoadmapItemDrawer,
        typeFilterOptions: TYPE_FILTER_OPTIONS,
      }),
    [props.isRoadmapManager, props.openRoadmapItemDrawer, onEditItem, onDeleteItem],
  )

  const onUpdateRoadmapActivityFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRoadmapActivityForm(false)
    setSelectedItemId(null)
    if (wasSaved) {
      props.refreshRoadmapItems()
    }
  }

  const onUpdateRoadmapTimeboxFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRoadmapTimeboxForm(false)
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
      <TreeGrid<RoadmapItemTreeNode>
        data={treeData}
        isLoading={props.roadmapItemsIsLoading}
        columns={columns}
        rightSlot={props.viewSelector}
        onRefresh={async () => props.refreshRoadmapItems()}
        emptyMessage="No Roadmap Items"
        csvFileName="roadmap-items"
      />
      {openUpdateRoadmapActivityForm && (
        <EditRoadmapActivityForm
          showForm={openUpdateRoadmapActivityForm}
          activityId={selectedItemId}
          roadmapId={props.roadmapId}
          onFormComplete={() => onUpdateRoadmapActivityFormClosed(true)}
          onFormCancel={() => onUpdateRoadmapActivityFormClosed(false)}
        />
      )}
      {openUpdateRoadmapTimeboxForm && (
        <EditRoadmapTimeboxForm
          showForm={openUpdateRoadmapTimeboxForm}
          timeboxId={selectedItemId}
          roadmapId={props.roadmapId}
          onFormComplete={() => onUpdateRoadmapTimeboxFormClosed(true)}
          onFormCancel={() => onUpdateRoadmapTimeboxFormClosed(false)}
        />
      )}
      {openDeleteRoadmapItemForm && (
        <DeleteRoadmapItemForm
          roadmapId={props.roadmapId}
          roadmapItemId={selectedItemId}
          showForm={openDeleteRoadmapItemForm}
          onFormComplete={() => onDeleteRoadmapItemFormClosed(true)}
          onFormCancel={() => onDeleteRoadmapItemFormClosed(false)}
        />
      )}
    </>
  )
}

export default RoadmapItemsGrid
