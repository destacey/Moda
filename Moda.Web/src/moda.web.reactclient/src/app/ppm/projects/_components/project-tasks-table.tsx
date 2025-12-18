'use client'

import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import { ModaEmpty } from '@/src/components/common'
import {
  Button,
  DatePicker,
  Input,
  Select,
  Space,
  Spin,
  Tag,
  Tooltip,
  Typography,
  theme as antdTheme,
} from 'antd'
import {
  CaretRightOutlined,
  CaretDownOutlined,
  CaretUpOutlined,
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
  ClearOutlined,
  SearchOutlined,
  DownloadOutlined,
  FilterOutlined,
} from '@ant-design/icons'
import {
  Fragment,
  type ChangeEvent,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import dayjs from 'dayjs'
import {
  ColumnDef,
  type ColumnFiltersState,
  type ColumnSizingState,
  flexRender,
  getCoreRowModel,
  getFilteredRowModel,
  getExpandedRowModel,
  getSortedRowModel,
  type SortingState,
  useReactTable,
} from '@tanstack/react-table'

const { Text } = Typography

interface ProjectTasksTableProps {
  tasks: ProjectTaskTreeDto[]
  isLoading: boolean
  onCreateTask?: () => void
  onEditTask?: (task: ProjectTaskTreeDto) => void
  onDeleteTask?: (task: ProjectTaskTreeDto) => void
  onRefresh?: () => void
  onUpdateStatus?: (taskId: string, statusId: number) => Promise<void>
  onUpdatePriority?: (taskId: string, priorityId: number) => Promise<void>
  onUpdateName?: (taskId: string, name: string) => Promise<void>
  onUpdateType?: (taskId: string, typeId: number) => Promise<void>
  onUpdatePlannedStart?: (taskId: string, date: string | null) => Promise<void>
  onUpdatePlannedEnd?: (taskId: string, date: string | null) => Promise<void>
}

const ProjectTasksTable = ({
  tasks,
  isLoading,
  onCreateTask,
  onEditTask,
  onDeleteTask,
  onRefresh,
  onUpdateStatus,
  onUpdatePriority,
  onUpdateName,
  onUpdateType,
  onUpdatePlannedStart,
  onUpdatePlannedEnd,
}: ProjectTasksTableProps) => {
  const [searchValue, setSearchValue] = useState('')
  const [sorting, setSorting] = useState<SortingState>([])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])
  const [columnSizing, setColumnSizing] = useState<ColumnSizingState>({})
  const [selectedRowId, setSelectedRowId] = useState<string | null>(null)
  const [selectedCellId, setSelectedCellId] = useState<string | null>(null)

  const { typeOptions, statusOptions, priorityOptions } = useMemo(() => {
    const types = new Set<string>()
    const statuses = new Set<string>()
    const priorities = new Set<string>()

    const walk = (items: ProjectTaskTreeDto[]) => {
      for (const item of items) {
        if (item.type?.name) types.add(item.type.name)
        if (item.status?.name) statuses.add(item.status.name)
        if (item.priority?.name) priorities.add(item.priority.name)
        if (item.children?.length) walk(item.children)
      }
    }

    walk(tasks)

    const toOptions = (values: Set<string>) =>
      Array.from(values)
        .sort((a, b) => a.localeCompare(b))
        .map((v) => ({ label: v, value: v }))

    return {
      typeOptions: toOptions(types),
      statusOptions: toOptions(statuses),
      priorityOptions: toOptions(priorities),
    }
  }, [tasks])

  const totalRowCount = useMemo(() => {
    const count = (items: ProjectTaskTreeDto[]): number =>
      items.reduce(
        (acc, item) =>
          acc + 1 + (item.children?.length ? count(item.children) : 0),
        0,
      )
    return count(tasks)
  }, [tasks])

  // Static CSS using Ant Design CSS variables - automatically updates with theme
  const cssString = `
    .moda-project-tasks-table {
      width: 100%;
    }
    .moda-project-tasks-table__toolbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--ant-margin);
      margin-bottom: var(--ant-margin);
      flex-wrap: nowrap;
      overflow: hidden;
    }
    .moda-project-tasks-table__toolbar-right {
      display: flex;
      align-items: center;
      justify-content: flex-end;
      gap: var(--ant-margin-sm);
      flex-wrap: nowrap;
      white-space: nowrap;
    }
    .moda-project-tasks-table__toolbar-search {
      width: 225;
    }
    .moda-project-tasks-table__toolbar-divider {
      width: 1px;
      height: 30px;
      background-color: var(--ant-color-split);
    }
    .moda-project-tasks-table__table-wrapper {
      overflow-x: auto;
      overflow-y: hidden;
      border-radius: var(--ant-border-radius-lg);
      border: 1px solid var(--ant-color-border);
      background-color: var(--ant-color-bg-container);
    }
    .moda-project-tasks-table__table {
      width: max-content;
      min-width: 100%;
      border-collapse: separate;
      border-spacing: 0;
      background-color: transparent;
      font-size: var(--ant-font-size);
      table-layout: fixed;
    }
    .moda-project-tasks-table__th {
      padding: var(--ant-padding-sm) var(--ant-padding);
      background-color: var(--ant-color-fill-alter);
      border-bottom: 1px solid var(--ant-color-border);
      color: var(--ant-color-text);
      text-align: left;
      font-weight: var(--ant-font-weight-strong);
      white-space: nowrap;
      position: relative;
      overflow: hidden;
    }
    .moda-project-tasks-table__th--sortable {
      cursor: pointer;
      user-select: none;
    }
    .moda-project-tasks-table__th--resizable {
      padding-right: var(--ant-padding-lg);
    }
    .moda-project-tasks-table__th-content {
      display: inline-flex;
      align-items: center;
      gap: var(--ant-margin-xs);
    }
    .moda-project-tasks-table__resizer {
      position: absolute;
      top: 0;
      right: 0;
      width: 10px;
      height: 100%;
      cursor: col-resize;
      user-select: none;
      touch-action: none;
      z-index: 1;
    }
    .moda-project-tasks-table__resizer::after {
      content: "";
      position: absolute;
      top: var(--ant-padding-xs);
      bottom: var(--ant-padding-xs);
      right: 4px;
      width: 1px;
      background-color: var(--ant-color-border);
      opacity: 0.7;
    }
    .moda-project-tasks-table__resizer--active::after {
      background-color: var(--ant-color-primary);
      opacity: 1;
    }
    .moda-project-tasks-table__filter-th {
      padding: 4px 4px;
      background-color: var(--ant-color-fill-alter);
      border-bottom: 1px solid var(--ant-color-border);
      color: var(--ant-color-text);
      font-weight: normal;
      overflow: hidden;
      text-align: left;
    }
    .moda-project-tasks-table__filter-control {
      width: 100%;
      min-width: 0;
      max-width: 100%;
      display: block;
      box-sizing: border-box;
    }
    .moda-project-tasks-table__filter-control.ant-input-affix-wrapper {
      min-width: 0;
      max-width: 100%;
    }
    .moda-project-tasks-table__filter-control.ant-input-affix-wrapper .ant-input {
      min-width: 0;
      font-weight: normal;
    }
    .moda-project-tasks-table__filter-control.ant-select {
      width: 100%;
      display: block;
      min-width: 0;
      max-width: 100%;
      position: relative;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selector {
      min-width: 0;
      max-width: 100%;
      align-items: center;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selection-item {
      font-weight: normal;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selection-search {
      min-width: 0;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selection-search-input {
      font-weight: normal;
    }
    .moda-project-tasks-table__filter-popup {
      min-width: 220px;
    }
    .moda-project-tasks-table__td {
      padding: var(--ant-padding-xs) var(--ant-padding-sm);
      border-bottom: 1px solid var(--ant-color-border-secondary);
      color: var(--ant-color-text);
      vertical-align: middle;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }
    .moda-project-tasks-table__tr--alt .moda-project-tasks-table__td {
      background-color: var(--ant-color-fill-quaternary);
    }
    .moda-project-tasks-table__tr:hover .moda-project-tasks-table__td {
      background-color: var(--ant-color-fill-secondary);
    }
    .moda-project-tasks-table__tr--selected .moda-project-tasks-table__td {
      background-color: var(--ant-color-bg-container);
    }
    .moda-project-tasks-table__tr {
      cursor: pointer;
    }
    .moda-project-tasks-table__editable-cell {
      padding: 2px 4px !important;
    }
    .moda-project-tasks-table__editable-cell .ant-input,
    .moda-project-tasks-table__editable-cell .ant-select,
    .moda-project-tasks-table__editable-cell .ant-picker {
      border: 1px solid var(--ant-color-border);
      background-color: var(--ant-color-bg-container);
      border-radius: var(--ant-border-radius);
    }
    .moda-project-tasks-table__editable-cell .ant-input:hover,
    .moda-project-tasks-table__editable-cell .ant-select:hover .ant-select-selector,
    .moda-project-tasks-table__editable-cell .ant-picker:hover {
      border-color: var(--ant-color-primary-hover);
    }
    .moda-project-tasks-table__editable-cell .ant-input:focus,
    .moda-project-tasks-table__editable-cell .ant-select-focused .ant-select-selector,
    .moda-project-tasks-table__editable-cell .ant-select.ant-select-open .ant-select-selector,
    .moda-project-tasks-table__editable-cell .ant-picker-focused {
      border-color: var(--ant-color-primary) !important;
      border-width: 2px !important;
      box-shadow: 0 0 0 3px var(--ant-color-primary-bg);
    }
    .moda-project-tasks-table__expander-btn {
      padding: 0 4px;
    }
    .moda-project-tasks-table__name-cell {
      display: flex;
      align-items: center;
      gap: var(--ant-margin-xxs);
      min-width: 0;
      width: 100%;
    }
    .moda-project-tasks-table__indent-spacer {
      display: inline-block;
      width: 24px;
      flex: 0 0 24px;
    }
    .moda-project-tasks-table__empty {
      padding: var(--ant-padding-lg);
    }
    .moda-project-tasks-table__loading {
      padding: var(--ant-padding-lg);
      display: flex;
      justify-content: center;
    }
  `

  // Editable columns in order
  const editableColumns = useMemo(
    () => ['name', 'type', 'status', 'priority', 'plannedStart', 'plannedEnd'],
    [],
  )

  // Use a ref to store the table instance to avoid circular dependency
  const tableRef = useRef<any>(null)

  // Global keyboard handler for Tab navigation
  useEffect(() => {
    if (!selectedRowId || !tableRef.current) return

    const handleGlobalKeyDown = (e: KeyboardEvent) => {
      if (e.key !== 'Tab') return

      const activeElement = document.activeElement
      const currentCellElement = activeElement?.closest('[data-cell-id]')
      if (!currentCellElement) return

      const cellId = currentCellElement.getAttribute('data-cell-id')
      if (!cellId) return

      const parts = cellId.split('-')
      const columnId = parts.slice(1).join('-')
      const currentColIndex = editableColumns.indexOf(columnId)
      if (currentColIndex === -1) return

      e.preventDefault()
      e.stopPropagation()

      const rows = tableRef.current.getRowModel().rows
      const currentRowIndex = rows.findIndex(
        (r: any) => r.original.id === selectedRowId,
      )
      if (currentRowIndex === -1) return

      let nextRowId: string | null = null
      let nextColId: string | null = null

      if (e.shiftKey) {
        // Tab backwards
        if (currentColIndex > 0) {
          nextColId = editableColumns[currentColIndex - 1]
          nextRowId = selectedRowId
        } else if (currentRowIndex > 0) {
          nextColId = editableColumns[editableColumns.length - 1]
          nextRowId = rows[currentRowIndex - 1].original.id
        }
      } else {
        // Tab forwards
        if (currentColIndex < editableColumns.length - 1) {
          nextColId = editableColumns[currentColIndex + 1]
          nextRowId = selectedRowId
        } else if (currentRowIndex < rows.length - 1) {
          nextColId = editableColumns[0]
          nextRowId = rows[currentRowIndex + 1].original.id
        }
      }

      if (nextRowId && nextColId) {
        setSelectedRowId(nextRowId)
        setSelectedCellId(`${nextRowId}-${nextColId}`)

        setTimeout(() => {
          const nextCell = document.querySelector(
            `[data-cell-id="${nextRowId}-${nextColId}"]`,
          )
          if (nextCell) {
            const input = nextCell.querySelector('input, .ant-select, .ant-picker')
            if (input instanceof HTMLElement) {
              input.focus()
              if (input instanceof HTMLInputElement) {
                input.select()
              }
            }
          }
        }, 10)
      }
    }

    document.addEventListener('keydown', handleGlobalKeyDown, true)
    return () => {
      document.removeEventListener('keydown', handleGlobalKeyDown, true)
    }
  }, [selectedRowId, editableColumns])

  // Handle keyboard navigation
  const handleKeyDown = useCallback(
    (e: React.KeyboardEvent, rowId: string, columnId: string) => {
      if (!selectedRowId || !tableRef.current) return

      const rows = tableRef.current.getRowModel().rows
      const currentRowIndex = rows.findIndex((r: any) => r.original.id === rowId)
      if (currentRowIndex === -1) return

      const currentColIndex = editableColumns.indexOf(columnId)
      if (currentColIndex === -1) return

      let nextRowId: string | null = null
      let nextColId: string | null = null

      switch (e.key) {
        case 'Enter':
          e.preventDefault()
          // Move down to same column
          if (currentRowIndex < rows.length - 1) {
            nextRowId = rows[currentRowIndex + 1].original.id
            nextColId = columnId
          }
          break

        case 'ArrowUp':
          e.preventDefault()
          if (currentRowIndex > 0) {
            nextRowId = rows[currentRowIndex - 1].original.id
            nextColId = columnId
          }
          break

        case 'ArrowDown':
          e.preventDefault()
          if (currentRowIndex < rows.length - 1) {
            nextRowId = rows[currentRowIndex + 1].original.id
            nextColId = columnId
          }
          break

        case 'Escape':
          e.preventDefault()
          setSelectedRowId(null)
          setSelectedCellId(null)
          break
      }

      if (nextRowId && nextColId) {
        setSelectedRowId(nextRowId)
        setSelectedCellId(`${nextRowId}-${nextColId}`)

        // Focus the next cell after a short delay to allow rendering
        setTimeout(() => {
          const nextCell = document.querySelector(
            `[data-cell-id="${nextRowId}-${nextColId}"]`,
          )
          if (nextCell) {
            const input = nextCell.querySelector('input, .ant-select, .ant-picker')
            if (input instanceof HTMLElement) {
              input.focus()
              if (input instanceof HTMLInputElement) {
                input.select()
              }
            }
          }
        }, 10)
      }
    },
    [selectedRowId, editableColumns],
  )

  const columns = useMemo<ColumnDef<ProjectTaskTreeDto>[]>(
    () => [
      {
        id: 'expander',
        header: () => null,
        size: 40,
        minSize: 40,
        enableSorting: false,
        enableGlobalFilter: false,
        enableColumnFilter: false,
        cell: ({ row }) => {
          return row.getCanExpand() ? (
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
              onClick={row.getToggleExpandedHandler()}
              className="moda-project-tasks-table__expander-btn"
            />
          ) : null
        },
      },
      {
        accessorKey: 'wbs',
        header: 'WBS',
        size: 100,
        enableGlobalFilter: true,
        enableColumnFilter: true,
        filterFn: 'includesString',
      },
      {
        accessorKey: 'taskKey',
        header: 'Key',
        size: 120,
        enableGlobalFilter: true,
        enableColumnFilter: true,
        filterFn: 'includesString',
        cell: (info) => info.getValue(),
      },
      {
        accessorKey: 'name',
        header: 'Name',
        size: 300,
        enableGlobalFilter: true,
        enableColumnFilter: true,
        filterFn: 'includesString',
        cell: ({ row }) => {
          const depth = row.depth
          const task = row.original
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-name`

          const nameContent = (
            <span
              className="moda-project-tasks-table__name-cell"
              data-cell-id={cellId}
            >
              {Array.from({ length: depth }).map((_, index) => (
                <span
                  key={index}
                  className="moda-project-tasks-table__indent-spacer"
                />
              ))}
              {isSelected && onUpdateName ? (
                <Input
                  size="small"
                  variant="borderless"
                  defaultValue={task.name}
                  onBlur={(e) => {
                    const newName = e.target.value.trim()
                    if (newName && newName !== task.name) {
                      onUpdateName(task.id, newName)
                    }
                  }}
                  onPressEnter={(e) => {
                    e.currentTarget.blur()
                  }}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'name')}
                  onClick={(e) => e.stopPropagation()}
                  style={{ flex: 1, minWidth: 0 }}
                />
              ) : (
                <span>{task.name}</span>
              )}
            </span>
          )

          return nameContent
        },
      },
      {
        id: 'type',
        accessorFn: (row) => row.type?.name ?? '',
        header: 'Type',
        size: 110,
        enableGlobalFilter: true,
        enableColumnFilter: true,
        filterFn: (row, columnId, filterValue) => {
          const cellValue = String(row.getValue(columnId) ?? '')
          if (filterValue == null) return true
          if (Array.isArray(filterValue)) {
            return filterValue.length === 0 || filterValue.includes(cellValue)
          }
          return String(filterValue) === cellValue
        },
        cell: (info) => {
          const task = info.row.original
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-type`

          if (!isSelected || !onUpdateType) {
            return info.getValue()
          }

          const typeOptions = [
            { label: 'Task', value: 0 },
            { label: 'Milestone', value: 1 },
          ]

          return (
            <div data-cell-id={cellId}>
              <Select
                size="small"
                value={task.type?.id}
                options={typeOptions}
                style={{ width: '100%' }}
                variant="borderless"
                onChange={(value) => onUpdateType(task.id, value)}
                onClick={(e) => e.stopPropagation()}
                onKeyDown={(e) => handleKeyDown(e, task.id, 'type')}
              />
            </div>
          )
        },
      },
      {
        id: 'status',
        accessorFn: (row) => row.status?.name ?? '',
        header: 'Status',
        size: 130,
        enableGlobalFilter: true,
        enableColumnFilter: true,
        filterFn: (row, columnId, filterValue) => {
          const cellValue = String(row.getValue(columnId) ?? '')
          if (filterValue == null) return true
          if (Array.isArray(filterValue)) {
            return filterValue.length === 0 || filterValue.includes(cellValue)
          }
          return String(filterValue) === cellValue
        },
        cell: (info) => {
          const task = info.row.original
          const status = (info.getValue() as string) ?? ''
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-status`
          const colorMap: Record<string, string> = {
            'Not Started': 'default',
            'In Progress': 'processing',
            Completed: 'success',
            Cancelled: 'error',
          }

          if (!isSelected || !onUpdateStatus) {
            return (
              <Tag color={colorMap[status] || 'default'}>{status || '-'}</Tag>
            )
          }

          const statusOptions = [
            { label: 'Not Started', value: 0 },
            { label: 'In Progress', value: 1 },
            { label: 'Completed', value: 2 },
            { label: 'Cancelled', value: 3 },
          ]

          return (
            <div data-cell-id={cellId}>
              <Select
                size="small"
                value={task.status?.id}
                options={statusOptions}
                style={{ width: '100%' }}
                variant="borderless"
                onChange={(value) => onUpdateStatus(task.id, value)}
                onClick={(e) => e.stopPropagation()}
                onKeyDown={(e) => handleKeyDown(e, task.id, 'status')}
              />
            </div>
          )
        },
      },
      {
        id: 'priority',
        accessorFn: (row) => row.priority?.name ?? '',
        header: 'Priority',
        size: 110,
        enableGlobalFilter: true,
        enableColumnFilter: true,
        filterFn: (row, columnId, filterValue) => {
          const cellValue = String(row.getValue(columnId) ?? '')
          if (filterValue == null) return true
          if (Array.isArray(filterValue)) {
            return filterValue.length === 0 || filterValue.includes(cellValue)
          }
          return String(filterValue) === cellValue
        },
        cell: (info) => {
          const task = info.row.original
          const priority = (info.getValue() as string) ?? ''
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-priority`

          if (!isSelected || !onUpdatePriority) {
            if (!priority) return '-'
            const colorMap: Record<string, string> = {
              Low: 'green',
              Medium: 'orange',
              High: 'red',
              Critical: 'magenta',
            }
            return <Tag color={colorMap[priority]}>{priority}</Tag>
          }

          const priorityOptions = [
            { label: 'Low', value: 0 },
            { label: 'Medium', value: 1 },
            { label: 'High', value: 2 },
            { label: 'Critical', value: 3 },
          ]

          return (
            <div data-cell-id={cellId}>
              <Select
                size="small"
                value={task.priority?.id}
                options={priorityOptions}
                style={{ width: '100%' }}
                variant="borderless"
                onChange={(value) => onUpdatePriority(task.id, value)}
                onClick={(e) => e.stopPropagation()}
                onKeyDown={(e) => handleKeyDown(e, task.id, 'priority')}
              />
            </div>
          )
        },
      },
      {
        id: 'plannedStart',
        accessorFn: (row) =>
          row.plannedStart ? dayjs(row.plannedStart).format('MMM D, YYYY') : '',
        header: 'Planned Start',
        size: 130,
        enableGlobalFilter: false,
        enableColumnFilter: true,
        filterFn: 'includesString',
        cell: (info) => {
          const task = info.row.original
          const value = (info.getValue() as string) ?? ''
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-plannedStart`

          if (!isSelected || !onUpdatePlannedStart) {
            return value || '-'
          }

          return (
            <div data-cell-id={cellId}>
              <DatePicker
                size="small"
                variant="borderless"
                value={task.plannedStart ? dayjs(task.plannedStart) : null}
                format="MMM D, YYYY"
                onChange={(date) => {
                  onUpdatePlannedStart(
                    task.id,
                    date ? date.format('YYYY-MM-DD') : null,
                  )
                }}
                onClick={(e) => e.stopPropagation()}
                onKeyDown={(e) => handleKeyDown(e, task.id, 'plannedStart')}
                style={{ width: '100%' }}
                placeholder="-"
              />
            </div>
          )
        },
        sortingFn: (a, b) => {
          const av = a.original.plannedStart
            ? dayjs(a.original.plannedStart).valueOf()
            : -Infinity
          const bv = b.original.plannedStart
            ? dayjs(b.original.plannedStart).valueOf()
            : -Infinity
          return av === bv ? 0 : av > bv ? 1 : -1
        },
      },
      {
        id: 'plannedEnd',
        accessorFn: (row) =>
          row.plannedEnd ? dayjs(row.plannedEnd).format('MMM D, YYYY') : '',
        header: 'Planned End',
        size: 130,
        enableGlobalFilter: false,
        enableColumnFilter: true,
        filterFn: 'includesString',
        cell: (info) => {
          const task = info.row.original
          const value = (info.getValue() as string) ?? ''
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-plannedEnd`

          if (!isSelected || !onUpdatePlannedEnd) {
            return value || '-'
          }

          return (
            <div data-cell-id={cellId}>
              <DatePicker
                size="small"
                variant="borderless"
                value={task.plannedEnd ? dayjs(task.plannedEnd) : null}
                format="MMM D, YYYY"
                onChange={(date) => {
                  onUpdatePlannedEnd(
                    task.id,
                    date ? date.format('YYYY-MM-DD') : null,
                  )
                }}
                onClick={(e) => e.stopPropagation()}
                onKeyDown={(e) => handleKeyDown(e, task.id, 'plannedEnd')}
                style={{ width: '100%' }}
                placeholder="-"
              />
            </div>
          )
        },
        sortingFn: (a, b) => {
          const av = a.original.plannedEnd
            ? dayjs(a.original.plannedEnd).valueOf()
            : -Infinity
          const bv = b.original.plannedEnd
            ? dayjs(b.original.plannedEnd).valueOf()
            : -Infinity
          return av === bv ? 0 : av > bv ? 1 : -1
        },
      },
      {
        id: 'actions',
        header: 'Actions',
        size: 110,
        enableSorting: false,
        enableGlobalFilter: false,
        enableColumnFilter: false,
        cell: ({ row }) => (
          <Space size="small">
            {onEditTask && (
              <Tooltip title="Edit">
                <Button
                  type="text"
                  size="small"
                  icon={<EditOutlined />}
                  onClick={() => onEditTask(row.original)}
                />
              </Tooltip>
            )}
            {onDeleteTask && (
              <Tooltip title="Delete">
                <Button
                  type="text"
                  size="small"
                  danger
                  icon={<DeleteOutlined />}
                  onClick={() => onDeleteTask(row.original)}
                />
              </Tooltip>
            )}
          </Space>
        ),
      },
    ],
    [
      onEditTask,
      onDeleteTask,
      onUpdateStatus,
      onUpdatePriority,
      onUpdateName,
      onUpdateType,
      onUpdatePlannedStart,
      onUpdatePlannedEnd,
      selectedRowId,
      handleKeyDown,
    ],
  )

  const table = useReactTable({
    data: tasks,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    getSubRows: (row) => row.children,
    filterFromLeafRows: true,
    globalFilterFn: 'includesString',
    enableMultiSort: true,
    isMultiSortEvent: (e) =>
      (e as unknown as { ctrlKey?: boolean } | null)?.ctrlKey === true,
    enableColumnResizing: true,
    columnResizeMode: 'onChange',
    state: {
      globalFilter: searchValue,
      sorting,
      columnFilters,
      columnSizing,
    },
    onGlobalFilterChange: (value) => setSearchValue(String(value ?? '')),
    onSortingChange: setSorting,
    onColumnFiltersChange: setColumnFilters,
    onColumnSizingChange: setColumnSizing,
    initialState: {
      expanded: true,
    },
  })

  // Store the table instance in ref for keyboard navigation
  tableRef.current = table

  const displayedRowCount = table.getRowModel().rows.length

  const onSearchChange = useCallback((e: ChangeEvent<HTMLInputElement>) => {
    setSearchValue(e.target.value)
  }, [])

  const onClearFilters = useCallback(() => {
    setSearchValue('')
    setSorting([])
    setColumnFilters([])
  }, [])

  const hasActiveFilters =
    !!searchValue || columnFilters.length > 0 || sorting.length > 0

  const onExportCsv = useCallback(() => {
    const headers = [
      'WBS',
      'Key',
      'Name',
      'Type',
      'Status',
      'Priority',
      'Planned Start',
      'Planned End',
    ]

    const escapeCsv = (value: unknown) => {
      const str = value == null ? '' : String(value)
      const escaped = str.replace(/\"/g, '""')
      return /[\",\n\r]/.test(escaped) ? `"${escaped}"` : escaped
    }

    const rows = table.getRowModel().rows.map((row) => {
      const task = row.original
      return [
        task.wbs ?? '',
        task.taskKey ?? '',
        task.name ?? '',
        task.type?.name ?? '',
        task.status?.name ?? '',
        task.priority?.name ?? '',
        task.plannedStart ? dayjs(task.plannedStart).format('MMM D, YYYY') : '',
        task.plannedEnd ? dayjs(task.plannedEnd).format('MMM D, YYYY') : '',
      ]
    })

    const csv = [
      headers.map(escapeCsv).join(','),
      ...rows.map((r) => r.map(escapeCsv).join(',')),
    ].join('\n')

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
    const url = URL.createObjectURL(blob)

    const link = document.createElement('a')
    link.href = url
    link.download = `project-tasks-${dayjs().format('YYYY-MM-DD')}.csv`
    link.click()

    URL.revokeObjectURL(url)
  }, [table])

  return (
    <>
      <style>{cssString}</style>
      <div className="moda-project-tasks-table">
        <div className="moda-project-tasks-table__toolbar">
          <div>
            {onCreateTask && (
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={onCreateTask}
              >
                Create Task
              </Button>
            )}
          </div>

          <div className="moda-project-tasks-table__toolbar-right">
            <Text>
              {displayedRowCount} of {totalRowCount}
            </Text>
            <Input
              placeholder="Search"
              allowClear={true}
              value={searchValue}
              onChange={onSearchChange}
              suffix={<SearchOutlined />}
              className="moda-project-tasks-table__toolbar-search"
            />
            {onRefresh && (
              <Tooltip title="Refresh">
                <Button
                  type="text"
                  shape="circle"
                  icon={<ReloadOutlined />}
                  onClick={onRefresh}
                />
              </Tooltip>
            )}
            <Tooltip title="Clear Filters and Sorting">
              <Button
                type="text"
                shape="circle"
                icon={<ClearOutlined />}
                onClick={onClearFilters}
                disabled={!hasActiveFilters}
              />
            </Tooltip>
            <span className="moda-project-tasks-table__toolbar-divider" />
            <Tooltip title="Export to CSV">
              <Button
                type="text"
                shape="circle"
                icon={<DownloadOutlined />}
                onClick={onExportCsv}
                disabled={isLoading || displayedRowCount === 0}
              />
            </Tooltip>
          </div>
        </div>

        <div className="moda-project-tasks-table__table-wrapper">
          <table className="moda-project-tasks-table__table">
            <colgroup>
              {table.getVisibleLeafColumns().map((column) => (
                <col key={column.id} width={column.getSize()} />
              ))}
            </colgroup>
            <thead>
              {table.getHeaderGroups().map((headerGroup) => (
                <Fragment key={headerGroup.id}>
                  <tr key={headerGroup.id}>
                    {headerGroup.headers.map((header) => {
                      const canSort = header.column.getCanSort()
                      const sortState = header.column.getIsSorted()
                      const canResize = header.column.getCanResize()

                      const sortIcon =
                        sortState === 'asc' ? (
                          <CaretUpOutlined />
                        ) : sortState === 'desc' ? (
                          <CaretDownOutlined />
                        ) : null

                      return (
                        <th
                          key={header.id}
                          className={`moda-project-tasks-table__th${
                            canSort
                              ? ' moda-project-tasks-table__th--sortable'
                              : ''
                          }${canResize ? ' moda-project-tasks-table__th--resizable' : ''}`}
                          onClick={
                            canSort
                              ? header.column.getToggleSortingHandler()
                              : undefined
                          }
                        >
                          <span className="moda-project-tasks-table__th-content">
                            {header.isPlaceholder
                              ? null
                              : flexRender(
                                  header.column.columnDef.header,
                                  header.getContext(),
                                )}
                            {sortIcon}
                          </span>

                          {canResize && (
                            <span
                              role="separator"
                              aria-orientation="vertical"
                              onMouseDown={header.getResizeHandler()}
                              onTouchStart={header.getResizeHandler()}
                              onDoubleClick={() => header.column.resetSize()}
                              onClick={(e) => e.stopPropagation()}
                              className={`moda-project-tasks-table__resizer${
                                header.column.getIsResizing()
                                  ? ' moda-project-tasks-table__resizer--active'
                                  : ''
                              }`}
                            />
                          )}
                        </th>
                      )
                    })}
                  </tr>

                  <tr key={`${headerGroup.id}-filters`}>
                    {headerGroup.headers.map((header) => {
                      const column = header.column

                      if (!column.getCanFilter() || header.isPlaceholder) {
                        return (
                          <th
                            key={`${header.id}-filter`}
                            className="moda-project-tasks-table__filter-th"
                          />
                        )
                      }

                      const colId = column.id
                      const rawFilterValue = column.getFilterValue()
                      const textValue = (rawFilterValue ?? '') as string
                      const selectValue = (
                        Array.isArray(rawFilterValue) ? rawFilterValue : []
                      ) as string[]
                      const isSelect =
                        colId === 'type' ||
                        colId === 'status' ||
                        colId === 'priority'
                      const options =
                        colId === 'type'
                          ? typeOptions
                          : colId === 'status'
                            ? statusOptions
                            : colId === 'priority'
                              ? priorityOptions
                              : []

                      return (
                        <th
                          key={`${header.id}-filter`}
                          className="moda-project-tasks-table__filter-th"
                          onClick={(e) => e.stopPropagation()}
                        >
                          {isSelect ? (
                            <Select
                              size="small"
                              mode="multiple"
                              allowClear
                              value={
                                selectValue.length ? selectValue : undefined
                              }
                              options={options}
                              suffixIcon={<FilterOutlined />}
                              popupMatchSelectWidth={false}
                              getPopupContainer={() => document.body}
                              popupClassName="moda-project-tasks-table__filter-popup"
                              onChange={(v) =>
                                column.setFilterValue(
                                  v && v.length ? v : undefined,
                                )
                              }
                              className="moda-project-tasks-table__filter-control"
                            />
                          ) : (
                            <Input
                              size="small"
                              allowClear
                              value={textValue}
                              onChange={(e) => {
                                const next = e.target.value
                                column.setFilterValue(next ? next : undefined)
                              }}
                              className="moda-project-tasks-table__filter-control"
                            />
                          )}
                        </th>
                      )
                    })}
                  </tr>
                </Fragment>
              ))}
            </thead>
            <tbody>
              {isLoading ? (
                <tr>
                  <td
                    colSpan={columns.length}
                    className="moda-project-tasks-table__td moda-project-tasks-table__loading"
                  >
                    <Spin />
                  </td>
                </tr>
              ) : table.getRowModel().rows.length === 0 ? (
                <tr>
                  <td
                    colSpan={columns.length}
                    className="moda-project-tasks-table__td moda-project-tasks-table__empty"
                  >
                    <ModaEmpty message="No tasks found" />
                  </td>
                </tr>
              ) : (
                table.getRowModel().rows.map((row, index) => {
                  const isSelected = selectedRowId === row.original.id
                  return (
                    <tr
                      key={row.id}
                      className={`moda-project-tasks-table__tr${index % 2 === 1 ? ' moda-project-tasks-table__tr--alt' : ''}${isSelected ? ' moda-project-tasks-table__tr--selected' : ''}`}
                      onClick={() => {
                        setSelectedRowId(
                          selectedRowId === row.original.id
                            ? null
                            : row.original.id,
                        )
                      }}
                    >
                      {row.getVisibleCells().map((cell) => {
                        // Determine if this cell is editable when row is selected
                        const editableColumns = [
                          'name',
                          'type',
                          'status',
                          'priority',
                          'plannedStart',
                          'plannedEnd',
                        ]
                        const isEditableCell =
                          isSelected && editableColumns.includes(cell.column.id)

                        return (
                          <td
                            key={cell.id}
                            className={`moda-project-tasks-table__td${isEditableCell ? ' moda-project-tasks-table__editable-cell' : ''}`}
                          >
                            {flexRender(
                              cell.column.columnDef.cell,
                              cell.getContext(),
                            )}
                          </td>
                        )
                      })}
                    </tr>
                  )
                })
              )}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}

export default ProjectTasksTable
