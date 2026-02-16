'use client'

import {
  CaretDownOutlined,
  CaretRightOutlined,
  DeleteOutlined,
  EditOutlined,
  HolderOutlined,
  MoreOutlined,
} from '@ant-design/icons'
import { Button, ColorPicker, Dropdown, Flex } from 'antd'
import dayjs from 'dayjs'
import { ColumnDef } from '@tanstack/react-table'
import styles from '@/src/components/common/tree-grid/tree-grid.module.css'
import {
  type FilterOption,
  type TreeGridColumnMeta,
  setContainsFilter,
  dateSortBy,
  useTreeGridDragHandle,
} from '@/src/components/common/tree-grid'
import type { RoadmapItemTreeNode } from './roadmap-items-grid'

const DATE_FORMAT = 'MMM D, YYYY'
const DRAG_HANDLE_STYLE = {
  cursor: 'grab',
  touchAction: 'none',
} as const
const NAME_LINK_STYLE = {
  flex: 1,
  minWidth: 0,
  overflow: 'hidden',
  textOverflow: 'ellipsis',
  whiteSpace: 'nowrap',
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

interface RoadmapItemsGridColumnsParams {
  isRoadmapManager: boolean
  onEditItem: (item: RoadmapItemTreeNode) => void
  onDeleteItem: (item: RoadmapItemTreeNode) => void
  openRoadmapItemDrawer: (itemId: string) => void
  typeFilterOptions: FilterOption[]
  isDragEnabled: boolean
  enableDragAndDrop: boolean
}

export const getRoadmapItemsGridColumns = ({
  isRoadmapManager,
  onEditItem,
  onDeleteItem,
  openRoadmapItemDrawer,
  typeFilterOptions,
  isDragEnabled,
  enableDragAndDrop,
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
              return (
                <Flex justify="end" gap={8}>
                  {enableDragAndDrop && (
                    <DragHandleCell
                      isDragEnabled={isDragEnabled}
                      isActivity={item.type === 'Activity'}
                    />
                  )}
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

        return (
          <Flex className={styles.nameCell} align="center" gap={0}>
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
            <a
              onClick={(e) => {
                e.stopPropagation()
                openRoadmapItemDrawer(item.id)
              }}
              style={NAME_LINK_STYLE}
            >
              {item.name}
            </a>
          </Flex>
        )
      },
    },
    {
      accessorKey: 'type',
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
    },
    {
      accessorKey: 'color',
      header: 'Color',
      size: 100,
      enableSorting: false,
      enableGlobalFilter: false,
      enableColumnFilter: false,
      meta: {
        enableExport: false,
      } satisfies TreeGridColumnMeta,
      cell: (info) => {
        const value = info.getValue() as string | null | undefined
        if (!value) return null
        return <ColorPicker value={value} size="small" showText disabled />
      },
    },
  ]
}
