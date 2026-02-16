'use client'

import {
  CaretDownOutlined,
  CaretRightOutlined,
  DeleteOutlined,
  EditOutlined,
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
} from '@/src/components/common/tree-grid'
import type { RoadmapItemTreeNode } from './roadmap-items-grid'

interface RoadmapItemsGridColumnsParams {
  isRoadmapManager: boolean
  onEditItem: (item: RoadmapItemTreeNode) => void
  onDeleteItem: (item: RoadmapItemTreeNode) => void
  openRoadmapItemDrawer: (itemId: string) => void
  typeFilterOptions: FilterOption[]
}

export const getRoadmapItemsGridColumns = ({
  isRoadmapManager,
  onEditItem,
  onDeleteItem,
  openRoadmapItemDrawer,
  typeFilterOptions,
}: RoadmapItemsGridColumnsParams): ColumnDef<RoadmapItemTreeNode>[] => {
  return [
    ...(isRoadmapManager
      ? [
          {
            id: 'actions',
            header: '',
            size: 36,
            enableSorting: false,
            enableGlobalFilter: false,
            enableColumnFilter: false,
            enableResizing: false,
            meta: { enableExport: false } satisfies TreeGridColumnMeta,
            cell: ({ row }: { row: any }) => {
              const item = row.original as RoadmapItemTreeNode
              return (
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
              style={{
                flex: 1,
                minWidth: 0,
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap',
              }}
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
        const dateValue =
          row.type === 'Milestone' ? row.date : row.start
        return dateValue ? dayjs(dateValue).format('MMM D, YYYY') : ''
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
          const dateValue =
            row.type === 'Milestone' ? row.date : row.start
          return dateValue ? dayjs(dateValue).format('MMM D, YYYY') : ''
        },
      } satisfies TreeGridColumnMeta,
    },
    {
      id: 'end',
      accessorFn: (row) =>
        row.end ? dayjs(row.end).format('MMM D, YYYY') : '',
      header: 'End',
      size: 120,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: dateSortBy((row: any) => row.original.end),
      meta: {
        exportFormatter: (value) =>
          value ? dayjs(value as string).format('MMM D, YYYY') : '',
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
