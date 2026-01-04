'use client'

import styles from './project-tasks-table.module.css'
import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import { ModaEmpty } from '@/src/components/common'
import {
  Button,
  DatePicker,
  Descriptions,
  Flex,
  Form,
  Input,
  Popover,
  Select,
  Space,
  Spin,
  Tag,
  Tooltip,
  Typography,
  theme,
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
  QuestionCircleOutlined,
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
import { generateCsv, downloadCsvWithTimestamp } from '@/src/utils/csv-utils'

import { useMessage } from '@/src/components/contexts/messaging'
import {
  useGetTaskPriorityOptionsQuery,
  useGetTaskStatusOptionsQuery,
  useGetTaskTypeOptionsQuery,
  usePatchProjectTaskMutation,
} from '@/src/store/features/ppm/project-tasks-api'
import {
  CreateProjectTaskForm,
  DeleteProjectTaskForm,
  EditProjectTaskForm,
} from '.'

const { Text } = Typography
const { Item: DescriptionItem } = Descriptions
const { Item: FormItem } = Form

// Keyboard shortcuts help content
const KeyboardShortcutsContent = () => (
  <Flex vertical gap="large" style={{ width: 400 }}>
    <Descriptions
      size="small"
      column={1}
      styles={{ header: { marginBottom: 4 } }}
      title="Inline Editing"
    >
      <DescriptionItem label="Click row or cell">
        <Text>Enter edit mode</Text>
      </DescriptionItem>
      <DescriptionItem label="Enter / ↓">
        <Text>Save and move to next row</Text>
      </DescriptionItem>
      <DescriptionItem label="↑">
        <Text>Save and move to previous row</Text>
      </DescriptionItem>
      <DescriptionItem label="Tab">
        <Text>Move to next field (wraps to next row)</Text>
      </DescriptionItem>
      <DescriptionItem label="Shift + Tab">
        <Text>Move to previous field (wraps to previous row)</Text>
      </DescriptionItem>
      <DescriptionItem label="Esc">
        <Text>Cancel changes and exit edit mode</Text>
      </DescriptionItem>
      <DescriptionItem label="Click outside table">
        <Text>Save changes and exit edit mode</Text>
      </DescriptionItem>
    </Descriptions>
    <Descriptions
      size="small"
      column={1}
      title="Dropdown Navigation"
      styles={{ header: { marginBottom: 4 } }}
    >
      <DescriptionItem label="Space">
        <Text>Open dropdown</Text>
      </DescriptionItem>
      <DescriptionItem label="↑ / ↓">
        <Text>Navigate options in open dropdown</Text>
      </DescriptionItem>
      <DescriptionItem label="Enter">
        <Text>Select highlighted option</Text>
      </DescriptionItem>
    </Descriptions>
  </Flex>
)

interface ProjectTasksTableProps {
  projectKey: string
  tasks: ProjectTaskTreeDto[]
  isLoading: boolean
  canManageTasks: boolean
  refetch: () => Promise<any>
}

const ProjectTasksTable = ({
  projectKey,
  tasks = [],
  isLoading,
  canManageTasks,
  refetch,
}: ProjectTasksTableProps) => {
  const [searchValue, setSearchValue] = useState('')
  const [sorting, setSorting] = useState<SortingState>([
    { id: 'wbs', desc: false },
  ])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])
  const [columnSizing, setColumnSizing] = useState<ColumnSizingState>({})
  const [selectedRowId, setSelectedRowId] = useState<string | null>(null)
  const [selectedCellId, setSelectedCellId] = useState<string | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const [form] = Form.useForm()
  const tableRef = useRef<any>(null)
  const isInitializingRef = useRef(false)
  const lastFocusedCellRef = useRef<string | null>(null)
  const clickedInTableRef = useRef(false)

  const [openCreateTaskForm, setOpenCreateTaskForm] = useState<boolean>(false)
  const [openEditTaskForm, setOpenEditTaskForm] = useState<boolean>(false)
  const [openDeleteTaskForm, setOpenDeleteTaskForm] = useState<boolean>(false)
  const [selectedTaskId, setSelectedTaskId] = useState<string | undefined>(
    undefined,
  )
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})

  const { token } = theme.useToken()
  const messageApi = useMessage()

  const { data: taskStatusOptions = [] } = useGetTaskStatusOptionsQuery()
  const { data: taskStatusOptionsForMilestone = [] } =
    useGetTaskStatusOptionsQuery({ forMilestone: true })

  const { data: taskPriorityOptions = [] } = useGetTaskPriorityOptionsQuery()

  const { data: taskTypeOptions = [] } = useGetTaskTypeOptionsQuery()

  const [patchProjectTask] = usePatchProjectTaskMutation()

  // Task form handlers
  const handleCreateTask = useCallback(() => {
    setOpenCreateTaskForm(true)
  }, [])

  const handleEditTask = useCallback((task: any) => {
    setSelectedRowId(null) // Exit inline edit mode
    setSelectedTaskId(task.id)
    setOpenEditTaskForm(true)
  }, [])

  const handleDeleteTask = useCallback((task: any) => {
    setSelectedRowId(null) // Exit inline edit mode
    setSelectedTaskId(task.id)
    setOpenDeleteTaskForm(true)
  }, [])

  const handleUpdateTask = useCallback(
    async (taskId: string, updates: Partial<any>) => {
      if (!projectKey) return

      // Find the task in the tree
      const findTask = (tasks: any[], id: string): any => {
        for (const task of tasks) {
          if (task.id === id) return task
          if (task.children?.length) {
            const found = findTask(task.children, id)
            if (found) return found
          }
        }
        return null
      }

      const task = findTask(tasks || [], taskId)
      if (!task) return

      try {
        // Build RFC 6902 JSON Patch operations for only changed fields
        // Note: ASP.NET Core JsonPatch expects PascalCase paths matching the C# property names
        const patchOperations: Array<{
          op: 'replace'
          path: string
          value: any
        }> = []

        if (updates.name !== undefined) {
          patchOperations.push({ op: 'replace', path: '/Name', value: updates.name })
        }
        if (updates.description !== undefined) {
          patchOperations.push({ op: 'replace', path: '/Description', value: updates.description })
        }
        if (updates.statusId !== undefined) {
          patchOperations.push({ op: 'replace', path: '/StatusId', value: updates.statusId })
        }
        if (updates.priorityId !== undefined) {
          patchOperations.push({ op: 'replace', path: '/PriorityId', value: updates.priorityId })
        }
        if (updates.progress !== undefined) {
          patchOperations.push({ op: 'replace', path: '/Progress', value: updates.progress })
        }
        if (updates.plannedStart !== undefined) {
          patchOperations.push({ op: 'replace', path: '/PlannedStart', value: updates.plannedStart })
        }
        if (updates.plannedEnd !== undefined) {
          patchOperations.push({ op: 'replace', path: '/PlannedEnd', value: updates.plannedEnd })
        }
        if (updates.plannedDate !== undefined) {
          patchOperations.push({ op: 'replace', path: '/PlannedDate', value: updates.plannedDate })
        }
        if (updates.estimatedEffortHours !== undefined) {
          patchOperations.push({ op: 'replace', path: '/EstimatedEffortHours', value: updates.estimatedEffortHours })
        }

        const response = await patchProjectTask({
          projectIdOrKey: projectKey,
          taskId: taskId,
          patchOperations,
          cacheKey: taskId,
        })
        if (response.error) {
          throw response.error
        }
        // Refetch to ensure UI has latest data before navigation
        // DO NOT REMOVE THE AWAIT - the UI will show stale data in between api calls
        await refetch()
        return true
      } catch (error: any) {
        if (error?.status === 422 && error?.errors) {
          const errorMap: Record<string, string> = {}
          const errorFields: string[] = []
          Object.entries(error.errors).forEach(([key, messages]) => {
            const fieldName = key.charAt(0).toLowerCase() + key.slice(1)
            errorMap[fieldName] = Array.isArray(messages)
              ? messages[0]
              : messages
            errorFields.push(fieldName)
          })
          setFieldErrors(errorMap)

          // Focus on the first mappable error field, or first editable column if none map
          setTimeout(() => {
            let focused = false

            // Try each error field in order
            for (const errorField of errorFields) {
              const columnId =
                errorField === 'plannedDate'
                  ? 'plannedStart'
                  : errorField.replace(/Id$/, '')

              const cellElement = document.querySelector(
                `[data-cell-id="${taskId}-${columnId}"]`,
              )
              if (cellElement) {
                const input = cellElement.querySelector(
                  'input, .ant-select',
                ) as HTMLElement
                if (input) {
                  input.focus()
                  focused = true
                  break
                }
              }
            }

            // If no error field could be mapped, focus first editable column
            if (!focused) {
              const cellElement = document.querySelector(
                `[data-cell-id="${taskId}-name"]`,
              )
              if (cellElement) {
                const input = cellElement.querySelector('input') as HTMLElement
                if (input) {
                  input.focus()
                }
              }
            }
          }, 0)

          messageApi.error('Correct the validation error(s) to continue.')
          return false
        } else {
          messageApi.error(
            error?.detail ??
              'An error occurred while updating the project task. Please try again.',
          )
        }
        return false
      }
    },
    [messageApi, projectKey, refetch, tasks, patchProjectTask],
  )

  const onCreateTaskFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCreateTaskForm(false)
      if (wasSaved) {
        refetch()
      }
    },
    [refetch],
  )

  const onEditTaskFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditTaskForm(false)
      setSelectedTaskId(undefined)
      if (wasSaved) {
        refetch()
      }
    },
    [refetch],
  )

  const onDeleteTaskFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteTaskForm(false)
      setSelectedTaskId(undefined)
      if (wasDeleted) {
        refetch()
      }
    },
    [refetch],
  )

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

  // Helper to get field errors
  const getFieldError = useCallback(
    (fieldName: string): string | undefined => {
      return fieldErrors[fieldName]
    },
    [fieldErrors],
  )

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

  // Save form changes - defined early so it can be used in effects
  const saveFormChanges = useCallback(
    async (
      taskId: string,
      formInstance: ReturnType<typeof Form.useForm>[0],
    ) => {
      try {
        // Validate form
        await formInstance.validateFields()

        const task = getCurrentTask(taskId)
        if (!task) {
          return false
        }

        if (!handleUpdateTask) {
          return true
        }

        // Get current form values
        const values = formInstance.getFieldsValue() as any

        // Detect changes by comparing to current task
        const updates: Record<string, any> = {}
        let hasChanges = false

        // Check simple fields
        if (values.name !== task.name) {
          updates.name = values.name
          hasChanges = true
        }
        if (values.statusId !== task.status?.id) {
          updates.statusId = values.statusId
          hasChanges = true
        }
        if (values.priorityId !== task.priority?.id) {
          updates.priorityId = values.priorityId
          hasChanges = true
        }

        // Check date fields
        const taskPlannedStart = task.plannedStart
          ? String(task.plannedStart).split('T')[0]
          : null
        const plannedStartFormatted = values.plannedStart
          ? values.plannedStart.format('YYYY-MM-DD')
          : null
        if (plannedStartFormatted !== taskPlannedStart) {
          updates.plannedStart = plannedStartFormatted
          hasChanges = true
        }

        const taskPlannedEnd = task.plannedEnd
          ? String(task.plannedEnd).split('T')[0]
          : null
        const plannedEndFormatted = values.plannedEnd
          ? values.plannedEnd.format('YYYY-MM-DD')
          : null
        if (plannedEndFormatted !== taskPlannedEnd) {
          updates.plannedEnd = plannedEndFormatted
          hasChanges = true
        }

        const taskPlannedDate = task.plannedDate
          ? String(task.plannedDate).split('T')[0]
          : null
        const plannedDateFormatted = values.plannedDate
          ? values.plannedDate.format('YYYY-MM-DD')
          : null
        if (plannedDateFormatted !== taskPlannedDate) {
          updates.plannedDate = plannedDateFormatted
          hasChanges = true
        }

        if (values.progress !== task.progress) {
          updates.progress = values.progress
          hasChanges = true
        }

        if (values.estimatedEffortHours !== task.estimatedEffortHours) {
          updates.estimatedEffortHours = values.estimatedEffortHours
            ? Number(values.estimatedEffortHours)
            : null
          hasChanges = true
        }

        if (!hasChanges) {
          return true
        }

        // Send update
        setIsSaving(true)
        const success = await handleUpdateTask(taskId, updates)
        setIsSaving(false)
        return success
      } catch (error) {
        setIsSaving(false)
        return false
      }
    },

    [getCurrentTask, handleUpdateTask],
  )

  // Initialize form when row selection changes
  useEffect(() => {
    isInitializingRef.current = true

    if (selectedRowId) {
      const task = getCurrentTask(selectedRowId)
      if (task) {
        form.resetFields()
        setFieldErrors({})
        const initialValues = {
          name: task.name,
          typeId: task.type?.id,
          statusId: task.status?.id,
          priorityId: task.priority?.id,
          progress: task.progress,
          plannedStart: task.plannedStart ? dayjs(task.plannedStart) : null,
          plannedEnd: task.plannedEnd ? dayjs(task.plannedEnd) : null,
          plannedDate: task.plannedDate ? dayjs(task.plannedDate) : null,
          estimatedEffortHours: task.estimatedEffortHours,
        }
        form.setFieldsValue(initialValues)

        // Allow change tracking after form is set
        isInitializingRef.current = false
      }
    } else {
      form.resetFields()
      isInitializingRef.current = false
    }
    // Clear cell focus tracking when row changes
    lastFocusedCellRef.current = null
    // Only re-run when selectedRowId changes (cell focus is separate)
  }, [form, getCurrentTask, selectedRowId])

  // Handle cell focusing separately - only focus when cell ID changes
  useEffect(() => {
    if (!selectedRowId || !selectedCellId) return

    // Only focus if we've moved to a different cell
    if (lastFocusedCellRef.current === selectedCellId) return

    lastFocusedCellRef.current = selectedCellId

    const timeout = setTimeout(() => {
      // First try finding by data-cell-id on td
      let cellElement = document.querySelector(
        `td[data-cell-id="${selectedCellId}"]`,
      )

      // If not found, search within the row
      if (!cellElement) {
        const rows = document.querySelectorAll('tr')
        for (const row of rows) {
          const cell = row.querySelector(`[data-cell-id="${selectedCellId}"]`)
          if (cell) {
            cellElement = cell
            break
          }
        }
      }

      if (cellElement) {
        // Try to find input, select, or date picker
        let input = cellElement.querySelector('input') as HTMLElement

        if (!input) {
          input = cellElement.querySelector(
            '.ant-select, .ant-picker',
          ) as HTMLElement
        }

        if (input) {
          input.focus()
          if (input instanceof HTMLInputElement) {
            input.select()
          }
        }
      }
    }, 10)

    return () => clearTimeout(timeout)
  }, [selectedRowId, selectedCellId])

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

      // Check if click was in table via ref flag or if focused element is in table
      const focusedInTable = document.activeElement?.closest(
        `.${styles.tableWrapper}`,
      )
      if (!clickedInTableRef.current && !focusedInTable) {
        await saveFormChanges(selectedRowId, form)
        setSelectedRowId(null)
        setSelectedCellId(null)
      }
      clickedInTableRef.current = false
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [selectedRowId, form, saveFormChanges])

  // Editable columns in order
  const editableColumns = useMemo(
    () => [
      'name',
      'status',
      'priority',
      'plannedStart',
      'plannedEnd',
      'progress',
      'estimatedEffortHours',
    ],
    [],
  )

  // Global keyboard handler for navigation when row is selected
  useEffect(() => {
    if (!selectedRowId || !tableRef.current) return

    const handleGlobalKeyDown = async (e: KeyboardEvent) => {
      // Don't intercept if saving
      if (isSaving) return

      const activeElement = document.activeElement
      const currentCellElement = activeElement?.closest('[data-cell-id]')

      // Handle Tab navigation when inside a cell
      if (e.key === 'Tab' && currentCellElement) {
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
          // Move to next cell
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
        return
      }

      // Handle Enter/Arrow navigation when row is selected but no cell is focused
      if (
        (e.key === 'Enter' || e.key === 'ArrowUp' || e.key === 'ArrowDown') &&
        !currentCellElement
      ) {
        // Don't intercept if dropdown is open
        if (
          document.querySelector(
            '.ant-select-dropdown:not(.ant-select-dropdown-hidden), .ant-picker-dropdown:not(.ant-picker-dropdown-hidden)',
          )
        ) {
          return
        }

        e.preventDefault()

        const rows = tableRef.current.getRowModel().rows
        const currentRowIndex = rows.findIndex(
          (r: any) => r.original.id === selectedRowId,
        )
        if (currentRowIndex === -1) return

        let targetRowId: string | null = null

        if (e.key === 'Enter' || e.key === 'ArrowDown') {
          // Navigate down
          if (currentRowIndex < rows.length - 1) {
            targetRowId = rows[currentRowIndex + 1].original.id
          }
        } else if (e.key === 'ArrowUp') {
          // Navigate up
          if (currentRowIndex > 0) {
            targetRowId = rows[currentRowIndex - 1].original.id
          }
        }

        if (targetRowId) {
          // Save current row before navigating
          await saveFormChanges(selectedRowId, form)
          setSelectedRowId(targetRowId)
          setSelectedCellId(`${targetRowId}-name`)
        }
      }
    }

    document.addEventListener('keydown', handleGlobalKeyDown, true)
    return () => {
      document.removeEventListener('keydown', handleGlobalKeyDown, true)
    }
  }, [selectedRowId, editableColumns, saveFormChanges, isSaving, form])

  // Handle keyboard navigation
  const handleKeyDown = useCallback(
    async (e: React.KeyboardEvent, rowId: string, columnId: string) => {
      if (!selectedRowId || !tableRef.current) return

      // Don't queue navigation while already saving
      if (isSaving) {
        e.preventDefault()
        return
      }

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
          // Don't intercept Enter if a Select dropdown is open
          if (
            document.querySelector(
              '.ant-select-dropdown:not(.ant-select-dropdown-hidden)',
            )
          ) {
            return
          }
          e.preventDefault()
          // Save and navigate down to first editable column
          if (currentRowIndex < rows.length - 1) {
            const saved = await saveFormChanges(selectedRowId, form)
            if (saved) {
              nextRowId = rows[currentRowIndex + 1].original.id
              nextColId = editableColumns[0]
              setSelectedRowId(nextRowId)
              setSelectedCellId(`${nextRowId}-${nextColId}`)
            }
            return
          }
          break

        case 'ArrowUp':
          // Don't intercept arrow keys if a Select dropdown or DatePicker is open
          if (
            document.querySelector(
              '.ant-select-dropdown:not(.ant-select-dropdown-hidden), .ant-picker-dropdown:not(.ant-picker-dropdown-hidden)',
            )
          ) {
            return
          }
          e.preventDefault()
          // Save and navigate up to first editable column
          if (currentRowIndex > 0) {
            nextRowId = rows[currentRowIndex - 1].original.id
            nextColId = editableColumns[0]
            await saveFormChanges(selectedRowId, form)
            setSelectedRowId(nextRowId)
            setSelectedCellId(`${nextRowId}-${nextColId}`)
            return
          }
          break

        case 'ArrowDown':
          // Don't intercept arrow keys if a Select dropdown or DatePicker is open
          if (
            document.querySelector(
              '.ant-select-dropdown:not(.ant-select-dropdown-hidden), .ant-picker-dropdown:not(.ant-picker-dropdown-hidden)',
            )
          ) {
            return
          }
          e.preventDefault()
          // Save and navigate down to same column
          if (currentRowIndex < rows.length - 1) {
            nextRowId = rows[currentRowIndex + 1].original.id
            nextColId = columnId
            await saveFormChanges(selectedRowId, form)
            setSelectedRowId(nextRowId)
            setSelectedCellId(`${nextRowId}-${nextColId}`)
            return
          }
          break

        case 'Escape':
          e.preventDefault()
          // Exit edit mode without saving
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
                // Check if a cell with this column ID exists in the DOM
                // (TanStack cell.id uses row index, so just check if column exists)
                const cellElement = document.querySelector(
                  `[data-cell-id*="-${testColId}"]`,
                )

                if (cellElement) {
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
              // No previous field found - save and exit edit mode
              await saveFormChanges(rowId, form)
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
              // No next field found - save and exit edit mode
              await saveFormChanges(rowId, form)
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
    [selectedRowId, editableColumns, form, saveFormChanges, isSaving],
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
        accessorKey: 'key',
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
            <span className={styles.nameCell} data-cell-id={cellId}>
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
                  onClick={row.getToggleExpandedHandler()}
                  className={styles.expanderBtn}
                />
              ) : (
                <span style={{ width: 24, display: 'inline-block' }} />
              )}
              {isSelected && handleUpdateTask ? (
                <FormItem
                  name="name"
                  style={{ margin: 0, flex: 1, minWidth: 0 }}
                  rules={[
                    { required: true, message: 'Name is required' },
                    { max: 256, message: 'Name cannot exceed 256 characters' },
                  ]}
                  validateStatus={getFieldError('name') ? 'error' : ''}
                >
                  <Input
                    size="small"
                    onPressEnter={(e) => {
                      e.currentTarget.blur()
                    }}
                    onKeyDown={(e) => handleKeyDown(e, task.id, 'name')}
                    style={{ flex: 1, minWidth: 0 }}
                    status={getFieldError('name') ? 'error' : ''}
                  />
                </FormItem>
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
          return info.getValue()
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

          if (!isSelected || !handleUpdateTask) {
            return (
              <Tag color={colorMap[status] || 'default'}>{status || '-'}</Tag>
            )
          }

          // Use pre-filtered status options based on task type
          const isMilestone = task.type?.name === 'Milestone'
          const availableStatusOptions = isMilestone
            ? taskStatusOptionsForMilestone
            : taskStatusOptions

          const error = getFieldError('statusId')
          return (
            <div data-cell-id={cellId}>
              <FormItem
                name="statusId"
                style={{ margin: 0 }}
                rules={[{ required: true, message: 'Status is required' }]}
                validateStatus={error ? 'error' : ''}
              >
                <Select
                  size="small"
                  options={availableStatusOptions}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'status')}
                  status={error ? 'error' : ''}
                />
              </FormItem>
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

          if (!isSelected || !handleUpdateTask) {
            if (!priority) return '-'
            const colorMap: Record<string, string> = {
              Low: 'green',
              Medium: 'orange',
              High: 'red',
              Critical: 'magenta',
            }
            return <Tag color={colorMap[priority]}>{priority}</Tag>
          }

          const error = getFieldError('priorityId')
          return (
            <div data-cell-id={cellId}>
              <FormItem
                name="priorityId"
                style={{ margin: 0 }}
                rules={[{ required: true, message: 'Priority is required' }]}
                validateStatus={error ? 'error' : ''}
              >
                <Select
                  size="small"
                  options={taskPriorityOptions}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'priority')}
                  status={error ? 'error' : ''}
                />
              </FormItem>
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

          if (!isSelected || !handleUpdateTask) {
            return value
          }

          const fieldName = isMilestone ? 'plannedDate' : 'plannedStart'
          const error = getFieldError(fieldName)

          return (
            <div data-cell-id={cellId}>
              <FormItem
                name={fieldName}
                style={{ margin: 0 }}
                validateStatus={error ? 'error' : ''}
              >
                <DatePicker
                  size="small"
                  format="MMM D, YYYY"
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'plannedStart')}
                  onOpenChange={(open) => {
                    if (!open) {
                      // Restore focus to the input after picker closes
                      setTimeout(() => {
                        const cell = document.querySelector(
                          `[data-cell-id="${cellId}"]`,
                        )
                        if (cell) {
                          const input = cell.querySelector('input') as HTMLInputElement
                          if (input) {
                            input.focus()
                          }
                        }
                      }, 0)
                    }
                  }}
                  status={error ? 'error' : ''}
                />
              </FormItem>
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

          if (!isSelected || !handleUpdateTask || isMilestone) {
            return value
          }

          const error = getFieldError('plannedEnd')

          return (
            <div data-cell-id={cellId}>
              <FormItem
                name="plannedEnd"
                style={{ margin: 0 }}
                validateStatus={error ? 'error' : ''}
              >
                <DatePicker
                  size="small"
                  format="MMM D, YYYY"
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'plannedEnd')}
                  onOpenChange={(open) => {
                    if (!open) {
                      // Restore focus to the input after picker closes
                      setTimeout(() => {
                        const cell = document.querySelector(
                          `[data-cell-id="${cellId}"]`,
                        )
                        if (cell) {
                          const input = cell.querySelector('input') as HTMLInputElement
                          if (input) {
                            input.focus()
                          }
                        }
                      }, 0)
                    }
                  }}
                  status={error ? 'error' : ''}
                />
              </FormItem>
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
      ,
      {
        id: 'progress',
        accessorFn: (row) => row.progress ?? '',
        header: 'Progress',
        size: 90,
        enableGlobalFilter: false,
        enableColumnFilter: true,
        cell: (info) => {
          const task = info.row.original
          const value = task.progress
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-progress`
          const isMilestone = task.type?.name === 'Milestone'

          if (!isSelected || !handleUpdateTask || isMilestone) {
            return value !== undefined ? `${value} %` : ''
          }

          const error = getFieldError('progress')
          return (
            <div data-cell-id={cellId}>
              <FormItem
                name="progress"
                style={{ margin: 0 }}
                validateStatus={error ? 'error' : ''}
              >
                <Input
                  size="small"
                  type="number"
                  min={0}
                  max={100}
                  suffix="%"
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'progress')}
                  status={error ? 'error' : ''}
                />
              </FormItem>
            </div>
          )
        },
        sortingFn: (a, b) => {
          const av = a.original.progress ?? -Infinity
          const bv = b.original.progress ?? -Infinity
          return av === bv ? 0 : av > bv ? 1 : -1
        },
      },
      {
        id: 'estimatedEffortHours',
        accessorFn: (row) => row.estimatedEffortHours ?? '',
        header: 'Est Effort',
        size: 90,
        enableGlobalFilter: false,
        enableColumnFilter: false,
        cell: (info) => {
          const task = info.row.original
          const value = task.estimatedEffortHours
          const isSelected = selectedRowId === task.id
          const cellId = `${task.id}-estimatedEffortHours`
          const isMilestone = task.type?.name === 'Milestone'

          if (!isSelected || !handleUpdateTask || isMilestone) {
            return value
          }

          const error = getFieldError('estimatedEffortHours')
          return (
            <div data-cell-id={cellId}>
              <FormItem
                name="estimatedEffortHours"
                style={{ margin: 0 }}
                validateStatus={error ? 'error' : ''}
              >
                <Input
                  size="small"
                  type="number"
                  min={0}
                  step={0.25}
                  suffix="h"
                  onKeyDown={(e) =>
                    handleKeyDown(e, task.id, 'estimatedEffortHours')
                  }
                  status={error ? 'error' : ''}
                />
              </FormItem>
            </div>
          )
        },
        sortingFn: (a, b) => {
          const av = a.original.estimatedEffortHours ?? -Infinity
          const bv = b.original.estimatedEffortHours ?? -Infinity
          return av === bv ? 0 : av > bv ? 1 : -1
        },
      },
      ...(canManageTasks
        ? [
            {
              id: 'actions',
              header: 'Actions',
              size: 110,
              enableSorting: false,
              enableGlobalFilter: false,
              enableColumnFilter: false,
              enableExport: false,
              cell: ({ row }) => (
                <Space size="small">
                  <Tooltip title="Edit">
                    <Button
                      type="text"
                      size="small"
                      icon={<EditOutlined />}
                      onClick={() => handleEditTask(row.original)}
                      tabIndex={-1}
                    />
                  </Tooltip>
                  <Tooltip title="Delete">
                    <Button
                      type="text"
                      size="small"
                      danger
                      icon={<DeleteOutlined />}
                      onClick={() => handleDeleteTask(row.original)}
                      tabIndex={-1}
                    />
                  </Tooltip>
                </Space>
              ),
            },
          ]
        : []),
    ],
    [
      canManageTasks,
      getFieldError,
      handleDeleteTask,
      handleEditTask,
      handleKeyDown,
      handleUpdateTask,
      selectedRowId,
      taskPriorityOptions,
      taskStatusOptions,
      taskStatusOptionsForMilestone,
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
    const exportableColumns = columns.filter(
      (col: any) => col.enableExport !== false,
    )

    const headers = exportableColumns.map((col: any) => {
      if (typeof col.header === 'string') {
        return col.header
      }
      return col.id || ''
    })

    const rows = table.getRowModel().rows.map((row) => {
      return exportableColumns.map((col: any) => {
        let value: unknown = ''

        // Use accessor function if available, otherwise use accessorKey
        if (col.accessorFn) {
          value = col.accessorFn(row.original, row.index)
        } else if (col.accessorKey) {
          const key = col.accessorKey as string
          value = (row.original as any)[key]
        }

        // Format dates for plannedStart
        if (col.id === 'plannedStart' && value) {
          const isMilestone = row.original.type?.name === 'Milestone'
          const plannedStartDate = isMilestone
            ? row.original.plannedDate
            : row.original.plannedStart
          return plannedStartDate
            ? dayjs(plannedStartDate).format('MMM D, YYYY')
            : ''
        }

        // Format dates for plannedEnd
        if (
          (col.id === 'plannedEnd' || col.accessorKey === 'plannedEnd') &&
          value
        ) {
          return dayjs(value as string).format('MMM D, YYYY')
        }

        return value ?? ''
      })
    })

    const csv = generateCsv(headers, rows)
    downloadCsvWithTimestamp(csv, 'project-tasks')
  }, [table, columns])

  return (
    <>
      <Form form={form} component={false}>
        <div className={styles.table}>
          <div className={styles.toolbar}>
            <div>
              {canManageTasks && (
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={handleCreateTask}
                >
                  Create Task
                </Button>
              )}
            </div>

            <div className={styles.toolbarRight}>
              <Text>
                {displayedRowCount} of {totalRowCount}
              </Text>
              <Input
                placeholder="Search"
                allowClear={true}
                value={searchValue}
                onChange={onSearchChange}
                suffix={<SearchOutlined />}
                className={styles.toolbarSearch}
              />
              {refetch && (
                <Tooltip title="Refresh">
                  <Button
                    type="text"
                    shape="circle"
                    icon={<ReloadOutlined />}
                    onClick={refetch}
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
              <span className={styles.toolbarDivider} />
              <Tooltip title="Export to CSV">
                <Button
                  type="text"
                  shape="circle"
                  icon={<DownloadOutlined />}
                  onClick={onExportCsv}
                  disabled={isLoading || displayedRowCount === 0}
                />
              </Tooltip>
              <Popover
                content={<KeyboardShortcutsContent />}
                title="Keyboard Shortcuts"
                trigger="click"
                placement="bottomRight"
              >
                <Tooltip title="Keyboard Shortcuts">
                  <Button
                    type="text"
                    shape="circle"
                    icon={<QuestionCircleOutlined />}
                  />
                </Tooltip>
              </Popover>
            </div>
          </div>

          <div className={styles.tableWrapper}>
            <table className={styles.tableElement}>
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
                            className={`${styles.th}${
                              canSort ? ` ${styles.thSortable}` : ''
                            }${canResize ? ` ${styles.thResizable}` : ''}`}
                            onClick={
                              canSort
                                ? header.column.getToggleSortingHandler()
                                : undefined
                            }
                          >
                            <span className={styles.thContent}>
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
                                className={`${styles.resizer}${
                                  header.column.getIsResizing()
                                    ? ` ${styles.resizerActive}`
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
                              className={styles.filterTh}
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
                            className={styles.filterTh}
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
                                    .map(
                                      (v) =>
                                        options.find((o) => o.value === v.value)
                                          ?.label,
                                    )
                                    .filter(Boolean)
                                  return labels.length === 1
                                    ? labels[0]
                                    : `${labels.length} selected`
                                }}
                                value={
                                  selectValue.length ? selectValue : undefined
                                }
                                options={options}
                                suffixIcon={<FilterOutlined />}
                                popupMatchSelectWidth={false}
                                classNames={{
                                  popup: {
                                    root: styles.filterPopup,
                                  },
                                }}
                                onChange={(v) =>
                                  column.setFilterValue(
                                    v && v.length ? v : undefined,
                                  )
                                }
                                className={styles.filterControl}
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
                                className={styles.filterControl}
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
                      className={`${styles.td} ${styles.loading}`}
                    >
                      <Spin />
                    </td>
                  </tr>
                ) : table.getRowModel().rows.length === 0 ? (
                  <tr>
                    <td
                      colSpan={columns.length}
                      className={`${styles.td} ${styles.empty}`}
                    >
                      <ModaEmpty message="No tasks found" />
                    </td>
                  </tr>
                ) : (
                  table.getRowModel().rows.flatMap((row, index) => {
                    const isSelected = selectedRowId === row.original.id
                    const rowElements = [
                      <tr
                        key={row.id}
                        className={`${styles.tr}${index % 2 === 1 ? ` ${styles.trAlt}` : ''}${isSelected ? ` ${styles.trSelected}` : ''}`}
                        onClick={async (e) => {
                          clickedInTableRef.current = true

                          // Block row selection while saving or if cannot manage tasks
                          if (isSaving || !canManageTasks) {
                            return
                          }

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

                          // If no cell found, ignore the click
                          if (!cellElement || !clickedColumnId) {
                            return
                          }

                          // Check if clicked column is editable
                          const isEditableColumn =
                            editableColumns.includes(clickedColumnId)

                          if (selectedRowId === row.original.id) {
                            // Clicking same row
                            if (isEditableColumn) {
                              // Clicked an editable cell - switch to it only if different cell
                              const targetCellId = `${row.original.id}-${clickedColumnId}`
                              if (selectedCellId !== targetCellId) {
                                setSelectedCellId(targetCellId)
                              }
                            } else {
                              // Clicked a non-editable cell - exit edit mode
                              setSelectedRowId(null)
                              setSelectedCellId(null)
                            }
                          } else if (selectedRowId) {
                            // Clicking different row - save current row first, then navigate
                            const saved = await saveFormChanges(
                              selectedRowId,
                              form,
                            )
                            if (saved) {
                              setSelectedRowId(row.original.id)
                              const targetCellId = isEditableColumn
                                ? `${row.original.id}-${clickedColumnId}`
                                : `${row.original.id}-name`
                              setSelectedCellId(targetCellId)
                            }
                          } else {
                            // No previous selection - just select the row
                            setSelectedRowId(row.original.id)
                            const targetCellId = isEditableColumn
                              ? `${row.original.id}-${clickedColumnId}`
                              : `${row.original.id}-name`
                            setSelectedCellId(targetCellId)
                          }
                        }}
                      >
                        {row.getVisibleCells().map((cell) => {
                          // Determine if this cell is editable when row is selected
                          const editableCells = [
                            'name',
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
                              data-cell-id={`${row.original.id}-${cell.column.id}`}
                              data-column-id={cell.column.id}
                              className={`${styles.td}${isEditableCell ? ` ${styles.editableCell}` : ''}`}
                              onClick={(e) => {
                                // If clicking inside a form input/select, let it handle the click
                                const target = e.target as HTMLElement
                                if (
                                  target.closest('input') ||
                                  target.closest('.ant-select') ||
                                  target.closest('.ant-picker')
                                ) {
                                  e.stopPropagation()
                                }
                              }}
                            >
                              {flexRender(
                                cell.column.columnDef.cell,
                                cell.getContext(),
                              )}
                            </td>
                          )
                        })}
                      </tr>,
                    ]

                    // Add error row if row is selected and has field errors
                    if (isSelected && Object.keys(fieldErrors).length > 0) {
                      const errorItems = Object.entries(fieldErrors).map(
                        ([field, error]) => (
                          <div key={field} style={{ marginBottom: '4px' }}>
                            <span style={{ fontWeight: 500 }}>{field}:</span>{' '}
                            {error}
                          </div>
                        ),
                      )

                      rowElements.push(
                        <tr
                          key={`${row.id}-errors`}
                          className={styles.tr}
                          style={{
                            backgroundColor: token.colorErrorBg,
                          }}
                        >
                          <td
                            colSpan={columns.length}
                            className={styles.td}
                            style={{
                              color: token.colorError,
                              fontSize: '12px',
                              padding: '12px',
                              borderTop: `1px solid ${token.colorErrorBorder}`,
                            }}
                          >
                            {errorItems}
                          </td>
                        </tr>,
                      )
                    }

                    return rowElements
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>
      </Form>

      {openCreateTaskForm && (
        <CreateProjectTaskForm
          projectIdOrKey={projectKey}
          showForm={openCreateTaskForm}
          onFormComplete={() => onCreateTaskFormClosed(true)}
          onFormCancel={() => onCreateTaskFormClosed(false)}
        />
      )}
      {openEditTaskForm && selectedTaskId && (
        <EditProjectTaskForm
          projectIdOrKey={projectKey}
          taskIdOrKey={selectedTaskId}
          showForm={openEditTaskForm}
          onFormComplete={() => onEditTaskFormClosed(true)}
          onFormCancel={() => onEditTaskFormClosed(false)}
        />
      )}
      {openDeleteTaskForm && selectedTaskId && (
        <DeleteProjectTaskForm
          projectIdOrKey={projectKey}
          taskIdOrKey={selectedTaskId}
          showForm={openDeleteTaskForm}
          onFormComplete={() => onDeleteTaskFormClosed(true)}
          onFormCancel={() => onDeleteTaskFormClosed(false)}
        />
      )}
    </>
  )
}

export default ProjectTasksTable

