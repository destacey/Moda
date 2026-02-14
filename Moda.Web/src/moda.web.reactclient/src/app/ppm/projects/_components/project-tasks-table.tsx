'use client'

import styles from '@/src/components/common/tree-grid/tree-grid.module.css'
import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import { ModaEmpty } from '@/src/components/common'
import { Button, Form, Input, Select, Spin } from 'antd'
import {
  ArrowUpOutlined,
  ArrowDownOutlined,
  FilterOutlined,
  PlusOutlined,
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
import {
  DndContext,
  DragEndEvent,
  DragStartEvent,
  DragCancelEvent,
  useSensor,
  useSensors,
  PointerSensor,
  KeyboardSensor,
  closestCenter,
} from '@dnd-kit/core'
import { SortableContext } from '@dnd-kit/sortable'

import { useMessage } from '@/src/components/contexts/messaging'
import {
  useGetTaskPriorityOptionsQuery,
  useGetTaskStatusOptionsQuery,
  useGetTaskTypeOptionsQuery,
  usePatchProjectTaskMutation,
  useUpdateProjectTaskPlacementMutation,
  useCreateProjectTaskMutation,
} from '@/src/store/features/ppm/project-tasks-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'

import {
  type DraftItem,
  type MoveValidator,
  countTreeNodes,
  findNodeById,
  flattenTree,
  getProjection,
  defaultMoveValidator,
  calculateOrderInParent,
  mergeDraftsIntoTree,
  stringContainsFilter,
  TreeGridSortableRow,
  TreeGridToolbar,
  useTreeGridEditing,
  INDENTATION_WIDTH,
  DRAG_ACTIVATION_DISTANCE,
} from '@/src/components/common/tree-grid'
import CreateProjectTaskForm from './create-project-task-form'
import DeleteProjectTaskForm from './delete-project-task-form'
import EditProjectTaskForm from './edit-project-task-form'
import { buildProjectTaskPatchOperations } from './project-task-patch'
import { getProjectTasksTableColumns } from './project-tasks-table.columns'
import { ProjectTasksHelp } from './project-tasks-table.keyboard-shortcuts'

const EMPTY_STRING_ARRAY: string[] = []

interface ProjectTasksTableProps {
  projectKey: string
  tasks: ProjectTaskTreeDto[]
  isLoading: boolean
  canManageTasks: boolean
  refetch: () => Promise<any>
  enableDragAndDrop?: boolean
}

const ProjectTasksTable = ({
  projectKey,
  tasks = [],
  isLoading,
  canManageTasks,
  refetch,
  enableDragAndDrop = true,
}: ProjectTasksTableProps) => {
  const [searchValue, setSearchValue] = useState('')
  const [sorting, setSorting] = useState<SortingState>([])
  const [columnFilters, setColumnFilters] = useState<ColumnFiltersState>([])
  const [columnSizing, setColumnSizing] = useState<ColumnSizingState>({})
  const [form] = Form.useForm()

  const [openCreateTaskForm, setOpenCreateTaskForm] = useState<boolean>(false)
  const [openEditTaskForm, setOpenEditTaskForm] = useState<boolean>(false)
  const [openDeleteTaskForm, setOpenDeleteTaskForm] = useState<boolean>(false)
  const [selectedTaskId, setSelectedTaskId] = useState<string | undefined>(
    undefined,
  )
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})
  const [draggedTaskId, setDraggedTaskId] = useState<string | null>(null)
  const isResizingRef = useRef(false)
  const messageApi = useMessage()

  const { data: taskStatusOptions = [] } = useGetTaskStatusOptionsQuery()
  const { data: taskStatusOptionsForMilestone = [] } =
    useGetTaskStatusOptionsQuery({ forMilestone: true })

  const { data: taskPriorityOptions = [] } = useGetTaskPriorityOptionsQuery()

  const { data: taskTypeOptions = [] } = useGetTaskTypeOptionsQuery()
  const milestoneTypeValue = useMemo(
    () => taskTypeOptions.find((opt) => opt.label === 'Milestone')?.value,
    [taskTypeOptions],
  )

  const { data: employeeOptions = [] } = useGetEmployeeOptionsQuery(false)

  // Track the selected row's type to dynamically show/hide milestone-specific fields
  const selectedTypeId = Form.useWatch('typeId', form)
  const isSelectedRowMilestone = selectedTypeId === milestoneTypeValue

  const [patchProjectTask] = usePatchProjectTaskMutation()
  const [updateProjectTaskPlacement] = useUpdateProjectTaskPlacementMutation()
  const [createProjectTask] = useCreateProjectTaskMutation()

  // Reset status to "Not Started" if user switches to Milestone while "In Progress" is selected
  useEffect(() => {
    if (isSelectedRowMilestone) {
      const currentStatus = form.getFieldValue('statusId')
      const inProgressValue = taskStatusOptions.find(
        (opt) => opt.label === 'In Progress',
      )?.value
      if (inProgressValue !== undefined && currentStatus === inProgressValue) {
        const notStartedValue = taskStatusOptions.find(
          (opt) => opt.label === 'Not Started',
        )?.value
        if (notStartedValue !== undefined) {
          form.setFieldValue('statusId', notStartedValue)
        }
      }
    }
  }, [isSelectedRowMilestone, form, taskStatusOptions])

  // Draft tasks state for inline creation
  const [draftTasks, setDraftTasks] = useState<DraftItem[]>([])
  const draftCounterRef = useRef(0)

  const createDraftProjectTask = useCallback(
    (draft: DraftItem): ProjectTaskTreeDto => ({
      id: draft.id,
      projectId: '',
      key: '',
      wbs: '',
      name: '',
      type: { id: 1, name: 'Task' },
      status: { id: 1, name: 'Not Started' },
      priority: { id: 2, name: 'Medium' },
      assignees: [],
      progress: 0,
      order: draft.order,
      children: [],
    }),
    [],
  )

  // Merge draft tasks with real tasks early (before using in hooks)
  const tasksWithDrafts = useMemo(() => {
    return mergeDraftsIntoTree(tasks, draftTasks, createDraftProjectTask)
  }, [tasks, draftTasks, createDraftProjectTask])

  // Flatten tree for drag-and-drop
  const flattenedTasks = useMemo(() => {
    return flattenTree(tasks)
  }, [tasks])

  // Domain-specific DnD move validator: milestones cannot have children
  const projectTaskMoveValidator: MoveValidator<ProjectTaskTreeDto> = useCallback(
    (activeNode, targetParentNode, targetParentId) => {
      const result = defaultMoveValidator(activeNode, targetParentNode, targetParentId)
      if (!result.canMove) return result
      if (targetParentNode?.type?.name === 'Milestone') {
        return { canMove: false, reason: 'Milestones cannot have child tasks' }
      }
      return { canMove: true }
    },
    [],
  )

  // DnD sensors
  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: DRAG_ACTIVATION_DISTANCE,
      },
    }),
    useSensor(KeyboardSensor),
  )

  const handleUpdateTask = useCallback(
    async (taskId: string, updates: Partial<any>): Promise<boolean> => {
      if (!projectKey) return false

      // Check if this is a draft task (new task being created)
      const isDraft = taskId.startsWith('draft-')

      if (isDraft) {
        // This is a new task - use create API
        try {
          const draft = draftTasks.find((d) => d.id === taskId)
          if (!draft) return false

          // Build create request from updates
          // Note: progress must be null for milestones and a number for tasks.
          // The saveFormChanges function sets the correct value based on type.
          const request: any = {
            name: updates.name || '',
            typeId: updates.typeId || 1, // Default to Task type
            statusId: updates.statusId || 1,
            priorityId: updates.priorityId || 2,
            assigneeIds: updates.assigneeIds || [],
            progress: updates.progress,
            parentId: draft.parentId,
            plannedStart: updates.plannedStart,
            plannedEnd: updates.plannedEnd,
            plannedDate: updates.plannedDate,
            estimatedEffortHours: updates.estimatedEffortHours,
          }

          const response = await createProjectTask({
            projectIdOrKey: projectKey,
            request,
          })

          if (response.error) {
            throw response.error
          }

          messageApi.success(`Task created: ${response.data.key}`)

          // Remove draft from state
          setDraftTasks((prev) => prev.filter((d) => d.id !== taskId))

          // Refetch to get the new task
          await refetch()

          return true
        } catch (error: any) {
          const status = error?.status ?? error?.data?.status
          const errors = error?.errors ?? error?.data?.errors
          const detail = error?.detail ?? error?.data?.detail

          if (status === 422 && errors) {
            const errorMap: Record<string, string> = {}
            const errorFields: string[] = []
            Object.entries(errors).forEach(([key, messages]) => {
              const fieldName = key.charAt(0).toLowerCase() + key.slice(1)
              errorMap[fieldName] = Array.isArray(messages)
                ? messages[0]
                : messages
              errorFields.push(fieldName)
            })
            setFieldErrors(errorMap)

            // Focus on the first mappable error field
            setTimeout(() => {
              let focused = false

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
              detail ??
                'An error occurred while creating the project task. Please try again.',
            )
          }
          return false
        }
      } else {
        // This is an existing task - use patch API
        const task = findNodeById(tasks || [], taskId)
        if (!task) return false

        try {
          const patchOperations = buildProjectTaskPatchOperations(updates)

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
          const status = error?.status ?? error?.data?.status
          const errors = error?.errors ?? error?.data?.errors
          const detail = error?.detail ?? error?.data?.detail

          if (status === 422 && errors) {
            const errorMap: Record<string, string> = {}
            const errorFields: string[] = []
            Object.entries(errors).forEach(([key, messages]) => {
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
              detail ??
                'An error occurred while updating the project task. Please try again.',
            )
          }
          return false
        }
      }
    },
    [
      messageApi,
      projectKey,
      refetch,
      tasks,
      patchProjectTask,
      draftTasks,
      createProjectTask,
    ],
  )

  const {
    tableRef,
    selectedRowId,
    setSelectedRowId,
    setSelectedCellId,
    getFieldError,
    editableColumns,
    handleKeyDown,
    handleRowClick,
  } = useTreeGridEditing<ProjectTaskTreeDto>({
    data: tasksWithDrafts,
    canEdit: canManageTasks,
    form,
    tableWrapperClassName: styles.tableWrapper,
    draftPrefix: 'draft-',

    editableColumnIds: (rowId) => {
      const base = [
        'name',
        'status',
        'priority',
        'plannedStart',
        'plannedEnd',
        'assignees',
        'progress',
        'estimatedEffortHours',
      ]
      // Type is only editable for draft (new) tasks
      return rowId?.startsWith('draft-')
        ? ['name', 'type', ...base.slice(1)]
        : base
    },

    getFormValues: (rowId, data) => {
      const task = findNodeById(data, rowId) as ProjectTaskTreeDto | null
      const isDraft = rowId.startsWith('draft-')

      if (isDraft || !task) {
        return {
          name: '',
          typeId: 1,
          statusId: 1,
          priorityId: 2,
          assigneeIds: [],
          progress: 0,
          plannedStart: null,
          plannedEnd: null,
          plannedDate: null,
          estimatedEffortHours: null,
        }
      }

      return {
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
    },

    computeChanges: (rowId, formValues, data) => {
      const isDraft = rowId.startsWith('draft-')
      const values = formValues as any

      if (isDraft) {
        const updates: Record<string, any> = {
          name: values.name || '',
          typeId: values.typeId,
          statusId: values.statusId,
          priorityId: values.priorityId,
          assigneeIds: values.assigneeIds ?? [],
        }
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
        return updates
      }

      // Existing task — diff fields individually
      const task = findNodeById(data, rowId) as ProjectTaskTreeDto | null
      if (!task) return null

      const updates: Record<string, any> = {}
      let hasChanges = false

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

      return hasChanges ? updates : null
    },

    validateFields: (rowId, formValues) => {
      const isDraft = rowId.startsWith('draft-')
      const task = findNodeById(tasksWithDrafts, rowId) as ProjectTaskTreeDto | null
      const isMilestone = isDraft
        ? isSelectedRowMilestone
        : task?.type?.name === 'Milestone'

      // Milestones don't have planned date pair validation
      if (isMilestone) return {}

      const errors: Record<string, string> = {}
      const hasPlannedStart = Boolean(formValues.plannedStart)
      const hasPlannedEnd = Boolean(formValues.plannedEnd)

      if (hasPlannedStart !== hasPlannedEnd) {
        const message =
          'Planned Start and Planned End must both have a value or both be empty.'
        errors.plannedStart = message
        errors.plannedEnd = message
      }

      if (hasPlannedStart && hasPlannedEnd) {
        const plannedStart = dayjs(formValues.plannedStart)
        const plannedEnd = dayjs(formValues.plannedEnd)
        if (plannedEnd.isBefore(plannedStart, 'day')) {
          errors.plannedEnd =
            'Planned End cannot be earlier than Planned Start.'
        }
      }

      return errors
    },

    cellIdColumnMatchOrder: [
      'estimatedEffortHours',
      'plannedStart',
      'plannedEnd',
      'assignees',
      'priority',
      'status',
      'progress',
      'type',
      'name',
    ],

    onSave: handleUpdateTask,
    fieldErrors,
    setFieldErrors,
    onCancelDraft: (taskId: string) => {
      if (taskId.startsWith('draft-')) {
        setDraftTasks((prev) => prev.filter((d) => d.id !== taskId))
      }
    },
  })

  // Clear date-pair validation errors as soon as the user fixes the values
  const watchedPlannedStart = Form.useWatch('plannedStart', form)
  const watchedPlannedEnd = Form.useWatch('plannedEnd', form)

  useEffect(() => {
    if (!selectedRowId) return
    if (!fieldErrors.plannedStart && !fieldErrors.plannedEnd) return

    const task = findNodeById(tasksWithDrafts, selectedRowId) as ProjectTaskTreeDto | null
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
    isSelectedRowMilestone,
    selectedRowId,
    setFieldErrors,
    tasksWithDrafts,
    watchedPlannedEnd,
    watchedPlannedStart,
  ])

  // Add a draft task at root level
  const addDraftTaskAtRoot = useCallback(() => {
    if (selectedRowId !== null || draftTasks.length > 0) {
      return
    }

    draftCounterRef.current += 1
    const newDraft: DraftItem = {
      id: `draft-${Date.now()}-${draftCounterRef.current}`,
      parentId: undefined,
      order: 0,
    }
    setDraftTasks((prev) => [...prev, newDraft])

    // Auto-select the new draft row for editing and focus the name field
    // Use requestAnimationFrame + setTimeout to ensure DOM is fully rendered
    requestAnimationFrame(() => {
      setTimeout(() => {
        setSelectedRowId(newDraft.id)
        setSelectedCellId(`${newDraft.id}-name`)
      }, 50)
    })
  }, [draftTasks.length, selectedRowId, setSelectedRowId, setSelectedCellId])

  // Add a draft task as a child of a specific parent
  const addDraftTaskAsChild = useCallback(
    (parentId: string) => {
      if (selectedRowId !== null || draftTasks.length > 0) {
        return
      }

      draftCounterRef.current += 1
      const newDraft: DraftItem = {
        id: `draft-${Date.now()}-${draftCounterRef.current}`,
        parentId,
        order: 0,
      }
      setDraftTasks((prev) => [...prev, newDraft])

      // Ensure parent is expanded
      if (tableRef.current) {
        const rows = tableRef.current.getRowModel().rows
        const parentRow = rows.find((r: any) => r.original.id === parentId)
        if (parentRow && !parentRow.getIsExpanded()) {
          parentRow.toggleExpanded()
        }
      }

      // Auto-select the new draft row for editing and focus the name field
      // Use requestAnimationFrame + setTimeout to ensure DOM is fully rendered
      requestAnimationFrame(() => {
        setTimeout(() => {
          setSelectedRowId(newDraft.id)
          setSelectedCellId(`${newDraft.id}-name`)
        }, 50)
      })
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [draftTasks.length, selectedRowId, setSelectedRowId, setSelectedCellId], // tableRef is stable and doesn't need to be in deps
  )

  // Task form handlers
  const handleCreateTask = useCallback(() => {
    // Instead of opening modal, add draft task inline
    addDraftTaskAtRoot()
  }, [addDraftTaskAtRoot])

  const handleEditTask = useCallback(
    (task: any) => {
      setSelectedRowId(null) // Exit inline edit mode
      setSelectedTaskId(task.id)
      setOpenEditTaskForm(true)
    },
    [setSelectedRowId],
  )

  const handleDeleteTask = useCallback(
    (task: any) => {
      setSelectedRowId(null) // Exit inline edit mode
      setSelectedTaskId(task.id)
      setOpenDeleteTaskForm(true)
    },
    [setSelectedRowId],
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
  const emptyFilterArray = EMPTY_STRING_ARRAY

  // Column filters for type/status/priority operate on the column accessor values (names),
  // so convert id-based API options to name-based option values.
  const taskTypeFilterOptions = useMemo(
    () =>
      taskTypeOptions
        .map((o: any) => {
          const label = (o?.label ?? '') as string
          return label ? { label, value: label } : null
        })
        .filter(Boolean),
    [taskTypeOptions],
  )

  const taskStatusFilterOptions = useMemo(
    () =>
      taskStatusOptions
        .map((o: any) => {
          const label = (o?.label ?? '') as string
          return label ? { label, value: label } : null
        })
        .filter(Boolean),
    [taskStatusOptions],
  )

  const taskPriorityFilterOptions = useMemo(
    () =>
      taskPriorityOptions
        .map((o: any) => {
          const label = (o?.label ?? '') as string
          return label ? { label, value: label } : null
        })
        .filter(Boolean),
    [taskPriorityOptions],
  )

  const totalRowCount = useMemo(() => {
    return countTreeNodes(tasks) + draftTasks.length
  }, [tasks, draftTasks])

  // Early calculation for isDragEnabled (before columns definition)
  const hasFilters =
    !!searchValue || columnFilters.length > 0 || sorting.length > 0
  const isEditing = selectedRowId !== null
  const canCreateDraftTask =
    canManageTasks && !isEditing && draftTasks.length === 0
  const isDragEnabled = useMemo(() => {
    return (
      enableDragAndDrop !== false &&
      canManageTasks &&
      !hasFilters &&
      !isLoading &&
      !isEditing
    )
  }, [enableDragAndDrop, canManageTasks, hasFilters, isLoading, isEditing])

  const columns = useMemo<ColumnDef<ProjectTaskTreeDto>[]>(
    () =>
      getProjectTasksTableColumns({
        canManageTasks,
        selectedRowId,
        handleEditTask,
        handleDeleteTask,
        handleUpdateTask,
        getFieldError,
        handleKeyDown,
        taskStatusOptions,
        taskStatusOptionsForMilestone,
        taskPriorityOptions,
        taskTypeOptions,
        employeeOptions,
        isDragEnabled,
        enableDragAndDrop,
        addDraftTaskAsChild,
        canCreateTasks: canCreateDraftTask,
        isSelectedRowMilestone,
      }),
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
      taskTypeOptions,
      employeeOptions,
      isDragEnabled,
      enableDragAndDrop,
      addDraftTaskAsChild,
      canCreateDraftTask,
      isSelectedRowMilestone,
    ],
  )

  const table = useReactTable({
    data: tasksWithDrafts,
    columns,
    getCoreRowModel: getCoreRowModel(),
    getFilteredRowModel: getFilteredRowModel(),
    getSortedRowModel: getSortedRowModel(),
    getExpandedRowModel: getExpandedRowModel(),
    getSubRows: (row) => row.children,
    filterFromLeafRows: true,
    // TanStack's default global-filter eligibility is conservative (often string-only and can
    // exclude columns where the first row's value is undefined). We want global search to
    // include explicitly-enabled numeric columns like Est Effort.
    getColumnCanGlobalFilter: (column) => {
      const colDef = column.columnDef as unknown as {
        enableGlobalFilter?: boolean
        accessorFn?: unknown
        accessorKey?: unknown
      }

      if (colDef.enableGlobalFilter === false) return false

      return Boolean(colDef.accessorFn || colDef.accessorKey)
    },
    // TanStack's built-in includesString global filter ignores non-string values.
    // Use our stringifying variant so numeric columns (e.g. Est Effort) are searchable.
    globalFilterFn: stringContainsFilter,
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

  // Drag handlers
  const handleDragStart = useCallback(
    (event: DragStartEvent) => {
      setDraggedTaskId(event.active.id as string)
      setSelectedRowId(null) // Cancel inline editing
    },
    [setSelectedRowId],
  )

  const handleDragEnd = useCallback(
    async (event: DragEndEvent) => {
      const { active, over, delta } = event

      setDraggedTaskId(null)

      if (!over) {
        return
      }

      // Now that we've removed verticalListSortingStrategy, delta.x should work
      const horizontalOffset = delta.x

      // Allow dragging over self if there's horizontal movement (depth change)
      const hasHorizontalMovement =
        Math.abs(horizontalOffset) >= INDENTATION_WIDTH / 2

      if (active.id === over.id && !hasHorizontalMovement) {
        return
      }

      // Calculate projection using the tracked horizontal offset
      const projection = getProjection(
        flattenedTasks,
        active.id as string,
        over.id as string,
        horizontalOffset,
        INDENTATION_WIDTH,
        projectTaskMoveValidator,
      )

      if (!projection.canDrop) {
        messageApi.warning(
          projection.reason || 'Cannot move task to this location',
        )
        return
      }

      // Calculate new order
      const overIndex = flattenedTasks.findIndex((t) => t.node.id === over.id)
      const newOrder = calculateOrderInParent(
        flattenedTasks,
        active.id as string,
        overIndex,
        projection.parentId,
      )

      // Call API with optimistic update approach
      try {
        await updateProjectTaskPlacement({
          projectIdOrKey: projectKey,
          id: active.id as string,
          request: {
            taskId: active.id as string,
            parentId: projection.parentId ?? undefined,
            order: newOrder,
          },
        }).unwrap()

        // Refetch to ensure consistency
        await refetch()
      } catch (error: any) {
        messageApi.error(
          error?.data?.detail || 'Failed to move task. Please try again.',
        )
      }
    },
    [
      flattenedTasks,
      projectKey,
      projectTaskMoveValidator,
      updateProjectTaskPlacement,
      refetch,
      messageApi,
    ],
  )

  const handleDragCancel = useCallback((event: DragCancelEvent) => {
    setDraggedTaskId(null)
    // Remove focus from the drag handle after cancel
    if (document.activeElement instanceof HTMLElement) {
      document.activeElement.blur()
    }
  }, [])

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
          <TreeGridToolbar
            displayedRowCount={displayedRowCount}
            totalRowCount={totalRowCount}
            searchValue={searchValue}
            onSearchChange={onSearchChange}
            onRefresh={refetch}
            onClearFilters={onClearFilters}
            hasActiveFilters={hasActiveFilters}
            onExportCsv={onExportCsv}
            isLoading={isLoading}
            leftSlot={
              canManageTasks && (
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={handleCreateTask}
                  disabled={!canCreateDraftTask}
                >
                  Create Task
                </Button>
              )
            }
            helpContent={<ProjectTasksHelp />}
          />

          <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragStart={handleDragStart}
            onDragEnd={handleDragEnd}
            onDragCancel={handleDragCancel}
          >
            <SortableContext items={flattenedTasks.map((t) => t.node.id)}>
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
                                <ArrowUpOutlined />
                              ) : sortState === 'desc' ? (
                                <ArrowDownOutlined />
                              ) : null

                            const handleSortClick = canSort
                              ? (e: React.MouseEvent) => {
                                  // Skip sorting if we just finished resizing
                                  if (isResizingRef.current) {
                                    isResizingRef.current = false
                                    return
                                  }
                                  header.column.getToggleSortingHandler()?.(e)
                                }
                              : undefined

                            const handleResizeStart = (
                              e: React.MouseEvent | React.TouchEvent,
                            ) => {
                              isResizingRef.current = true
                              header.getResizeHandler()(e)
                            }

                            return (
                              <th
                                key={header.id}
                                className={`${styles.th}${
                                  canSort ? ` ${styles.thSortable}` : ''
                                }${canResize ? ` ${styles.thResizable}` : ''}`}
                                onClick={handleSortClick}
                              >
                                <span className={styles.thContent}>
                                  <span className={styles.thText}>
                                    {header.isPlaceholder
                                      ? null
                                      : flexRender(
                                          header.column.columnDef.header,
                                          header.getContext(),
                                        )}
                                  </span>
                                  {sortIcon}
                                </span>

                                {canResize && (
                                  <span
                                    role="separator"
                                    aria-orientation="vertical"
                                    onMouseDown={handleResizeStart}
                                    onTouchStart={handleResizeStart}
                                    onDoubleClick={() =>
                                      header.column.resetSize()
                                    }
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

                        <tr
                          key={`${headerGroup.id}-filters`}
                          data-role="column-filters"
                        >
                          {headerGroup.headers.map((header) => {
                            const column = header.column

                            if (
                              !column.getCanFilter() ||
                              header.isPlaceholder
                            ) {
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
                            ) as string[]
                            const options =
                              colId === 'type'
                                ? taskTypeFilterOptions
                                : colId === 'status'
                                  ? taskStatusFilterOptions
                                  : colId === 'priority'
                                    ? taskPriorityFilterOptions
                                    : []

                            const numericRangePlaceholder =
                              colId === 'progress' ||
                              colId === 'estimatedEffortHours'
                                ? 'e.g. >=10 or 2-6'
                                : undefined

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
                                        .map((v) =>
                                          String(v.label ?? v.value ?? ''),
                                        )
                                        .filter(Boolean)
                                      return labels.length === 1
                                        ? labels[0]
                                        : `${labels.length} selected`
                                    }}
                                    value={
                                      selectValue.length
                                        ? selectValue
                                        : undefined
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
                                    placeholder={numericRangePlaceholder}
                                    value={textValue}
                                    onChange={(e) => {
                                      const next = e.target.value
                                      column.setFilterValue(
                                        next ? next : undefined,
                                      )
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
                        const isDragging = draggedTaskId === row.original.id
                        const isDraftTask = row.original.id.startsWith('draft-')
                        const rowElements = [
                          <TreeGridSortableRow
                            key={row.id}
                            nodeId={row.original.id}
                            isDragEnabled={isDragEnabled && !isDraftTask}
                            isDragging={isDragging}
                            className={`${styles.tr}${index % 2 === 1 ? ` ${styles.trAlt}` : ''}${isSelected ? ` ${styles.trSelected}` : ''}`}
                            onClick={(e) => {
                              void handleRowClick(e, {
                                rowId: row.original.id,
                                isEditableColumn: (columnId) =>
                                  editableColumns.includes(columnId),
                                getClickedColumnId: (target) =>
                                  target
                                    .closest('td')
                                    ?.getAttribute('data-column-id') ?? null,
                              })
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
                                'assignees',
                              ]
                              const isEditableCell =
                                isSelected &&
                                editableCells.includes(cell.column.id)

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
                          </TreeGridSortableRow>,
                        ]

                        // Add error row if row is selected and has field errors
                        if (isSelected && Object.keys(fieldErrors).length > 0) {
                          const errorItems = Object.entries(fieldErrors).map(
                            ([field, error]) => (
                              <div
                                key={field}
                                className={styles.validationErrorItem}
                              >
                                <span className={styles.validationErrorField}>
                                  {field}:
                                </span>{' '}
                                {error}
                              </div>
                            ),
                          )

                          rowElements.push(
                            <tr
                              key={`${row.id}-errors`}
                              className={`${styles.tr} ${styles.validationErrorRow}`}
                            >
                              <td
                                colSpan={columns.length}
                                className={`${styles.td} ${styles.validationErrorCell}`}
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
            </SortableContext>
          </DndContext>
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
