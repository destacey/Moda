import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Form } from 'antd'
import dayjs from 'dayjs'
import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import { findProjectTaskById } from './project-task-tree'

interface UseProjectTasksInlineEditingParams {
  tasks: ProjectTaskTreeDto[]
  canManageTasks: boolean
  form: ReturnType<typeof Form.useForm>[0]
  tableWrapperClassName: string
  onUpdateTask: (taskId: string, updates: Record<string, any>) => Promise<any>
  fieldErrors: Record<string, string>
  setFieldErrors: (next: Record<string, string>) => void
  onCancelDraft?: (taskId: string) => void
  isSelectedRowMilestone?: boolean
}

export const useProjectTasksInlineEditing = ({
  tasks,
  canManageTasks,
  form,
  tableWrapperClassName,
  onUpdateTask,
  fieldErrors,
  setFieldErrors,
  onCancelDraft,
  isSelectedRowMilestone = false,
}: UseProjectTasksInlineEditingParams) => {
  const [selectedRowId, setSelectedRowId] = useState<string | null>(null)
  const [selectedCellId, setSelectedCellId] = useState<string | null>(null)
  const [isSaving, setIsSaving] = useState(false)

  const tableRef = useRef<any>(null)
  const isInitializingRef = useRef(false)
  const lastFocusedCellRef = useRef<string | null>(null)
  const clickedInTableRef = useRef(false)

  const editableColumns = useMemo(
    () => [
      'name',
      'type',
      'status',
      'priority',
      'plannedStart',
      'plannedEnd',
      'assignees',
      'progress',
      'estimatedEffortHours',
    ],
    [],
  )

  const getFieldError = useCallback(
    (fieldName: string): string | undefined => {
      return fieldErrors[fieldName]
    },
    [fieldErrors],
  )

  const saveFormChanges = useCallback(
    async (taskId: string) => {
      const isDraft = taskId.startsWith('draft-')
      const hasTouchedFields = form.isFieldsTouched()

      // Fast path for existing tasks: if nothing changed, skip validation
      // and diffing to keep row-to-row navigation responsive.
      if (!isDraft && !hasTouchedFields) {
        return true
      }

      try {
        await form.validateFields()

        const task = findProjectTaskById(tasks, taskId)

        // For draft tasks, task will be found in the merged tasks list
        // We always need to save draft tasks
        if (!task && !isDraft) {
          return false
        }

        const values = form.getFieldsValue() as any

        const updates: Record<string, any> = {}
        let hasChanges = false

        // For draft tasks, always save all fields
        if (isDraft) {
          updates.name = values.name || ''
          updates.typeId = values.typeId
          updates.statusId = values.statusId
          updates.priorityId = values.priorityId
          updates.assigneeIds = values.assigneeIds ?? []

          // Milestones use plannedDate only; tasks use plannedStart/End only
          if (isSelectedRowMilestone) {
            updates.plannedStart = null
            updates.plannedEnd = null
            updates.plannedDate = values.plannedDate
              ? values.plannedDate.format('YYYY-MM-DD')
              : null
            updates.progress = null
            updates.estimatedEffortHours = null
          } else {
            updates.plannedStart = values.plannedStart
              ? values.plannedStart.format('YYYY-MM-DD')
              : null
            updates.plannedEnd = values.plannedEnd
              ? values.plannedEnd.format('YYYY-MM-DD')
              : null
            updates.plannedDate = null
            updates.progress = values.progress ?? 0
            updates.estimatedEffortHours = values.estimatedEffortHours
              ? Number(values.estimatedEffortHours)
              : null
          }
          hasChanges = true
        } else if (task) {
          // For existing tasks, only send changed fields
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
          // Compare assignee IDs arrays
          const taskAssigneeIds = task.assignees?.map((a) => a.id) ?? []
          const formAssigneeIds = values.assigneeIds ?? []
          const assigneesChanged =
            taskAssigneeIds.length !== formAssigneeIds.length ||
            !taskAssigneeIds.every((id: string) => formAssigneeIds.includes(id))
          if (assigneesChanged) {
            updates.assigneeIds = formAssigneeIds
            hasChanges = true
          }

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
        }

        if (!hasChanges) {
          return true
        }

        setIsSaving(true)
        const success = await onUpdateTask(taskId, updates)
        setIsSaving(false)
        return Boolean(success)
      } catch {
        setIsSaving(false)
        return false
      }
    },
    [form, isSelectedRowMilestone, onUpdateTask, tasks],
  )

  // Initialize form when row selection changes
  useEffect(() => {
    isInitializingRef.current = true

    if (selectedRowId) {
      const task = findProjectTaskById(tasks, selectedRowId)
      const isDraft = selectedRowId.startsWith('draft-')

      if (task || isDraft) {
        form.resetFields()
        setFieldErrors({})

        // For draft tasks, use default values
        if (isDraft) {
          const initialValues = {
            name: '',
            typeId: 1, // Task
            statusId: 1, // Not Started
            priorityId: 2, // Medium
            assigneeIds: [],
            progress: 0,
            plannedStart: null,
            plannedEnd: null,
            plannedDate: null,
            estimatedEffortHours: null,
          }
          form.setFieldsValue(initialValues)
        } else if (task) {
          // For existing tasks, load current values
          const initialValues = {
            name: task.name,
            typeId: task.type?.id,
            statusId: task.status?.id,
            priorityId: task.priority?.id,
            assigneeIds: task.assignees?.map((a) => a.id) ?? [],
            progress: task.progress,
            plannedStart: task.plannedStart ? dayjs(task.plannedStart) : null,
            plannedEnd: task.plannedEnd ? dayjs(task.plannedEnd) : null,
            plannedDate: task.plannedDate ? dayjs(task.plannedDate) : null,
            estimatedEffortHours: task.estimatedEffortHours,
          }
          form.setFieldsValue(initialValues)
        }
        isInitializingRef.current = false
      }
    } else {
      form.resetFields()
      isInitializingRef.current = false
    }

    lastFocusedCellRef.current = null
  }, [form, selectedRowId, setFieldErrors, tasks])

  // Handle cell focusing separately - only focus when cell ID changes
  useEffect(() => {
    if (!selectedRowId || !selectedCellId) return

    if (lastFocusedCellRef.current === selectedCellId) return

    lastFocusedCellRef.current = selectedCellId

    const timeout = setTimeout(() => {
      let cellElement = document.querySelector(
        `td[data-cell-id="${selectedCellId}"]`,
      )

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

      if (
        target.closest('.ant-select-dropdown') ||
        target.closest('.ant-picker-dropdown')
      ) {
        return
      }

      // Treat interacting with the filter row like an "outside" click:
      // user is moving away from inline editing to filter the table.
      const clickedInFilterRow = Boolean(
        target.closest('[data-role="column-filters"]'),
      )

      if (clickedInFilterRow) {
        const saved = await saveFormChanges(selectedRowId)
        if (saved) {
          setSelectedRowId(null)
          setSelectedCellId(null)
        }
        clickedInTableRef.current = false
        return
      }

      const focusedInTable = document.activeElement?.closest(
        `.${tableWrapperClassName}`,
      )

      if (!clickedInTableRef.current && !focusedInTable) {
        const saved = await saveFormChanges(selectedRowId)
        if (saved) {
          setSelectedRowId(null)
          setSelectedCellId(null)
        }
      }
      clickedInTableRef.current = false
    }

    document.addEventListener('mousedown', handleClickOutside)
    return () => {
      document.removeEventListener('mousedown', handleClickOutside)
    }
  }, [selectedRowId, saveFormChanges, tableWrapperClassName])

  // Global keyboard handler for navigation when row is selected
  useEffect(() => {
    if (!selectedRowId || !tableRef.current) return

    const handleGlobalKeyDown = async (e: KeyboardEvent) => {
      if (isSaving) return

      const activeElement = document.activeElement
      const currentCellElement = activeElement?.closest('[data-cell-id]')

      if (e.key === 'Tab' && currentCellElement) {
        const cellId = currentCellElement.getAttribute('data-cell-id')
        if (!cellId) return

        // Extract column name by matching against known editable columns.
        // Cannot split on '-' because task IDs (UUIDs) also contain hyphens.
        let columnId = ''
        for (const col of editableColumns) {
          if (cellId.endsWith(`-${col}`)) {
            columnId = col
            break
          }
        }
        const currentColIndex = editableColumns.indexOf(columnId)
        if (currentColIndex === -1) return

        e.preventDefault()
        e.stopPropagation()

        // Blur the active element to close any open Select dropdowns
        // and commit their current value before navigating
        if (activeElement instanceof HTMLElement) {
          activeElement.blur()
        }

        const rows = tableRef.current.getRowModel().rows
        const currentRowIndex = rows.findIndex(
          (r: any) => r.original.id === selectedRowId,
        )
        if (currentRowIndex === -1) return

        let nextRowId: string | null = null
        let nextColId: string | null = null

        // Helper to find the next column that has an editable input for the
        // given row. Skips columns that render read-only for the row's type
        // (e.g. milestones don't have plannedEnd, progress, estimatedEffortHours).
        const findNextEditableCol = (
          rowId: string,
          startColIdx: number,
          direction: 1 | -1,
        ): string | null => {
          let idx = startColIdx
          while (idx >= 0 && idx < editableColumns.length) {
            const col = editableColumns[idx]
            const cell = document.querySelector(
              `[data-cell-id="${rowId}-${col}"]`,
            )
            if (
              cell &&
              cell.querySelector('input, .ant-select, .ant-picker')
            ) {
              return col
            }
            idx += direction
          }
          return null
        }

        if (e.shiftKey) {
          const col = findNextEditableCol(selectedRowId, currentColIndex - 1, -1)
          if (col) {
            nextColId = col
            nextRowId = selectedRowId
          } else if (currentRowIndex > 0) {
            // Wrap to end of previous row — pick last editable column
            const prevRowId = rows[currentRowIndex - 1].original.id
            const col = findNextEditableCol(
              prevRowId,
              editableColumns.length - 1,
              -1,
            )
            nextColId = col ?? editableColumns[editableColumns.length - 1]
            nextRowId = prevRowId
          }
        } else {
          const col = findNextEditableCol(selectedRowId, currentColIndex + 1, 1)
          if (col) {
            nextColId = col
            nextRowId = selectedRowId
          } else if (currentRowIndex < rows.length - 1) {
            // Wrap to start of next row — pick first editable column
            const nextRow = rows[currentRowIndex + 1].original.id
            const col = findNextEditableCol(nextRow, 0, 1)
            nextColId = col ?? editableColumns[0]
            nextRowId = nextRow
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

      if (
        (e.key === 'Enter' || e.key === 'ArrowUp' || e.key === 'ArrowDown') &&
        !currentCellElement
      ) {
        // If a dropdown is still visible, the user is interacting with it
        if (
          document.querySelector(
            '.ant-select-dropdown:not(.ant-select-dropdown-hidden), .ant-picker-dropdown:not(.ant-picker-dropdown-hidden)',
          )
        ) {
          return
        }

        // If focus is still inside the table (e.g. on a Select that just
        // closed its dropdown, or any other form control), don't navigate.
        if (activeElement?.closest(`.${tableWrapperClassName}`)) {
          return
        }

        // e.target is the original event target (doesn't change even if
        // focus moves). If the keypress originated from inside a cell's
        // Select or DatePicker, the user was confirming a selection — not
        // requesting row navigation.
        const eventTarget = e.target as HTMLElement
        if (
          eventTarget?.closest?.('[data-cell-id]') ||
          eventTarget?.closest?.('.ant-select') ||
          eventTarget?.closest?.('.ant-picker')
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
          if (currentRowIndex < rows.length - 1) {
            targetRowId = rows[currentRowIndex + 1].original.id
          }
        } else if (e.key === 'ArrowUp') {
          if (currentRowIndex > 0) {
            targetRowId = rows[currentRowIndex - 1].original.id
          }
        }

        if (targetRowId) {
          const saved = await saveFormChanges(selectedRowId)
          if (!saved) return
          setSelectedRowId(targetRowId)
          setSelectedCellId(`${targetRowId}-name`)
        }
      }
    }

    document.addEventListener('keydown', handleGlobalKeyDown, true)
    return () => {
      document.removeEventListener('keydown', handleGlobalKeyDown, true)
    }
  }, [
    editableColumns,
    isSaving,
    saveFormChanges,
    selectedRowId,
    tableWrapperClassName,
  ])

  const handleKeyDown = useCallback(
    async (e: React.KeyboardEvent, rowId: string, columnId: string) => {
      if (!selectedRowId || !tableRef.current) return

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
          // If a dropdown is open, Enter should confirm selection inside that
          // control and must not trigger save/navigation.
          if (
            document.querySelector(
              '.ant-select-dropdown:not(.ant-select-dropdown-hidden), .ant-picker-dropdown:not(.ant-picker-dropdown-hidden)',
            )
          ) {
            return
          }
          e.preventDefault()
          const saved = await saveFormChanges(selectedRowId)
          if (saved) {
            // If there's a next row, move to it
            if (currentRowIndex < rows.length - 1) {
              nextRowId = rows[currentRowIndex + 1].original.id
              nextColId = editableColumns[0]
              setSelectedRowId(nextRowId)
              setSelectedCellId(`${nextRowId}-${nextColId}`)
            } else {
              // If this was the last row, exit edit mode
              setSelectedRowId(null)
              setSelectedCellId(null)
            }
          }
          return

        case 'ArrowUp':
          if (
            document.querySelector(
              '.ant-select-dropdown:not(.ant-select-dropdown-hidden), .ant-picker-dropdown:not(.ant-picker-dropdown-hidden)',
            )
          ) {
            return
          }
          e.preventDefault()
          if (currentRowIndex > 0) {
            nextRowId = rows[currentRowIndex - 1].original.id
            nextColId = editableColumns[0]
            await saveFormChanges(selectedRowId)
            setSelectedRowId(nextRowId)
            setSelectedCellId(`${nextRowId}-${nextColId}`)
            return
          }
          break

        case 'ArrowDown':
          if (
            document.querySelector(
              '.ant-select-dropdown:not(.ant-select-dropdown-hidden), .ant-picker-dropdown:not(.ant-picker-dropdown-hidden)',
            )
          ) {
            return
          }
          e.preventDefault()
          if (currentRowIndex < rows.length - 1) {
            nextRowId = rows[currentRowIndex + 1].original.id
            nextColId = columnId
            await saveFormChanges(selectedRowId)
            setSelectedRowId(nextRowId)
            setSelectedCellId(`${nextRowId}-${nextColId}`)
            return
          }
          break

        case 'Escape':
          e.preventDefault()
          // If this is a draft task, cancel it
          if (rowId.startsWith('draft-') && onCancelDraft) {
            onCancelDraft(rowId)
          }
          setSelectedRowId(null)
          setSelectedCellId(null)
          return

        case 'Tab':
          e.preventDefault()
          e.stopPropagation()

          // Blur the active element to close any open Select dropdowns
          // and commit their current value before navigating
          if (document.activeElement instanceof HTMLElement) {
            document.activeElement.blur()
          }

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
              while (
                direction === 'forward'
                  ? currentColIdx < editableColumns.length
                  : currentColIdx >= 0
              ) {
                const testRowId = rows[currentRowIdx].original.id
                const testColId = editableColumns[currentColIdx]
                const cellElement = document.querySelector(
                  `[data-cell-id="${testRowId}-${testColId}"]`,
                )

                // Only consider this cell if it has an actual editable input
                if (
                  cellElement &&
                  cellElement.querySelector(
                    'input, .ant-select, .ant-picker',
                  )
                ) {
                  return { rowId: testRowId, colId: testColId }
                }

                currentColIdx += direction === 'forward' ? 1 : -1
              }

              currentRowIdx += direction === 'forward' ? 1 : -1
              currentColIdx =
                direction === 'forward' ? 0 : editableColumns.length - 1
            }

            return null
          }

          if (e.shiftKey) {
            const result = findNextAvailableField(
              rowId,
              currentColIndex - 1,
              'backward',
            )
            if (result) {
              nextRowId = result.rowId
              nextColId = result.colId
            } else {
              await saveFormChanges(rowId)
              setSelectedRowId(null)
              setSelectedCellId(null)
              return
            }
          } else {
            const result = findNextAvailableField(
              rowId,
              currentColIndex + 1,
              'forward',
            )
            if (result) {
              nextRowId = result.rowId
              nextColId = result.colId
            } else {
              await saveFormChanges(rowId)
              setSelectedRowId(null)
              setSelectedCellId(null)
              return
            }
          }
          break
      }

      if (nextRowId && nextColId) {
        setSelectedRowId(nextRowId)
        setSelectedCellId(`${nextRowId}-${nextColId}`)

        setTimeout(() => {
          const cellId = `${nextRowId}-${nextColId}`
          const nextCell = document.querySelector(`[data-cell-id="${cellId}"]`)
          if (nextCell) {
            let input = nextCell.querySelector('input') as HTMLElement

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
    [editableColumns, isSaving, onCancelDraft, saveFormChanges, selectedRowId],
  )

  const handleRowClick = useCallback(
    async (
      e: React.MouseEvent,
      args: {
        rowId: string
        isEditableColumn: (columnId: string) => boolean
        getClickedColumnId: (target: HTMLElement) => string | null
      },
    ) => {
      clickedInTableRef.current = true

      if (isSaving || !canManageTasks) {
        return
      }

      const target = e.target as HTMLElement
      if (
        target.closest('.ant-select-dropdown') ||
        target.closest('.ant-picker-dropdown') ||
        target.closest('input') ||
        target.closest('.ant-select-selector') ||
        target.classList.contains('ant-select-item-option-content')
      ) {
        return
      }

      if (target.closest('button') || target.closest('.ant-btn')) {
        return
      }

      const clickedColumnId = args.getClickedColumnId(target)
      if (!clickedColumnId) {
        return
      }

      const isEditable = args.isEditableColumn(clickedColumnId)

      if (selectedRowId === args.rowId) {
        if (isEditable) {
          const targetCellId = `${args.rowId}-${clickedColumnId}`
          if (selectedCellId !== targetCellId) {
            setSelectedCellId(targetCellId)
          }
        } else {
          setSelectedRowId(null)
          setSelectedCellId(null)
        }
      } else if (selectedRowId) {
        const saved = await saveFormChanges(selectedRowId)
        if (saved) {
          setSelectedRowId(args.rowId)
          const targetCellId = isEditable
            ? `${args.rowId}-${clickedColumnId}`
            : `${args.rowId}-name`
          setSelectedCellId(targetCellId)
        }
      } else {
        setSelectedRowId(args.rowId)
        const targetCellId = isEditable
          ? `${args.rowId}-${clickedColumnId}`
          : `${args.rowId}-name`
        setSelectedCellId(targetCellId)
      }
    },
    [canManageTasks, isSaving, saveFormChanges, selectedCellId, selectedRowId],
  )

  return {
    tableRef,
    selectedRowId,
    selectedCellId,
    setSelectedRowId,
    setSelectedCellId,
    isSaving,
    getFieldError,
    editableColumns,
    saveFormChanges,
    handleKeyDown,
    handleRowClick,
  }
}
