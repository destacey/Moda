'use client'

import {
  CaretDownOutlined,
  CaretRightOutlined,
  DeleteOutlined,
  EditOutlined,
  HolderOutlined,
  MoreOutlined,
  PlusOutlined,
} from '@ant-design/icons'
import { Button, DatePicker, Dropdown, Flex, Form, Input, Select } from 'antd'
import dayjs from 'dayjs'
import { ColumnDef } from '@tanstack/react-table'
import { ModaColorPicker } from '@/src/components/common'
import { useCallback, useRef } from 'react'
import styles from '@/src/components/common/tree-grid/tree-grid.module.css'
import {
  type FilterOption,
  type TreeGridColumnMeta,
  stringContainsFilter,
  setContainsFilter,
  dateSortBy,
  useTreeGridDragHandle,
} from '@/src/components/common/tree-grid'
import type { RoadmapItemTreeNode } from './roadmap-items-grid'

const { Item: FormItem } = Form
const DATE_FORMAT = 'MMM D, YYYY'
const DRAG_HANDLE_STYLE = {
  cursor: 'grab',
  touchAction: 'none',
} as const
const NAME_LINK_STYLE = {
  display: 'inline-block',
  maxWidth: '100%',
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
} as const
const NAME_LINK_CONTAINER_STYLE = {
  flex: 1,
  minWidth: 0,
} as const
const COLOR_SWATCH_STYLE = {
  width: 12,
  height: 12,
  borderRadius: 2,
  border: '1px solid var(--ant-color-border)',
  flexShrink: 0,
} as const

const formatDate = (dateValue?: Date | string | null) =>
  dateValue ? dayjs(dateValue).format(DATE_FORMAT) : ''

function DragHandleCell({
  isDragEnabled,
  isActivity,
}: {
  isDragEnabled: boolean
  isActivity: boolean
}) {
  const { listeners, attributes } = useTreeGridDragHandle()

  if (!isDragEnabled || !isActivity) {
    return null
  }

  return (
    <div
      {...listeners}
      {...attributes}
      className={styles.dragHandle}
      onClick={(e) => e.stopPropagation()}
      title="Drag to reorder or change parent"
      style={DRAG_HANDLE_STYLE}
    >
      <HolderOutlined style={{ fontSize: 14, color: '#8c8c8c' }} />
    </div>
  )
}

interface FocusableColorPickerFieldProps {
  value?: string
  onChange?: (value: string | undefined) => void
  rowId: string
  handleKeyDown: (
    e: React.KeyboardEvent,
    rowId: string,
    columnId: string,
  ) => Promise<void>
}

function FocusableColorPickerField({
  value,
  onChange,
  rowId,
  handleKeyDown,
}: FocusableColorPickerFieldProps) {
  const focusTargetRef = useRef<HTMLDivElement | null>(null)
  const onColorChange = useCallback(
    (nextValue: string | undefined) => {
      onChange?.(nextValue)
      setTimeout(() => {
        focusTargetRef.current?.focus()
      }, 0)
    },
    [onChange],
  )

  return (
    <div
      ref={focusTargetRef}
      tabIndex={0}
      data-color-picker-focus
      className={styles.colorPickerFocusTarget}
      onKeyDown={(e) => {
        if (e.key === ' ' || e.key === 'Spacebar' || e.key === 'Enter') {
          e.preventDefault()
          e.stopPropagation()
          const trigger = (
            e.currentTarget.querySelector('.ant-color-picker-trigger') ??
            e.currentTarget
              .closest('[data-cell-id]')
              ?.querySelector('.ant-color-picker-trigger')
          ) as HTMLElement | null
          trigger?.focus()
          trigger?.click()
          return
        }

        void handleKeyDown(e, rowId, 'color')
      }}
    >
      <ModaColorPicker value={value} onChange={onColorChange} />
    </div>
  )
}

interface RoadmapItemsGridColumnsParams {
  isRoadmapManager: boolean
  selectedRowId: string | null
  onEditItem: (item: RoadmapItemTreeNode) => void
  onDeleteItem: (item: RoadmapItemTreeNode) => void
  handleSaveRoadmapItem?: (
    itemId: string,
    updates: Record<string, any>,
  ) => Promise<boolean>
  getFieldError: (fieldName: string) => string | undefined
  handleKeyDown: (
    e: React.KeyboardEvent,
    rowId: string,
    columnId: string,
  ) => Promise<void>
  openRoadmapItemDrawer: (itemId: string) => void
  typeFilterOptions: FilterOption[]
  isDragEnabled: boolean
  enableDragAndDrop: boolean
  addDraftItemAsChild?: (parentId: string) => void
  canCreateItems?: boolean
  createTypeOptions?: Array<{ label: string; value: string }>
  isSelectedDraftActivity?: boolean
}

export const getRoadmapItemsGridColumns = ({
  isRoadmapManager,
  selectedRowId,
  onEditItem,
  onDeleteItem,
  handleSaveRoadmapItem,
  getFieldError,
  handleKeyDown,
  openRoadmapItemDrawer,
  typeFilterOptions,
  isDragEnabled,
  enableDragAndDrop,
  addDraftItemAsChild,
  canCreateItems = true,
  createTypeOptions = [],
  isSelectedDraftActivity = true,
}: RoadmapItemsGridColumnsParams): ColumnDef<RoadmapItemTreeNode>[] => {
  return [
    ...(isRoadmapManager
      ? [
          {
            id: 'actions',
            header: '',
            size: 50,
            enableSorting: false,
            enableGlobalFilter: false,
            enableColumnFilter: false,
            enableResizing: false,
            meta: { enableExport: false } satisfies TreeGridColumnMeta,
            cell: ({ row }: { row: any }) => {
              const item = row.original as RoadmapItemTreeNode
              const isDraft = item.id.startsWith('draft-')
              return (
                <Flex justify="end" gap={8}>
                  {enableDragAndDrop && (
                    <DragHandleCell
                      isDragEnabled={isDragEnabled}
                      isActivity={item.type === 'Activity'}
                    />
                  )}
                  {!isDraft && (
                    <Dropdown
                      menu={{
                        items: [
                          {
                            key: 'edit',
                            label: 'Edit',
                            icon: <EditOutlined />,
                            onClick: () => onEditItem(item),
                          },
                          {
                            key: 'delete',
                            label: 'Delete',
                            icon: <DeleteOutlined />,
                            onClick: () => onDeleteItem(item),
                          },
                        ],
                      }}
                      trigger={['click']}
                    >
                      <Button
                        type="text"
                        size="small"
                        icon={<MoreOutlined />}
                        tabIndex={-1}
                      />
                    </Dropdown>
                  )}
                </Flex>
              )
            },
          } as ColumnDef<RoadmapItemTreeNode>,
        ]
      : []),
    {
      accessorKey: 'name',
      header: 'Name',
      size: 300,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: 'alphanumeric',
      cell: ({ row }: { row: any }) => {
        const item = row.original as RoadmapItemTreeNode
        const depth = row.depth
        const isSelected = selectedRowId === item.id
        const isDraft = item.id.startsWith('draft-')
        const isActivity = item.type === 'Activity'

        return (
          <Flex
            className={styles.nameCell}
            align="center"
            justify="space-between"
            gap={4}
            style={{ width: '100%' }}
          >
            <Flex align="center" gap={0} style={{ flex: 1, minWidth: 0 }}>
              {Array.from({ length: depth }).map((_, index) => (
                <span key={index} className={styles.indentSpacer} />
              ))}
              {row.getCanExpand() ? (
                <Button
                  type="text"
                  size="small"
                  icon={
                    row.getIsExpanded() ? (
                      <CaretDownOutlined />
                    ) : (
                      <CaretRightOutlined />
                    )
                  }
                  onClick={(e) => {
                    e.stopPropagation()
                    row.getToggleExpandedHandler()()
                  }}
                  className={styles.expanderBtn}
                />
              ) : (
                <span className={styles.indentSpacer} />
              )}
              {isSelected && handleSaveRoadmapItem ? (
                <FormItem
                  name="name"
                  style={{ margin: 0, flex: 1, minWidth: 0 }}
                  rules={[
                    { required: true, message: 'Name is required' },
                    { max: 128, message: 'Name cannot exceed 128 characters' },
                  ]}
                  validateStatus={getFieldError('name') ? 'error' : ''}
                >
                  <Input
                    size="small"
                    onPressEnter={(e) => {
                      e.currentTarget.blur()
                    }}
                    onKeyDown={(e) => handleKeyDown(e, item.id, 'name')}
                    style={{ flex: 1, minWidth: 0 }}
                    status={getFieldError('name') ? 'error' : ''}
                  />
                </FormItem>
              ) : isDraft ? (
                <span style={{ ...NAME_LINK_STYLE }}>
                  {item.name || '(New Item)'}
                </span>
              ) : (
                <span style={NAME_LINK_CONTAINER_STYLE}>
                  <a
                    onClick={(e) => {
                      e.stopPropagation()
                      openRoadmapItemDrawer(item.id)
                    }}
                    style={NAME_LINK_STYLE}
                  >
                    {item.name}
                  </a>
                </span>
              )}
            </Flex>
            {!isSelected &&
              isRoadmapManager &&
              !isDraft &&
              isActivity &&
              addDraftItemAsChild && (
                <Button
                  type="text"
                  size="small"
                  icon={<PlusOutlined />}
                  className={`${styles.rowActionBtn} ${styles.inlineRowAction}`}
                  onClick={(e) => {
                    e.stopPropagation()
                    addDraftItemAsChild(item.id)
                  }}
                  disabled={!canCreateItems}
                  title="Add child item"
                />
              )}
          </Flex>
        )
      },
    },
    {
      id: 'type',
      accessorFn: (row) => row.type,
      header: 'Type',
      size: 110,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: setContainsFilter,
      sortingFn: 'text',
      meta: {
        filterType: 'select',
        filterOptions: typeFilterOptions,
      } satisfies TreeGridColumnMeta,
      cell: ({ row }: { row: any }) => {
        const item = row.original as RoadmapItemTreeNode
        const isSelected = selectedRowId === item.id
        const isDraft = item.id.startsWith('draft-')

        if (isSelected && isDraft && handleSaveRoadmapItem) {
          return (
            <FormItem
              name="itemType"
              style={{ margin: 0 }}
              validateStatus={getFieldError('itemType') ? 'error' : ''}
            >
              <Select
                size="small"
                options={createTypeOptions}
                style={{ width: '100%' }}
                onKeyDown={(e) => handleKeyDown(e, item.id, 'type')}
                status={getFieldError('itemType') ? 'error' : ''}
              />
            </FormItem>
          )
        }

        return item.type
      },
    },
    {
      id: 'start',
      accessorFn: (row) => {
        const dateValue = row.type === 'Milestone' ? row.date : row.start
        return formatDate(dateValue)
      },
      header: 'Start',
      size: 120,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: dateSortBy((row: any) => {
        const item = row.original as RoadmapItemTreeNode
        return item.type === 'Milestone' ? item.date : item.start
      }),
      meta: {
        exportFormatter: (_value, row) => {
          const dateValue = row.type === 'Milestone' ? row.date : row.start
          return formatDate(dateValue)
        },
      } satisfies TreeGridColumnMeta,
      cell: ({ row }: { row: any }) => {
        const item = row.original as RoadmapItemTreeNode
        const isSelected = selectedRowId === item.id
        const isDraft = item.id.startsWith('draft-')
        const cellId = `${item.id}-start`

        if (isSelected && handleSaveRoadmapItem) {
          return (
            <FormItem
              name="start"
              style={{ margin: 0 }}
              validateStatus={getFieldError('start') ? 'error' : ''}
            >
              <DatePicker
                size="small"
                style={{ width: '100%' }}
                onKeyDown={(e) => handleKeyDown(e, item.id, 'start')}
                onOpenChange={(open) => {
                  if (!open) {
                    setTimeout(() => {
                      const cell = document.querySelector(
                        `[data-cell-id="${cellId}"]`,
                      )
                      if (cell) {
                        const input = cell.querySelector('input') as
                          | HTMLInputElement
                          | undefined
                        input?.focus()
                      }
                    }, 0)
                  }
                }}
                status={getFieldError('start') ? 'error' : ''}
              />
            </FormItem>
          )
        }

        const dateValue = item.type === 'Milestone' ? item.date : item.start
        return formatDate(dateValue)
      },
    },
    {
      id: 'end',
      accessorFn: (row) => formatDate(row.end),
      header: 'End',
      size: 120,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: dateSortBy((row: any) => row.original.end),
      meta: {
        exportFormatter: (value) => formatDate(value as string | null),
      } satisfies TreeGridColumnMeta,
      cell: ({ row }: { row: any }) => {
        const item = row.original as RoadmapItemTreeNode
        const isSelected = selectedRowId === item.id
        const isDraft = item.id.startsWith('draft-')
        const cellId = `${item.id}-end`

        if (isSelected && handleSaveRoadmapItem && item.type !== 'Milestone') {
          return (
            <FormItem
              name="end"
              style={{ margin: 0 }}
              validateStatus={getFieldError('end') ? 'error' : ''}
            >
              <DatePicker
                size="small"
                style={{ width: '100%' }}
                onKeyDown={(e) => handleKeyDown(e, item.id, 'end')}
                onOpenChange={(open) => {
                  if (!open) {
                    setTimeout(() => {
                      const cell = document.querySelector(
                        `[data-cell-id="${cellId}"]`,
                      )
                      if (cell) {
                        const input = cell.querySelector('input') as
                          | HTMLInputElement
                          | undefined
                        input?.focus()
                      }
                    }, 0)
                  }
                }}
                status={getFieldError('end') ? 'error' : ''}
              />
            </FormItem>
          )
        }

        return formatDate(item.end)
      },
    },
    {
      id: 'color',
      accessorFn: (row) => row.color ?? 'Transparent',
      header: 'Color',
      size: 100,
      enableSorting: false,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: stringContainsFilter,
      meta: {
        exportFormatter: (_value, row) => {
          if (row.type === 'Timebox') return ''
          return row.color ?? 'Transparent'
        },
      } satisfies TreeGridColumnMeta,
      cell: ({ row }: { row: any }) => {
        const item = row.original as RoadmapItemTreeNode
        const isSelected = selectedRowId === item.id
        const isDraft = item.id.startsWith('draft-')

        if (item.type === 'Timebox') {
          return null
        }

        if (
          isSelected &&
          handleSaveRoadmapItem &&
          (!isDraft || isSelectedDraftActivity)
        ) {
          return (
            <FormItem name="color" style={{ margin: 0 }}>
              <FocusableColorPickerField
                rowId={item.id}
                handleKeyDown={handleKeyDown}
              />
            </FormItem>
          )
        }

        const value = item.color
        if (!value) {
          return (
            <Flex align="center" gap={6}>
              <span
                aria-hidden
                style={{
                  ...COLOR_SWATCH_STYLE,
                  backgroundColor: 'transparent',
                }}
              />
              <span>Transparent</span>
            </Flex>
          )
        }

        return (
          <Flex align="center" gap={6}>
            <span
              aria-hidden
              style={{ ...COLOR_SWATCH_STYLE, backgroundColor: value }}
            />
            <span>{value.toUpperCase()}</span>
          </Flex>
        )
      },
    },
  ]
}
