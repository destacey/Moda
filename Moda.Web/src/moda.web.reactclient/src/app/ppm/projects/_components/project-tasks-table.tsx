'use client'

import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import { ModaEmpty } from '@/src/components/common'
import { OptionModel } from '@/src/components/types'
import {
  Button,
  DatePicker,
  Form,
  Input,
  Select,
  Space,
  Spin,
  Tag,
  Tooltip,
  Typography,
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
  createContext,
  Fragment,
  type ChangeEvent,
  useCallback,
  useContext,
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

// Create context for editable row form
interface EditableContextType {
  form: ReturnType<typeof Form.useForm>[0]
  task: ProjectTaskTreeDto
  onSave: () => Promise<boolean>
}

const EditableContext = createContext<EditableContextType | null>(null)

// Editable row component that wraps each row with its own Form
interface EditableRowProps {
  task: ProjectTaskTreeDto
  isSelected: boolean
  onSave: (
    taskId: string,
    formInstance: ReturnType<typeof Form.useForm>[0],
  ) => Promise<boolean>
  children: React.ReactNode
}

const EditableRow = ({
  task,
  isSelected,
  onSave,
  children,
}: EditableRowProps) => {
  // For now, just render children - we'll handle form in a different way
  return <>{children}</>
}

// Hook to access the editable context within cells
const useEditableContext = () => {
  const context = useContext(EditableContext)
  return context
}

interface ProjectTasksTableProps {
  tasks: ProjectTaskTreeDto[]
  isLoading: boolean
  onCreateTask?: () => void
  onEditTask?: (task: ProjectTaskTreeDto) => void
  onDeleteTask?: (task: ProjectTaskTreeDto) => void
  onRefresh?: () => void
  onUpdateTask?: (
    taskId: string,
    updates: Partial<{
      name: string
      statusId: number
      priorityId: number
      typeId: number
      plannedStart: string | null
      plannedEnd: string | null
      plannedDate: string | null
    }>,
  ) => Promise<void>
  onUpdateStatus?: (taskId: string, statusId: number) => Promise<void>
  onUpdatePriority?: (taskId: string, priorityId: number) => Promise<void>
  onUpdateName?: (taskId: string, name: string) => Promise<void>
  onUpdateType?: (taskId: string, typeId: number) => Promise<void>
  onUpdatePlannedStart?: (taskId: string, date: string | null) => Promise<void>
  onUpdatePlannedEnd?: (taskId: string, date: string | null) => Promise<void>
  taskStatusOptions?: OptionModel<number>[]
  taskPriorityOptions?: OptionModel<number>[]
  taskTypeOptions?: OptionModel<number>[]
}

const ProjectTasksTable = ({
  tasks,
  isLoading,
  onCreateTask,
  onEditTask,
  onDeleteTask,
  onRefresh,
  onUpdateTask,
  onUpdateStatus,
  onUpdatePriority,
  onUpdateName,
  onUpdateType,
  onUpdatePlannedStart,
  onUpdatePlannedEnd,
  taskStatusOptions = [],
  taskPriorityOptions = [],
  taskTypeOptions = [],
}: ProjectTasksTableProps) => {
  const [searchValue, setSearchValue] = useState('')
  const [sorting, setSorting] = useState<SortingState>([])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])
  const [columnSizing, setColumnSizing] = useState<ColumnSizingState>({})
  const [selectedRowId, setSelectedRowId] = useState<string | null>(null)
  const [selectedCellId, setSelectedCellId] = useState<string | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const [form] = Form.useForm()
  const previousSelectedRowIdRef = useRef<string | null>(null)
  const formInitialValuesRef = useRef<Record<string, any> | null>(null)
  const nameInputRef = useRef<any>(null)
  const isExitingEditModeRef = useRef(false)

  // API options are already in the correct format - use them directly
  // Stable empty array reference to avoid re-renders
  const emptyFilterArray = useMemo(() => [] as number[], [])

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
      max-width: 100%;
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
      min-width: 0;
      max-width: 100%;
      position: relative;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selector {
      min-width: 0;
      max-width: 100%;
      align-items: center;
      flex-wrap: nowrap !important;
      overflow: hidden;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selection-item {
      font-weight: normal;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
      flex-shrink: 1;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selection-overflow {
      flex-wrap: nowrap !important;
      overflow: hidden;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selection-overflow-item {
      flex-shrink: 1;
      max-width: 100%;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selection-search {
      min-width: 0;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-selection-search-input {
      font-weight: normal;
    }
    .moda-project-tasks-table__filter-control.ant-select .ant-select-clear {
      margin-right: 10px;
    }
    .moda-project-tasks-table__filter-popup {
      min-width: 220px;
    }
    .moda-project-tasks-table__td {
      padding: 1px 7px;
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
      padding: 1px 4px !important;
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
    .moda-project-tasks-table__editable-cell .ant-select-focused,
    .moda-project-tasks-table__editable-cell .ant-select-open,
    .moda-project-tasks-table__editable-cell .ant-picker-focused {
      border-color: var(--ant-color-primary) !important;
      border-width: 2px !important;
      box-shadow: 0 0 0 3px var(--ant-color-primary-bg) !important;
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

  // Helper to get the current task
  const getCurrentTask = useCallback(
    (taskId: string) => {
      const findTask = (
        items: ProjectTaskTreeDto[],
      ): ProjectTaskTreeDto | null => {
        for (const item of items) {
          if (item.id === taskId) return item
          if (item.children?.length) {
            const found = findTask(item.children)
            if (found) return found
          }
        }
        return null
      }
      return findTask(tasks)
    },
    [tasks],
  )

  // Handle row selection changes - save previous row and initialize new row
  useEffect(() => {
    const handleRowChange = async () => {
      // If we're exiting edit mode, skip the save (it was already done in the exit handler)
      if (isExitingEditModeRef.current) {
        isExitingEditModeRef.current = false
        previousSelectedRowIdRef.current = selectedRowId
        return
      }

      // If we had a previously selected row and it's different from current
      // But only save if we're moving to another row, not if we're exiting edit mode (selectedRowId === null)
      if (
        previousSelectedRowIdRef.current &&
        previousSelectedRowIdRef.current !== selectedRowId &&
        selectedRowId !== null
      ) {
        await saveFormChanges(previousSelectedRowIdRef.current, form)
      }

      // Initialize form for newly selected row
      if (selectedRowId) {
        const task = getCurrentTask(selectedRowId)
        if (task) {
          // Reset form first to prevent visual artifacts
          form.resetFields()
          const initialValues = {
            name: task.name,
            typeId: task.type?.id,
            statusId: task.status?.id,
            priorityId: task.priority?.id,
            plannedStart: task.plannedStart ? dayjs(task.plannedStart) : null,
            plannedEnd: task.plannedEnd ? dayjs(task.plannedEnd) : null,
            plannedDate: task.plannedDate ? dayjs(task.plannedDate) : null,
          }
          form.setFieldsValue(initialValues)
          // Store initial values to compare later
          formInitialValuesRef.current = initialValues
          // Focus the selected cell (or name field if no specific cell selected)
          setTimeout(() => {
            if (selectedCellId) {
              const cellElement = document.querySelector(
                `[data-cell-id="${selectedCellId}"]`,
              )
              if (cellElement) {
                const input = cellElement.querySelector('input') as HTMLElement
                if (input) {
                  input.focus()
                  if (input instanceof HTMLInputElement) {
                    input.select()
                  }
                }
              }
            } else {
              nameInputRef.current?.focus()
            }
          }, 0)
        }
      } else {
        form.resetFields()
        formInitialValuesRef.current = null
      }

      previousSelectedRowIdRef.current = selectedRowId
    }

    handleRowChange()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedRowId])

  // Handle click outside table to save changes and exit edit mode
  useEffect(() => {
    if (!selectedRowId) return

    const handleClickOutside = async (event: MouseEvent) => {
      const target = event.target as HTMLElement

      // Check if click is on a dropdown/picker - if so, ignore
      if (
        target.closest('.ant-select-dropdown') ||
        target.closest('.ant-picker-dropdown')
      ) {
        return
      }

      // Check if click is on the actual table rows - if not, save and exit
      if (!target.closest('.moda-project-tasks-table__table-wrapper')) {
        await saveFormChanges(selectedRowId, form)
        setSelectedRowId(null)
        setSelectedCellId(null)
      }
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedRowId])

  // Save form changes - now works with EditableRow's form via ref passed from keyboard handlers
  const saveFormChanges = useCallback(
    async (
      taskId: string,
      formInstance: ReturnType<typeof Form.useForm>[0],
    ) => {
      try {
        // Validate form
        await formInstance.validateFields()

        const values = formInstance.getFieldsValue() as {
          name: string
          typeId: number
          statusId: number
          priorityId: number
          plannedStart: dayjs.Dayjs | null
          plannedEnd: dayjs.Dayjs | null
          plannedDate: dayjs.Dayjs | null
        }

        const task = getCurrentTask(taskId)
        if (!task) {
          return false
        }

        // Check if form has changes by comparing with initial values
        const initialValues = formInitialValuesRef.current
        if (!initialValues) {
          return true
        }

        // Helper to check if a field has changed
        const hasChanged = (fieldName: keyof typeof initialValues) => {
          const initial = initialValues[fieldName]
          const current = values[fieldName]

          // Handle dayjs comparison
          if (
            initial &&
            current &&
            typeof initial === 'object' &&
            'isSame' in initial
          ) {
            return !initial.isSame(current as any)
          }

          // Handle null/undefined
          if (initial === null && current === null) return false
          if (initial === undefined && current === undefined) return false
          if (initial === null && current === undefined) return false
          if (initial === undefined && current === null) return false

          return initial !== current
        }

        let hasAnyChanges = false
        setIsSaving(true)

        // Prefer batch update if available to avoid race conditions
        if (onUpdateTask) {
          const updates: Record<string, any> = {}

          if (hasChanged('name')) {
            updates.name = values.name
            hasAnyChanges = true
          }
          if (hasChanged('statusId')) {
            updates.statusId = values.statusId
            hasAnyChanges = true
          }
          if (hasChanged('priorityId')) {
            updates.priorityId = values.priorityId
            hasAnyChanges = true
          }
          if (hasChanged('typeId')) {
            updates.typeId = values.typeId
            hasAnyChanges = true
          }
          if (hasChanged('plannedStart')) {
            const dateValue = values.plannedStart
              ? values.plannedStart.format('YYYY-MM-DD')
              : null
            updates.plannedStart = dateValue
            hasAnyChanges = true
          }
          if (hasChanged('plannedEnd')) {
            const dateValue = values.plannedEnd
              ? values.plannedEnd.format('YYYY-MM-DD')
              : null
            updates.plannedEnd = dateValue
            hasAnyChanges = true
          }
          if (hasChanged('plannedDate')) {
            const dateValue = values.plannedDate
              ? values.plannedDate.format('YYYY-MM-DD')
              : null
            updates.plannedDate = dateValue
            hasAnyChanges = true
          }

          if (hasAnyChanges) {
            await onUpdateTask(taskId, updates)
          }
        } else {
          // Fallback to individual updates (old behavior)
          // Call update handlers for changed fields
          if (hasChanged('name') && onUpdateName) {
            await onUpdateName(taskId, values.name)
            hasAnyChanges = true
          }
          if (hasChanged('statusId') && onUpdateStatus) {
            await onUpdateStatus(taskId, values.statusId)
            hasAnyChanges = true
          }
          if (hasChanged('priorityId') && onUpdatePriority) {
            await onUpdatePriority(taskId, values.priorityId)
            hasAnyChanges = true
          }
          if (hasChanged('typeId') && onUpdateType) {
            await onUpdateType(taskId, values.typeId)
            hasAnyChanges = true
          }
          if (hasChanged('plannedStart') && onUpdatePlannedStart) {
            const dateValue = values.plannedStart
              ? values.plannedStart.format('YYYY-MM-DD')
              : null
            await onUpdatePlannedStart(taskId, dateValue)
            hasAnyChanges = true
          }
          if (hasChanged('plannedDate') && onUpdatePlannedStart) {
            const dateValue = values.plannedDate
              ? values.plannedDate.format('YYYY-MM-DD')
              : null
            await onUpdatePlannedStart(taskId, dateValue)
            hasAnyChanges = true
          }
          if (hasChanged('plannedEnd') && onUpdatePlannedEnd) {
            const dateValue = values.plannedEnd
              ? values.plannedEnd.format('YYYY-MM-DD')
              : null
            await onUpdatePlannedEnd(taskId, dateValue)
            hasAnyChanges = true
          }
        }

        setIsSaving(false)
        return true
      } catch (error) {
        console.error('Validation or save failed:', error)
        setIsSaving(false)
        return false
      }
    },
    [
      getCurrentTask,
      onUpdateTask,
      onUpdateName,
      onUpdateStatus,
      onUpdatePriority,
      onUpdateType,
      onUpdatePlannedStart,
      onUpdatePlannedEnd,
    ],
  )

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

    const handleGlobalKeyDown = async (e: KeyboardEvent) => {
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
        // Move to next cell - auto-save will trigger in EditableRow if row changes
        setSelectedRowId(nextRowId)
        setSelectedCellId(`${nextRowId}-${nextColId}`)

        setTimeout(() => {
          const nextCell = document.querySelector(
            `[data-cell-id="${nextRowId}-${nextColId}"]`,
          )
          if (nextCell) {
            const input = nextCell.querySelector(
              'input, .ant-select, .ant-picker',
            )
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
  }, [selectedRowId, editableColumns, saveFormChanges])

  // Handle keyboard navigation
  const handleKeyDown = useCallback(
    async (e: React.KeyboardEvent, rowId: string, columnId: string) => {
      if (!selectedRowId || !tableRef.current) return

      const rows = tableRef.current.getRowModel().rows
      const currentRowIndex = rows.findIndex(
        (r: any) => r.original.id === rowId,
      )
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
          // Reset form to original values and exit edit mode
          if (formInitialValuesRef.current) {
            form.setFieldsValue(formInitialValuesRef.current)
          }
          setSelectedRowId(null)
          setSelectedCellId(null)
          return

        case 'Tab':
          e.preventDefault()
          e.stopPropagation()

          // Helper to find next available field (skips fields that don't exist in DOM)
          const findNextAvailableField = (
            startRowId: string,
            startColIndex: number,
            direction: 'forward' | 'backward',
          ): { rowId: string; colId: string } | null => {
            let currentRowIdx = rows.findIndex(
              (r: any) => r.original.id === startRowId,
            )
            let currentColIdx = startColIndex

            while (currentRowIdx >= 0 && currentRowIdx < rows.length) {
              // Try fields in current row
              while (
                direction === 'forward'
                  ? currentColIdx < editableColumns.length
                  : currentColIdx >= 0
              ) {
                const testRowId = rows[currentRowIdx].original.id
                const testColId = editableColumns[currentColIdx]
                const cellExists = document.querySelector(
                  `[data-cell-id="${testRowId}-${testColId}"]`,
                )

                if (cellExists) {
                  return { rowId: testRowId, colId: testColId }
                }

                currentColIdx += direction === 'forward' ? 1 : -1
              }

              // Move to next/previous row
              currentRowIdx += direction === 'forward' ? 1 : -1
              currentColIdx =
                direction === 'forward' ? 0 : editableColumns.length - 1
            }

            return null
          }

          // Move to next field (or previous if Shift+Tab)
          if (e.shiftKey) {
            // Try previous field in same row first
            const result = findNextAvailableField(
              rowId,
              currentColIndex - 1,
              'backward',
            )
            if (result) {
              nextRowId = result.rowId
              nextColId = result.colId
            } else {
              // No previous field found - exit edit mode
              isExitingEditModeRef.current = true
              setSelectedRowId(null)
              setSelectedCellId(null)
              return
            }
          } else {
            // Try next field in same row first
            const result = findNextAvailableField(
              rowId,
              currentColIndex + 1,
              'forward',
            )
            if (result) {
              nextRowId = result.rowId
              nextColId = result.colId
            } else {
              // No next field found - exit edit mode and save current row
              await saveFormChanges(rowId, form)
              isExitingEditModeRef.current = true
              setSelectedRowId(null)
              setSelectedCellId(null)
              return
            }
          }
          break
      }

      if (nextRowId && nextColId) {
        // TODO: Save current row before moving to a different row
        // This needs to be refactored to work with EditableRow context
        setSelectedRowId(nextRowId)
        setSelectedCellId(`${nextRowId}-${nextColId}`)

        // Focus the next cell after a short delay to allow rendering
        setTimeout(() => {
          const cellId = `${nextRowId}-${nextColId}`
          const nextCell = document.querySelector(`[data-cell-id="${cellId}"]`)
          if (nextCell) {
            // Try to find the actual focusable input element
            // For Select/DatePicker, look for the input inside first
            let input = nextCell.querySelector('input') as HTMLElement

            // If no input found, try the Select/Picker wrapper
            if (!input) {
              input = nextCell.querySelector(
                '.ant-select, .ant-picker',
              ) as HTMLElement
            }

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
    [selectedRowId, editableColumns, form, saveFormChanges],
  )

  const columns = useMemo<ColumnDef<ProjectTaskTreeDto>[]>(
    () => [
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
                  onClick={row.getToggleExpandedHandler()}
                  className="moda-project-tasks-table__expander-btn"
                />
              ) : (
                <span style={{ width: 24, display: 'inline-block' }} />
              )}
              {isSelected && onUpdateName ? (
                <Form.Item
                  name="name"
                  style={{ margin: 0, flex: 1, minWidth: 0 }}
                  rules={[
                    { required: true, message: 'Name is required' },
                    { max: 256, message: 'Name cannot exceed 256 characters' },
                  ]}
                >
                  <Input
                    ref={nameInputRef}
                    size="small"
                    variant="borderless"
                    onPressEnter={(e) => {
                      e.currentTarget.blur()
                    }}
                    onKeyDown={(e) => handleKeyDown(e, task.id, 'name')}
                    onClick={(e) => e.stopPropagation()}
                    style={{ flex: 1, minWidth: 0 }}
                  />
                </Form.Item>
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
          const typeId = row.original.type?.id
          if (filterValue == null || typeId == null) return true
          if (Array.isArray(filterValue)) {
            return filterValue.length === 0 || filterValue.includes(typeId)
          }
          return filterValue === typeId
        },
        cell: (info) => {
          const task = info.row.original
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-type`

          if (!isSelected || !onUpdateType) {
            return info.getValue()
          }

          return (
            <div data-cell-id={cellId}>
              <Form.Item
                name="typeId"
                style={{ margin: 0 }}
                rules={[{ required: true, message: 'Type is required' }]}
              >
                <Select
                  size="small"
                  options={taskTypeOptions}
                  style={{ width: '100%' }}
                  variant="borderless"
                  onClick={(e) => e.stopPropagation()}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'type')}
                />
              </Form.Item>
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
          const statusId = row.original.status?.id
          if (filterValue == null || statusId == null) return true
          if (Array.isArray(filterValue)) {
            return filterValue.length === 0 || filterValue.includes(statusId)
          }
          return filterValue === statusId
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

          // Filter out "In Progress" for milestones
          const isMilestone = task.type?.name === 'Milestone'
          const availableStatusOptions = isMilestone
            ? taskStatusOptions.filter((opt) => opt.label !== 'In Progress')
            : taskStatusOptions

          return (
            <div data-cell-id={cellId}>
              <Form.Item
                name="statusId"
                style={{ margin: 0 }}
                rules={[{ required: true, message: 'Status is required' }]}
              >
                <Select
                  size="small"
                  options={availableStatusOptions}
                  style={{ width: '100%' }}
                  variant="borderless"
                  onClick={(e) => e.stopPropagation()}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'status')}
                />
              </Form.Item>
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
          const priorityId = row.original.priority?.id
          if (filterValue == null || priorityId == null) return true
          if (Array.isArray(filterValue)) {
            return filterValue.length === 0 || filterValue.includes(priorityId)
          }
          return filterValue === priorityId
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

          return (
            <div data-cell-id={cellId}>
              <Form.Item
                name="priorityId"
                style={{ margin: 0 }}
                rules={[{ required: true, message: 'Priority is required' }]}
              >
                <Select
                  size="small"
                  options={taskPriorityOptions}
                  style={{ width: '100%' }}
                  variant="borderless"
                  onClick={(e) => e.stopPropagation()}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'priority')}
                />
              </Form.Item>
            </div>
          )
        },
      },
      {
        id: 'plannedStart',
        accessorFn: (row) => {
          const isMilestone = row.type?.name === 'Milestone'
          const dateValue = isMilestone ? row.plannedDate : row.plannedStart
          return dateValue ? dayjs(dateValue).format('MMM D, YYYY') : ''
        },
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
          const isMilestone = task.type?.name === 'Milestone'

          if (!isSelected || !onUpdatePlannedStart) {
            return value || '-'
          }

          const fieldName = isMilestone ? 'plannedDate' : 'plannedStart'

          return (
            <div data-cell-id={cellId}>
              <Form.Item name={fieldName} style={{ margin: 0 }}>
                <DatePicker
                  size="small"
                  variant="borderless"
                  format="MMM D, YYYY"
                  onClick={(e) => e.stopPropagation()}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'plannedStart')}
                  style={{ width: '100%' }}
                  placeholder="-"
                />
              </Form.Item>
            </div>
          )
        },
        sortingFn: (a, b) => {
          const aIsMilestone = a.original.type?.name === 'Milestone'
          const bIsMilestone = b.original.type?.name === 'Milestone'
          const av = (
            aIsMilestone ? a.original.plannedDate : a.original.plannedStart
          )
            ? dayjs(
                aIsMilestone ? a.original.plannedDate : a.original.plannedStart,
              ).valueOf()
            : -Infinity
          const bv = (
            bIsMilestone ? b.original.plannedDate : b.original.plannedStart
          )
            ? dayjs(
                bIsMilestone ? b.original.plannedDate : b.original.plannedStart,
              ).valueOf()
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
          const isMilestone = task.type?.name === 'Milestone'

          if (!isSelected || !onUpdatePlannedEnd || isMilestone) {
            return value || '-'
          }

          return (
            <div data-cell-id={cellId}>
              <Form.Item name="plannedEnd" style={{ margin: 0 }}>
                <DatePicker
                  size="small"
                  variant="borderless"
                  format="MMM D, YYYY"
                  onClick={(e) => e.stopPropagation()}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'plannedEnd')}
                  style={{ width: '100%' }}
                  placeholder="-"
                />
              </Form.Item>
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
                  tabIndex={-1}
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
                  tabIndex={-1}
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
      taskStatusOptions,
      taskPriorityOptions,
      taskTypeOptions,
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
      const isMilestone = task.type?.name === 'Milestone'
      const plannedStartDate = isMilestone
        ? task.plannedDate
        : task.plannedStart

      return [
        task.wbs ?? '',
        task.taskKey ?? '',
        task.name ?? '',
        task.type?.name ?? '',
        task.status?.name ?? '',
        task.priority?.name ?? '',
        plannedStartDate ? dayjs(plannedStartDate).format('MMM D, YYYY') : '',
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
      <Form form={form} component={false}>
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
                        const isSelect =
                          colId === 'type' ||
                          colId === 'status' ||
                          colId === 'priority'
                        const selectValue = (
                          Array.isArray(rawFilterValue)
                            ? rawFilterValue
                            : emptyFilterArray
                        ) as number[]
                        const options =
                          colId === 'type'
                            ? taskTypeOptions
                            : colId === 'status'
                              ? taskStatusOptions
                              : colId === 'priority'
                                ? taskPriorityOptions
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
                                maxTagCount={0}
                                maxTagPlaceholder={(values) => {
                                  const labels = values
                                    .map((v) => options.find((o) => o.value === v.value)?.label)
                                    .filter(Boolean)
                                  return labels.length === 1 ? labels[0] : `${labels.length} selected`
                                }}
                                value={
                                  selectValue.length ? selectValue : undefined
                                }
                                options={options}
                                suffixIcon={<FilterOutlined />}
                                popupMatchSelectWidth={false}
                                classNames={{
                                  popup: {
                                    root: 'moda-project-tasks-table__filter-popup',
                                  },
                                }}
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
                        onClick={(e) => {
                          // Ignore clicks from form inputs/controls (they handle their own interactions)
                          const target = e.target as HTMLElement
                          if (
                            target.closest('.ant-select-dropdown') ||
                            target.closest('.ant-picker-dropdown') ||
                            target.closest('input') ||
                            target.closest('.ant-select-selector') ||
                            target.classList.contains(
                              'ant-select-item-option-content',
                            )
                          ) {
                            return
                          }

                          // Ignore clicks on buttons (expander, edit, delete) - they handle their own actions
                          if (
                            target.closest('button') ||
                            target.closest('.ant-btn')
                          ) {
                            return
                          }

                          // Find which cell was clicked
                          const cellElement = target.closest('td')
                          const clickedColumnId =
                            cellElement?.getAttribute('data-column-id')

                          // Check if clicked column is editable
                          const editableColumns = [
                            'name',
                            'type',
                            'status',
                            'priority',
                            'plannedStart',
                            'plannedEnd',
                          ]
                          const isEditableColumn =
                            clickedColumnId &&
                            editableColumns.includes(clickedColumnId)

                          if (selectedRowId === row.original.id) {
                            // Clicking same row - toggle selection
                            setSelectedRowId(null)
                            setSelectedCellId(null)
                          } else {
                            // Clicking different row - select it
                            setSelectedRowId(row.original.id)
                            // Set cell ID if an editable column was clicked, otherwise default to name
                            if (isEditableColumn) {
                              setSelectedCellId(
                                `${row.original.id}-${clickedColumnId}`,
                              )
                            } else {
                              setSelectedCellId(`${row.original.id}-name`)
                            }
                          }
                        }}
                      >
                        {row.getVisibleCells().map((cell) => {
                          // Determine if this cell is editable when row is selected
                          const editableCells = [
                            'name',
                            'type',
                            'status',
                            'priority',
                            'plannedStart',
                            'plannedEnd',
                          ]
                          const isEditableCell =
                            isSelected && editableCells.includes(cell.column.id)

                          return (
                            <td
                              key={cell.id}
                              data-column-id={cell.column.id}
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
      </Form>
    </>
  )
}

export default ProjectTasksTable
