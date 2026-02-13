import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { Form } from 'antd'
import dayjs from 'dayjs'
import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import { findProjectTaskById } from './project-task-tree'

const CELL_ID_COLUMN_MATCH_ORDER = [
  'estimatedEffortHours',
  'plannedStart',
  'plannedEnd',
  'assignees',
  'priority',
  'status',
  'progress',
  'type',
  'name',
] as const

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
  const focusRequestTokenRef = useRef(0)
  const focusObserverRef = useRef<MutationObserver | null>(null)

  const focusCellById = useCallback((cellId: string) => {
    // Disconnect any previous MutationObserver from an earlier call
    focusObserverRef.current?.disconnect()
    focusObserverRef.current = null
    const requestToken = ++focusRequestTokenRef.current
    let columnId = ''
    for (const col of CELL_ID_COLUMN_MATCH_ORDER) {
      if (cellId.endsWith(`-${col}`)) {
        columnId = col
        break
      }
    }
    const isDateColumn = columnId === 'plannedStart' || columnId === 'plannedEnd'

    const isActiveElementInsideCell = () => {
      const active = document.activeElement as HTMLElement | null
      const activeCellId = active
        ?.closest?.('[data-cell-id]')
        ?.getAttribute('data-cell-id')
      return activeCellId === cellId
    }
    const getActiveCellId = () => {
      const active = document.activeElement as HTMLElement | null
      return active?.closest?.('[data-cell-id]')?.getAttribute('data-cell-id')
    }

    const tryFocus = (attempt: number) => {
      if (focusRequestTokenRef.current !== requestToken) {
        return
      }

      let cellElement: Element | null = null
      const allCells = document.querySelectorAll('[data-cell-id]')
      for (const cell of allCells) {
        if (cell.getAttribute('data-cell-id') === cellId) {
          cellElement = cell
          break
        }
      }

      if (cellElement) {
        let input: HTMLElement | null = null

        if (columnId === 'plannedStart' || columnId === 'plannedEnd') {
          const picker = cellElement.querySelector('.ant-picker') as
            | HTMLElement
            | null
          const pickerInput = cellElement.querySelector(
            '.ant-picker-input > input',
          ) as HTMLElement | null

          // Focus the DatePicker wrapper first so visual focus styles are applied.
          picker?.focus()
          input = pickerInput ?? picker
        } else if (
          columnId === 'status' ||
          columnId === 'priority' ||
          columnId === 'type' ||
          columnId === 'assignees'
        ) {
          // The focusable element inside Ant Design Select is
          // input.ant-select-input (role="combobox").
          input = cellElement.querySelector(
            'input.ant-select-input',
          ) as HTMLElement | null
        } else {
          input = cellElement.querySelector('input')
        }

        if (!input && columnId !== 'assignees' && columnId !== 'status' && columnId !== 'priority' && columnId !== 'type') {
          input = cellElement.querySelector('.ant-picker')
        }

        if (input instanceof HTMLElement) {
          input.focus()
          if (input instanceof HTMLInputElement) {
            input.select()
          }

          if (!isActiveElementInsideCell()) {
            if (attempt < 12) {
              setTimeout(() => tryFocus(attempt + 1), 20)
            }
            return
          }

          // DatePicker inputs can be re-mounted during row/cell state updates.
          // Re-check shortly after and re-focus if focus was lost.
          if (isDateColumn) {
            setTimeout(() => {
              if (!isActiveElementInsideCell()) {
                const activeCellId = getActiveCellId()
                // If focus moved to another table cell (e.g., user pressed Tab),
                // do not steal it back.
                if (activeCellId && activeCellId !== cellId) {
                  return
                }
                tryFocus(6)
              }
            }, 40)
          }
          return
        }
      }

      if (attempt < 12) {
        setTimeout(() => tryFocus(attempt + 1), 20)
      }
    }

    // React may perform multiple render passes after row selection changes
    // (e.g. form.setFieldsValue triggers a microtask re-render that
    // unmounts and recreates Ant Design internals, blurring any focused
    // element). Use a MutationObserver to wait for the DOM to stabilize
    // before focusing. Once no mutations occur for 50ms, the DOM is
    // considered stable and we attempt to focus.
    const cellEl = document.querySelector(`[data-cell-id="${cellId}"]`)
    const observeTarget = cellEl?.closest('tr') ?? document.body
    let stabilityTimer: ReturnType<typeof setTimeout> | null = null
    const observer = new MutationObserver(() => {
      if (stabilityTimer) clearTimeout(stabilityTimer)
      stabilityTimer = setTimeout(() => {
        observer.disconnect()
        focusObserverRef.current = null
        tryFocus(0)
      }, 50)
    })
    focusObserverRef.current = observer
    observer.observe(observeTarget, {
      childList: true,
      subtree: true,
    })
    // Kick off the timer in case there are no mutations at all
    // (e.g. clicking a cell that's already in the selected row).
    stabilityTimer = setTimeout(() => {
      observer.disconnect()
      tryFocus(0)
    }, 50)
  }, [])

  const editableColumns = useMemo(
    () => {
      const baseColumns = [
        'name',
        'status',
        'priority',
        'plannedStart',
        'plannedEnd',
        'assignees',
        'progress',
        'estimatedEffortHours',
      ]

      // Type is only editable for draft (new) tasks.
      if (selectedRowId?.startsWith('draft-')) {
        return ['name', 'type', ...baseColumns.slice(1)]
      }

      return baseColumns
    },
    [selectedRowId],
  )

  const getFieldError = useCallback(
    (fieldName: string): string | undefined => {
      return fieldErrors[fieldName]
    },
    [fieldErrors],
  )
  const watchedPlannedStart = Form.useWatch('plannedStart', form)
  const watchedPlannedEnd = Form.useWatch('plannedEnd', form)

  // Clear date-pair validation errors as soon as the user fixes the values.
  useEffect(() => {
    if (!selectedRowId) return
    if (!fieldErrors.plannedStart && !fieldErrors.plannedEnd) return

    const task = findProjectTaskById(tasks, selectedRowId)
    const isDraft = selectedRowId.startsWith('draft-')
    const isMilestone = isDraft
      ? isSelectedRowMilestone
      : task?.type?.name === 'Milestone'

    let shouldClearDateErrors = false

    if (isMilestone) {
      shouldClearDateErrors = true
    } else {
      const hasPlannedStart = Boolean(watchedPlannedStart)
      const hasPlannedEnd = Boolean(watchedPlannedEnd)
      const bothOrNeither = hasPlannedStart === hasPlannedEnd
      const endNotBeforeStart =
        !hasPlannedStart ||
        !hasPlannedEnd ||
        !dayjs(watchedPlannedEnd).isBefore(dayjs(watchedPlannedStart), 'day')
      shouldClearDateErrors = bothOrNeither && endNotBeforeStart
    }

    if (!shouldClearDateErrors) return

    const nextErrors = { ...fieldErrors }
    delete nextErrors.plannedStart
    delete nextErrors.plannedEnd

    if (Object.keys(nextErrors).length !== Object.keys(fieldErrors).length) {
      setFieldErrors(nextErrors)
    }
  }, [
    fieldErrors,
    form,
    isSelectedRowMilestone,
    selectedRowId,
    setFieldErrors,
    tasks,
    watchedPlannedEnd,
    watchedPlannedStart,
  ])

  const saveFormChanges = useCallback(
    async (taskId: string) => {
      const isDraft = taskId.startsWith('draft-')
      const hasTouchedFields = form.isFieldsTouched()

      // Fast path for existing tasks: if nothing changed, skip validation
      // and diffing to keep row-to-row navigation responsive.
      if (!isDraft && !hasTouchedFields) {
        const task = findProjectTaskById(tasks, taskId)
        if (!task) return false

        const values = form.getFieldsValue() as any
        const isMilestoneForValidation = isDraft
          ? isSelectedRowMilestone
          : task?.type?.name === 'Milestone'

        // Client-side cross-field validation for Task rows.
        if (!isMilestoneForValidation) {
          const hasPlannedStart = Boolean(values.plannedStart)
          const hasPlannedEnd = Boolean(values.plannedEnd)
          const nextFieldErrors: Record<string, string> = {}

          if (hasPlannedStart !== hasPlannedEnd) {
            const message =
              'Planned Start and Planned End must both have a value or both be empty.'
            nextFieldErrors.plannedStart = message
            nextFieldErrors.plannedEnd = message
          }

          if (hasPlannedStart && hasPlannedEnd) {
            const plannedStart = dayjs(values.plannedStart)
            const plannedEnd = dayjs(values.plannedEnd)
            if (plannedEnd.isBefore(plannedStart, 'day')) {
              nextFieldErrors.plannedEnd =
                'Planned End cannot be earlier than Planned Start.'
            }
          }

          if (Object.keys(nextFieldErrors).length > 0) {
            setFieldErrors(nextFieldErrors)
            return false
          }
        }
        const taskAssigneeIds = task.assignees?.map((a) => a.id) ?? []
        const formAssigneeIds = values.assigneeIds ?? []
        const assigneesChanged =
          taskAssigneeIds.length !== formAssigneeIds.length ||
          !taskAssigneeIds.every((id: string) => formAssigneeIds.includes(id))

        const taskPlannedStart = task.plannedStart
          ? String(task.plannedStart).split('T')[0]
          : null
        const plannedStartFormatted = values.plannedStart
          ? values.plannedStart.format('YYYY-MM-DD')
          : null

        const taskPlannedEnd = task.plannedEnd
          ? String(task.plannedEnd).split('T')[0]
          : null
        const plannedEndFormatted = values.plannedEnd
          ? values.plannedEnd.format('YYYY-MM-DD')
          : null

        const taskPlannedDate = task.plannedDate
          ? String(task.plannedDate).split('T')[0]
          : null
        const plannedDateFormatted = values.plannedDate
          ? values.plannedDate.format('YYYY-MM-DD')
          : null

        const hasValueChanges =
          values.name !== task.name ||
          values.statusId !== task.status?.id ||
          values.priorityId !== task.priority?.id ||
          assigneesChanged ||
          plannedStartFormatted !== taskPlannedStart ||
          plannedEndFormatted !== taskPlannedEnd ||
          plannedDateFormatted !== taskPlannedDate ||
          values.progress !== task.progress ||
          values.estimatedEffortHours !== task.estimatedEffortHours

        if (!hasValueChanges) {
          return true
        }
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

        // Clear any prior inline errors once local validation passes.
        if (Object.keys(fieldErrors).length > 0) {
          setFieldErrors({})
        }

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
    [
      fieldErrors,
      form,
      isSelectedRowMilestone,
      onUpdateTask,
      setFieldErrors,
      tasks,
    ],
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

  // Focus the target cell when selectedCellId changes
  useEffect(() => {
    if (!selectedRowId || !selectedCellId) return

    if (lastFocusedCellRef.current === selectedCellId) return

    lastFocusedCellRef.current = selectedCellId
    focusCellById(selectedCellId)
  }, [focusCellById, selectedRowId, selectedCellId])

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
        return
      }

      const clickedInsideTable = target.closest(
        `.${tableWrapperClassName}`,
      )

      if (clickedInsideTable) {
        return
      }

      const saved = await saveFormChanges(selectedRowId)
      if (saved) {
        setSelectedRowId(null)
        setSelectedCellId(null)
      }
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

      // Tab is handled exclusively by the per-cell handleKeyDown to avoid
      // duplicate navigation and focus races. Do not handle Tab here.

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

        case 'Tab': {
          e.preventDefault()
          e.stopPropagation()

          // Blur the active element to close any open Select dropdowns
          // and commit their current value before navigating.
          if (document.activeElement instanceof HTMLElement) {
            document.activeElement.blur()
          }

          // Find next editable column within the CURRENT row by checking the
          // DOM for rendered inputs. Only the selected row has edit controls,
          // so we cannot DOM-query other rows for editability.
          const findNextColInCurrentRow = (
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
            // Try previous column in current row
            const col = findNextColInCurrentRow(currentColIndex - 1, -1)
            if (col) {
              nextColId = col
              nextRowId = rowId
            } else if (currentRowIndex > 0) {
              // Wrap to previous row, last editable column.
              // The row isn't selected yet so we can't DOM-check which
              // columns render inputs — use the last editable column and
              // let focusCellById handle finding the actual input.
              nextRowId = rows[currentRowIndex - 1].original.id
              nextColId = editableColumns[editableColumns.length - 1]
            } else {
              // Already at first column of first row — save and exit
              await saveFormChanges(rowId)
              setSelectedRowId(null)
              setSelectedCellId(null)
              return
            }
          } else {
            // Try next column in current row
            const col = findNextColInCurrentRow(currentColIndex + 1, 1)
            if (col) {
              nextColId = col
              nextRowId = rowId
            } else if (currentRowIndex < rows.length - 1) {
              // Wrap to next row, first editable column
              nextRowId = rows[currentRowIndex + 1].original.id
              nextColId = editableColumns[0]
            } else {
              // Already at last column of last row — save and exit
              await saveFormChanges(rowId)
              setSelectedRowId(null)
              setSelectedCellId(null)
              return
            }
          }
          break
        }
      }

      if (nextRowId && nextColId) {
        // If moving to a different row, save the current row first and
        // wait for the save to complete before navigating. This ensures
        // the refetch has finished and DOM is stable before focusing.
        if (nextRowId !== rowId) {
          const tabSaved = await saveFormChanges(rowId)
          if (!tabSaved) return
        }

        setSelectedRowId(nextRowId)
        setSelectedCellId(`${nextRowId}-${nextColId}`)
        // Focus is handled by the useEffect that watches selectedCellId
        // via focusCellById — no duplicate setTimeout needed here.
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

      const clickedColumnId = args.getClickedColumnId(target) ?? 'name'

      const isEditable = args.isEditableColumn(clickedColumnId)

      if (selectedRowId === args.rowId) {
        if (isEditable) {
          const targetCellId = `${args.rowId}-${clickedColumnId}`
          if (selectedCellId !== targetCellId) {
            setSelectedCellId(targetCellId)
          } else {
            focusCellById(targetCellId)
          }
        } else {
          const targetCellId = `${args.rowId}-name`
          if (selectedCellId !== targetCellId) {
            setSelectedCellId(targetCellId)
          } else {
            focusCellById(targetCellId)
          }
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
    [
      canManageTasks,
      focusCellById,
      isSaving,
      saveFormChanges,
      selectedCellId,
      selectedRowId,
    ],
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
