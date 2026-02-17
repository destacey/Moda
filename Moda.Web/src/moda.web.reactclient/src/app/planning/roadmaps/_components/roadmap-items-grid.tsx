'use client'

import {
  CreateRoadmapActivityRequest,
  CreateRoadmapTimeboxRequest,
  RoadmapActivityListDto,
  RoadmapItemListDto,
  RoadmapMilestoneListDto,
  RoadmapTimeboxListDto,
} from '@/src/services/moda-api'
import { PlusOutlined } from '@ant-design/icons'
import { Button, Form } from 'antd'
import dayjs from 'dayjs'
import {
  type DraftItem,
  TreeGrid,
  type TreeNode,
  type FilterOption,
  type MoveValidator,
  type TreeGridColumnContext,
  defaultMoveValidator,
  findNodeById,
} from '@/src/components/common/tree-grid'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useCreateRoadmapItemMutation,
  useUpdateRoadmapActivityPlacementMutation,
} from '@/src/store/features/planning/roadmaps-api'
import { FC, ReactNode, useCallback, useMemo, useRef, useState } from 'react'
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
const CREATE_TYPE_OPTIONS = [
  { label: 'Activity', value: 'activity' },
  { label: 'Timebox', value: 'timebox' },
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
  const [form] = Form.useForm()
  const selectedDraftItemType = Form.useWatch('itemType', form)
  const messageApi = useMessage()

  const [openUpdateRoadmapActivityForm, setOpenUpdateRoadmapActivityForm] =
    useState(false)
  const [openUpdateRoadmapTimeboxForm, setOpenUpdateRoadmapTimeboxForm] =
    useState(false)
  const [openDeleteRoadmapItemForm, setOpenDeleteRoadmapItemForm] =
    useState(false)
  const [selectedItemId, setSelectedItemId] = useState<string | null>(null)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const draftsRef = useRef<DraftItem[]>([])

  const [createRoadmapItem] = useCreateRoadmapItemMutation()
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

  const createDraftRoadmapItem = useCallback(
    (draft: DraftItem): RoadmapItemTreeNode => ({
      id: draft.id,
      name: '',
      type: 'Activity',
      $type: 'activity',
      parentId: draft.parentId ?? null,
      start: null,
      end: null,
      date: null,
      color: null,
      children: [],
    }),
    [],
  )

  const handleSaveRoadmapItem = useCallback(
    async (itemId: string, updates: Record<string, any>): Promise<boolean> => {
      if (!itemId.startsWith('draft-')) {
        return true
      }

      try {
        const draft = draftsRef.current.find((d) => d.id === itemId)
        if (!draft) return false

        const itemType =
          updates.itemType === 'timebox' ? 'timebox' : 'activity'
        const request:
          | CreateRoadmapActivityRequest
          | CreateRoadmapTimeboxRequest = {
          $type: itemType,
          roadmapId,
          parentId: draft.parentId,
          name: updates.name || '',
          start: updates.start,
          end: updates.end,
          ...(itemType === 'activity' ? { color: updates.color } : {}),
        } as CreateRoadmapActivityRequest | CreateRoadmapTimeboxRequest

        const response = await createRoadmapItem(request)
        if (response.error) throw response.error

        setFieldErrors({})
        messageApi.success(
          itemType === 'timebox'
            ? 'Roadmap timebox created successfully.'
            : 'Roadmap activity created successfully.',
        )
        refreshRoadmapItems()
        return true
      } catch (error: any) {
        const status = error?.status ?? error?.data?.status
        const errors = error?.errors ?? error?.data?.errors
        const detail = error?.detail ?? error?.data?.detail

        if (status === 422 && errors) {
          const errorMap: Record<string, string> = {}
          const errorFields: string[] = []
          Object.entries(errors).forEach(([key, messages]) => {
            const apiField = key.charAt(0).toLowerCase() + key.slice(1)
            const fieldName =
              apiField === 'type' || apiField === '$type'
                ? 'itemType'
                : apiField
            errorMap[fieldName] = Array.isArray(messages)
              ? String(messages[0] ?? '')
              : String(messages)
            errorFields.push(fieldName)
          })
          setFieldErrors(errorMap)

          setTimeout(() => {
            const fieldToColumn: Record<string, string> = {
              itemType: 'type',
              type: 'type',
            }

            let focused = false
            for (const errorField of errorFields) {
              const columnId = fieldToColumn[errorField] ?? errorField
              const cellElement = document.querySelector(
                `[data-cell-id="${itemId}-${columnId}"]`,
              )
              if (!cellElement) continue

              const input = cellElement.querySelector(
                'input, .ant-select, .ant-picker',
              ) as HTMLElement | null
              if (input) {
                input.focus()
                focused = true
                break
              }
            }

            if (!focused) {
              const cellElement = document.querySelector(
                `[data-cell-id="${itemId}-name"]`,
              )
              const input = cellElement?.querySelector(
                'input',
              ) as HTMLElement | null
              input?.focus()
            }
          }, 0)

          messageApi.error('Correct the validation error(s) to continue.')
          return false
        }

        messageApi.error(
          detail ??
            'An error occurred while creating the roadmap item. Please try again.',
        )
        return false
      }
    },
    [createRoadmapItem, messageApi, refreshRoadmapItems, roadmapId],
  )

  const getFormValues = useCallback(
    (rowId: string, data: RoadmapItemTreeNode[]) => {
      const item = findNodeById(data, rowId) as RoadmapItemTreeNode | null
      const isDraft = rowId.startsWith('draft-')

      if (isDraft || !item) {
        return {
          name: '',
          itemType: 'activity',
          start: null,
          end: null,
          color: null,
        }
      }

      return {
        name: item.name,
        itemType: item.$type,
        start: item.start ? dayjs(item.start) : null,
        end: item.end ? dayjs(item.end) : null,
        color: item.color ?? null,
      }
    },
    [],
  )

  const computeChanges = useCallback(
    (
      rowId: string,
      formValues: Record<string, any>,
      _data: RoadmapItemTreeNode[],
    ) => {
      if (!rowId.startsWith('draft-')) return null

      const values = formValues as any
      return {
        name: values.name || '',
        itemType: values.itemType || 'activity',
        start: values.start ? values.start.format('YYYY-MM-DD') : null,
        end: values.end ? values.end.format('YYYY-MM-DD') : null,
        color: values.color ?? null,
      }
    },
    [],
  )

  const validateFields = useCallback(
    (rowId: string, formValues: Record<string, any>) => {
      if (!rowId.startsWith('draft-')) return {}

      const errors: Record<string, string> = {}
      const name = String(formValues.name ?? '').trim()
      const start = formValues.start
      const end = formValues.end

      if (!name) {
        errors.name = 'Name is required.'
      }

      if (!start || !end) {
        const message = 'Start and end dates are required.'
        errors.start = message
        errors.end = message
      } else if (!dayjs(start).isBefore(dayjs(end), 'day')) {
        errors.end = 'End date must be after start date.'
      }

      return errors
    },
    [],
  )

  const columns = useCallback(
    (ctx: TreeGridColumnContext) =>
      getRoadmapItemsGridColumns({
        isRoadmapManager,
        selectedRowId: ctx.selectedRowId,
        onEditItem,
        onDeleteItem,
        handleSaveRoadmapItem,
        getFieldError: ctx.getFieldError,
        handleKeyDown: ctx.handleKeyDown,
        openRoadmapItemDrawer,
        typeFilterOptions: TYPE_FILTER_OPTIONS,
        isDragEnabled: ctx.isDragEnabled,
        enableDragAndDrop,
        addDraftItemAsChild: ctx.addDraftAsChild,
        canCreateItems: ctx.canCreateDraft,
        createTypeOptions: CREATE_TYPE_OPTIONS,
        isSelectedDraftActivity: selectedDraftItemType !== 'timebox',
      }),
    [
      isRoadmapManager,
      openRoadmapItemDrawer,
      onEditItem,
      onDeleteItem,
      handleSaveRoadmapItem,
      enableDragAndDrop,
      selectedDraftItemType,
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
      <Form form={form} component={false}>
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
          editingConfig={{
            canEdit: isRoadmapManager,
            form,
            editableColumnIds: (rowId) =>
              rowId?.startsWith('draft-')
                ? ['name', 'type', 'start', 'end', 'color']
                : [],
            onSave: handleSaveRoadmapItem,
            getFormValues,
            computeChanges,
            validateFields,
            cellIdColumnMatchOrder: ['start', 'type', 'name', 'color', 'end'],
          }}
          fieldErrors={fieldErrors}
          onFieldErrorsChange={setFieldErrors}
          createDraftNode={createDraftRoadmapItem}
          onDraftsChange={(drafts) => {
            draftsRef.current = drafts
          }}
          leftSlot={(ctx) =>
            isRoadmapManager && (
              <Button
                icon={<PlusOutlined />}
                onClick={() => ctx.addDraftAtRoot()}
                disabled={!ctx.canCreateDraft}
              >
                Create Item
              </Button>
            )
          }
        />
      </Form>
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
