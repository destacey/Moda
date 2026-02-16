'use client'

import {
  RoadmapActivityListDto,
  RoadmapItemListDto,
  RoadmapMilestoneListDto,
  RoadmapTimeboxListDto,
} from '@/src/services/moda-api'
import {
  TreeGrid,
  type TreeNode,
  type FilterOption,
  type MoveValidator,
  type TreeGridColumnContext,
  defaultMoveValidator,
} from '@/src/components/common/tree-grid'
import { useMessage } from '@/src/components/contexts/messaging'
import { useUpdateRoadmapActivityPlacementMutation } from '@/src/store/features/planning/roadmaps-api'
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
  $type: string
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
    $type: item.$type,
    parentId: item.parent?.id ?? null,
    start:
      'start' in item && item.start ? new Date(item.start as any) : null,
    end: 'end' in item && item.end ? new Date(item.end as any) : null,
    date:
      'date' in item && item.date ? new Date(item.date as any) : null,
    color: item.color ?? null,
    children: [],
  }

  if (
    'children' in item &&
    Array.isArray(item.children) &&
    item.children.length > 0
  ) {
    node.children = item.children.map((child) => mapToTreeNode(child))
  }

  return node
}

const RoadmapItemsGrid: FC<RoadmapItemsGridProps> = ({
  roadmapId,
  roadmapItemsData,
  roadmapItemsIsLoading,
  refreshRoadmapItems,
  viewSelector,
  openRoadmapItemDrawer,
  isRoadmapManager,
}) => {
  const messageApi = useMessage()

  const [openUpdateRoadmapActivityForm, setOpenUpdateRoadmapActivityForm] =
    useState(false)
  const [openUpdateRoadmapTimeboxForm, setOpenUpdateRoadmapTimeboxForm] =
    useState(false)
  const [openDeleteRoadmapItemForm, setOpenDeleteRoadmapItemForm] =
    useState(false)
  const [selectedItemId, setSelectedItemId] = useState<string | null>(null)

  const [updateActivityPlacement] = useUpdateRoadmapActivityPlacementMutation()

  const enableDragAndDrop = isRoadmapManager

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
    if (roadmapItemsIsLoading) return []
    return roadmapItemsData.map((item) => mapToTreeNode(item))
  }, [roadmapItemsData, roadmapItemsIsLoading])

  const roadmapActivityMoveValidator: MoveValidator<RoadmapItemTreeNode> =
    useCallback((activeNode, targetParentNode, targetParentId) => {
      // Only activities can be dragged
      if (activeNode.node.type !== 'Activity') {
        return { canMove: false, reason: 'Only activities can be reordered' }
      }
      const result = defaultMoveValidator(
        activeNode,
        targetParentNode,
        targetParentId,
      )
      if (!result.canMove) return result
      // Activities can only be dropped into other activities or root
      if (targetParentNode && targetParentNode.type !== 'Activity') {
        return {
          canMove: false,
          reason: 'Activities can only be placed under other activities',
        }
      }
      return { canMove: true }
    }, [])

  const handleNodeMove = useCallback(
    async (nodeId: string, parentId: string | null, order: number) => {
      try {
        await updateActivityPlacement({
          request: {
            roadmapId,
            itemId: nodeId,
            parentId: parentId ?? undefined,
            order,
          },
        }).unwrap()
        refreshRoadmapItems()
      } catch (error: any) {
        messageApi.error(
          error?.data?.detail ||
            'Failed to reorganize activity. Please try again.',
        )
      }
    },
    [roadmapId, updateActivityPlacement, refreshRoadmapItems, messageApi],
  )

  const columns = useCallback(
    (ctx: TreeGridColumnContext) =>
      getRoadmapItemsGridColumns({
        isRoadmapManager,
        onEditItem,
        onDeleteItem,
        openRoadmapItemDrawer,
        typeFilterOptions: TYPE_FILTER_OPTIONS,
        isDragEnabled: ctx.isDragEnabled,
        enableDragAndDrop,
      }),
    [
      isRoadmapManager,
      openRoadmapItemDrawer,
      onEditItem,
      onDeleteItem,
      enableDragAndDrop,
    ],
  )

  const onUpdateRoadmapActivityFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRoadmapActivityForm(false)
    setSelectedItemId(null)
    if (wasSaved) {
      refreshRoadmapItems()
    }
  }

  const onUpdateRoadmapTimeboxFormClosed = (wasSaved: boolean) => {
    setOpenUpdateRoadmapTimeboxForm(false)
    setSelectedItemId(null)
    if (wasSaved) {
      refreshRoadmapItems()
    }
  }

  const onDeleteRoadmapItemFormClosed = (wasSaved: boolean) => {
    setOpenDeleteRoadmapItemForm(false)
    setSelectedItemId(null)
    if (wasSaved) {
      refreshRoadmapItems()
    }
  }

  return (
    <>
      <TreeGrid<RoadmapItemTreeNode>
        data={treeData}
        isLoading={roadmapItemsIsLoading}
        columns={columns}
        rightSlot={viewSelector}
        onRefresh={async () => refreshRoadmapItems()}
        emptyMessage="No Roadmap Items"
        csvFileName="roadmap-items"
        enableDragAndDrop={enableDragAndDrop}
        onNodeMove={handleNodeMove}
        onMoveRejected={(reason) =>
          messageApi.warning(reason || 'Cannot move item to this location')
        }
        moveValidator={roadmapActivityMoveValidator}
      />
      {openUpdateRoadmapActivityForm && (
        <EditRoadmapActivityForm
          showForm={openUpdateRoadmapActivityForm}
          activityId={selectedItemId}
          roadmapId={roadmapId}
          onFormComplete={() => onUpdateRoadmapActivityFormClosed(true)}
          onFormCancel={() => onUpdateRoadmapActivityFormClosed(false)}
        />
      )}
      {openUpdateRoadmapTimeboxForm && (
        <EditRoadmapTimeboxForm
          showForm={openUpdateRoadmapTimeboxForm}
          timeboxId={selectedItemId}
          roadmapId={roadmapId}
          onFormComplete={() => onUpdateRoadmapTimeboxFormClosed(true)}
          onFormCancel={() => onUpdateRoadmapTimeboxFormClosed(false)}
        />
      )}
      {openDeleteRoadmapItemForm && (
        <DeleteRoadmapItemForm
          roadmapId={roadmapId}
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
