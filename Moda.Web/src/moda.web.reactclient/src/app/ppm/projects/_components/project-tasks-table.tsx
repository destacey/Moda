'use client'

import styles from './project-tasks-table.module.css'
import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import { ModaEmpty } from '@/src/components/common'
import { Form, Input, Select, Spin } from 'antd'
import {
  ArrowUpOutlined,
  ArrowDownOutlined,
  FilterOutlined,
} from '@ant-design/icons'
import {
  Fragment,
  type ChangeEvent,
  useCallback,
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
} from '@/src/store/features/ppm/project-tasks-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'

import CreateProjectTaskForm from './create-project-task-form'
import DeleteProjectTaskForm from './delete-project-task-form'
import EditProjectTaskForm from './edit-project-task-form'
import { countProjectTasks, findProjectTaskById } from './project-task-tree'
import { buildProjectTaskPatchOperations } from './project-task-patch'
import ProjectTasksTableToolbar from './project-tasks-table-toolbar'
import { getProjectTasksTableColumns } from './project-tasks-table.columns'
import { useProjectTasksInlineEditing } from './project-tasks-table.inline-editing'
import { stringContainsFilter } from './project-tasks-table.filters'
import { ProjectTaskSortableRow } from './project-task-sortable-row'
import {
  flattenTree,
  getProjection,
  calculateOrderInParent,
  INDENTATION_WIDTH,
  DRAG_ACTIVATION_DISTANCE,
} from './project-task-tree-dnd-utils'

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

  const { data: employeeOptions = [] } = useGetEmployeeOptionsQuery(false)

  const [patchProjectTask] = usePatchProjectTaskMutation()
  const [updateProjectTaskPlacement] = useUpdateProjectTaskPlacementMutation()

  // Flatten tree for drag-and-drop
  const flattenedTasks = useMemo(() => {
    return flattenTree(tasks)
  }, [tasks])

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
    async (taskId: string, updates: Partial<any>) => {
      if (!projectKey) return

      const task = findProjectTaskById(tasks || [], taskId)
      if (!task) return

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

  const {
    tableRef,
    selectedRowId,
    setSelectedRowId,
    getFieldError,
    editableColumns,
    handleKeyDown,
    handleRowClick,
  } = useProjectTasksInlineEditing({
    tasks,
    canManageTasks,
    form,
    tableWrapperClassName: styles.tableWrapper,
    onUpdateTask: handleUpdateTask,
    fieldErrors,
    setFieldErrors,
  })

  // Task form handlers
  const handleCreateTask = useCallback(() => {
    setOpenCreateTaskForm(true)
  }, [])

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
    return countProjectTasks(tasks)
  }, [tasks])

  // Early calculation for isDragEnabled (before columns definition)
  const hasFilters =
    !!searchValue || columnFilters.length > 0 || sorting.length > 0
  const isEditing = selectedRowId !== null
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
        employeeOptions,
        isDragEnabled,
        enableDragAndDrop,
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
      employeeOptions,
      isDragEnabled,
      enableDragAndDrop,
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
      )

      if (!projection.canDrop) {
        messageApi.warning(
          projection.reason || 'Cannot move task to this location',
        )
        return
      }

      // Calculate new order
      const overIndex = flattenedTasks.findIndex((t) => t.id === over.id)
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
          <ProjectTasksTableToolbar
            canManageTasks={canManageTasks}
            displayedRowCount={displayedRowCount}
            totalRowCount={totalRowCount}
            searchValue={searchValue}
            onSearchChange={onSearchChange}
            refetch={refetch}
            onClearFilters={onClearFilters}
            hasActiveFilters={hasActiveFilters}
            onExportCsv={onExportCsv}
            isLoading={isLoading}
            onCreateTask={handleCreateTask}
          />

          <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragStart={handleDragStart}
            onDragEnd={handleDragEnd}
            onDragCancel={handleDragCancel}
          >
            <SortableContext items={flattenedTasks.map((t) => t.id)}>
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
                        const rowElements = [
                          <ProjectTaskSortableRow
                            key={row.id}
                            taskId={row.original.id}
                            isDragEnabled={isDragEnabled}
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
                          </ProjectTaskSortableRow>,
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
