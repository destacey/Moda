import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import {
  CaretDownOutlined,
  CaretRightOutlined,
  DeleteOutlined,
  EditOutlined,
  HolderOutlined,
  MoreOutlined,
  PlusOutlined,
} from '@ant-design/icons'
import {
  Button,
  DatePicker,
  Dropdown,
  Flex,
  Form,
  Input,
  Select,
  Tag,
} from 'antd'
import { BaseOptionType } from 'antd/es/select'
import dayjs from 'dayjs'
import { ColumnDef } from '@tanstack/react-table'
import styles from './project-tasks-table.module.css'
import {
  numberRangeFilter,
  setContainsFilter,
} from './project-tasks-table.filters'
import { dateSortBy } from './project-tasks-table.sorting'
import { useDragHandle } from './project-task-sortable-row'

const { Item: FormItem } = Form

// Drag handle component that uses the context from the sortable row
function DragHandleCell({ isDragEnabled }: { isDragEnabled: boolean }) {
  const { listeners, attributes } = useDragHandle()

  if (!isDragEnabled) {
    return null
  }

  return (
    <div
      {...listeners}
      {...attributes}
      className={styles.dragHandle}
      onClick={(e) => e.stopPropagation()}
      title="Drag to reorder or change parent"
      style={{
        cursor: 'grab',
        touchAction: 'none',
      }}
    >
      <HolderOutlined style={{ fontSize: 14, color: '#8c8c8c' }} />
    </div>
  )
}

interface ProjectTasksTableColumnsParams {
  canManageTasks: boolean
  selectedRowId: string | null
  handleEditTask: (task: ProjectTaskTreeDto) => void
  handleDeleteTask: (task: ProjectTaskTreeDto) => void
  handleUpdateTask?: (taskId: string, updates: Record<string, any>) => unknown
  getFieldError: (fieldName: string) => string | undefined
  handleKeyDown: (
    e: React.KeyboardEvent,
    rowId: string,
    columnId: string,
  ) => Promise<void>
  taskStatusOptions: any[]
  taskStatusOptionsForMilestone: any[]
  taskPriorityOptions: any[]
  taskTypeOptions: any[]
  employeeOptions: BaseOptionType[]
  isDragEnabled?: boolean
  enableDragAndDrop?: boolean
  addDraftTaskAsChild?: (parentId: string) => void
  canCreateTasks?: boolean
  isSelectedRowMilestone?: boolean
}

export const getProjectTasksTableColumns = ({
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
  isDragEnabled = false,
  enableDragAndDrop = false,
  addDraftTaskAsChild,
  canCreateTasks = true,
  isSelectedRowMilestone = false,
}: ProjectTasksTableColumnsParams): ColumnDef<ProjectTaskTreeDto>[] => {
  return [
    ...(canManageTasks && enableDragAndDrop
      ? [
          {
            id: 'dragHandle',
            header: '',
            size: 32,
            enableSorting: false,
            enableGlobalFilter: false,
            enableColumnFilter: false,
            enableExport: false,
            cell: () => <DragHandleCell isDragEnabled={isDragEnabled} />,
          } as ColumnDef<ProjectTaskTreeDto>,
        ]
      : []),
    ...(canManageTasks
      ? [
          {
            id: 'actions',
            header: '',
            size: 40,
            enableSorting: false,
            enableGlobalFilter: false,
            enableColumnFilter: false,
            enableExport: false,
            cell: ({ row }: { row: any }) => {
              const isDraft = row.original.id.startsWith('draft-')
              // Don't show actions for draft tasks
              if (isDraft) return null

              return (
                <Dropdown
                  menu={{
                    items: [
                      {
                        key: 'edit',
                        label: 'Edit',
                        icon: <EditOutlined />,
                        onClick: () => handleEditTask(row.original),
                      },
                      {
                        key: 'delete',
                        label: 'Delete',
                        icon: <DeleteOutlined />,
                        onClick: () => handleDeleteTask(row.original),
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
          } as ColumnDef<ProjectTaskTreeDto>,
        ]
      : []),
    {
      accessorKey: 'wbs',
      header: 'WBS',
      size: 100,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: 'alphanumeric',
    },
    {
      accessorKey: 'key',
      header: 'Key',
      size: 120,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: 'alphanumeric',
      cell: (info) => info.getValue(),
    },
    {
      accessorKey: 'name',
      header: 'Name',
      size: 300,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: 'alphanumeric',
      cell: ({ row }: { row: any }) => {
        const depth = row.depth
        const task = row.original as ProjectTaskTreeDto
        const isSelected = selectedRowId === task.id
        const cellId = `${task.id}-name`
        const isDraft = task.id.startsWith('draft-')
        const isMilestone = task.type?.name === 'Milestone'

        return (
          <Flex
            className={styles.nameCell}
            data-cell-id={cellId}
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
                  onClick={row.getToggleExpandedHandler()}
                  className={styles.expanderBtn}
                />
              ) : (
                <span className={styles.indentSpacer} />
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
                <span style={{ flex: 1, minWidth: 0, overflow: 'hidden', textOverflow: 'ellipsis' }}>
                  {task.name || (isDraft ? '(New Task)' : '')}
                </span>
              )}
            </Flex>
            {!isSelected &&
              canManageTasks &&
              !isDraft &&
              !isMilestone &&
              addDraftTaskAsChild && (
                <Button
                  type="text"
                  size="small"
                  icon={<PlusOutlined />}
                  onClick={(e) => {
                    e.stopPropagation()
                    addDraftTaskAsChild(task.id)
                  }}
                  disabled={!canCreateTasks}
                  title="Add child task"
                  style={{
                    padding: '0 4px',
                    height: 20,
                    fontSize: 12,
                    flexShrink: 0,
                  }}
                />
              )}
          </Flex>
        )
      },
    },
    {
      id: 'type',
      accessorFn: (row) => row.type?.name ?? '',
      header: 'Type',
      size: 110,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: setContainsFilter,
      sortingFn: 'text',
      cell: ({ row }: { row: any }) => {
        const task = row.original as ProjectTaskTreeDto
        const isSelected = selectedRowId === task.id
        const isDraft = task.id.startsWith('draft-')
        const cellId = `${task.id}-type`

        if (isSelected && isDraft && handleUpdateTask) {
          return (
            <div data-cell-id={cellId}>
              <FormItem
                name="typeId"
                style={{ margin: 0 }}
                validateStatus={getFieldError('typeId') ? 'error' : ''}
              >
                <Select
                  size="small"
                  options={taskTypeOptions}
                  style={{ width: '100%' }}
                  onKeyDown={(e) => handleKeyDown(e, task.id, 'type')}
                  status={getFieldError('typeId') ? 'error' : ''}
                  onOpenChange={(open) => {
                    // When the dropdown closes after a selection, the Select
                    // loses focus — especially when changing Task↔Milestone
                    // which triggers a full column rebuild. Re-focus the
                    // Select's search input after React re-renders.
                    if (!open) {
                      setTimeout(() => {
                        const cell = document.querySelector(
                          `[data-cell-id="${cellId}"]`,
                        )
                        if (cell) {
                          const input = cell.querySelector(
                            '.ant-select-selection-search-input',
                          ) as HTMLElement
                          input?.focus()
                        }
                      }, 10)
                    }
                  }}
                />
              </FormItem>
            </div>
          )
        }

        return task.type?.name ?? ''
      },
    },
    {
      id: 'status',
      accessorFn: (row) => row.status?.name ?? '',
      header: 'Status',
      size: 130,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: setContainsFilter,
      sortingFn: 'text',
      cell: (info) => {
        const task = info.row.original as ProjectTaskTreeDto
        const status = (info.getValue() as string) ?? ''
        const isSelected = selectedRowId === task.id
        const isDraft = task.id.startsWith('draft-')
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

        const isMilestone = isSelected
          ? isDraft
            ? isSelectedRowMilestone
            : task.type?.name === 'Milestone'
          : task.type?.name === 'Milestone'
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
      filterFn: setContainsFilter,
      sortingFn: 'text',
      cell: (info) => {
        const task = info.row.original as ProjectTaskTreeDto
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
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: dateSortBy((row: any) => {
        const isMilestone = row.original.type?.name === 'Milestone'
        return isMilestone
          ? row.original.plannedDate
          : row.original.plannedStart
      }),
      cell: (info) => {
        const task = info.row.original as ProjectTaskTreeDto
        const value = (info.getValue() as string) ?? ''
        const isSelected = selectedRowId === task.id
        const isDraft = task.id.startsWith('draft-')
        const cellId = `${task.id}-plannedStart`
        const isMilestone = isSelected
          ? isDraft
            ? isSelectedRowMilestone
            : task.type?.name === 'Milestone'
          : task.type?.name === 'Milestone'

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
                status={error ? 'error' : ''}
              />
            </FormItem>
          </div>
        )
      },
    },
    {
      id: 'plannedEnd',
      accessorFn: (row) =>
        row.plannedEnd ? dayjs(row.plannedEnd).format('MMM D, YYYY') : '',
      header: 'Planned End',
      size: 130,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: dateSortBy((row: any) => row.original.plannedEnd),
      cell: (info) => {
        const task = info.row.original as ProjectTaskTreeDto
        const value = (info.getValue() as string) ?? ''
        const isSelected = selectedRowId === task.id
        const isDraft = task.id.startsWith('draft-')
        const cellId = `${task.id}-plannedEnd`
        const isMilestone = isSelected
          ? isDraft
            ? isSelectedRowMilestone
            : task.type?.name === 'Milestone'
          : task.type?.name === 'Milestone'

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
                status={error ? 'error' : ''}
              />
            </FormItem>
          </div>
        )
      },
    },
    {
      id: 'assignees',
      accessorFn: (row) => row.assignees?.map((a) => a.name).join(', ') ?? '',
      header: 'Assignees',
      size: 250,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: 'includesString',
      sortingFn: 'text',
      cell: (info) => {
        const task = info.row.original as ProjectTaskTreeDto
        const assignees = task.assignees ?? []
        const isSelected = selectedRowId === task.id
        const cellId = `${task.id}-assignees`

        if (!isSelected || !handleUpdateTask) {
          if (assignees.length === 0) return ''
          return assignees.map((a) => a.name).join(', ')
        }

        const error = getFieldError('assigneeIds')
        return (
          <div data-cell-id={cellId}>
            <FormItem
              name="assigneeIds"
              style={{ margin: 0 }}
              validateStatus={error ? 'error' : ''}
            >
              <Select
                size="small"
                mode="multiple"
                allowClear
                placeholder="Select assignees"
                optionFilterProp="label"
                filterOption={(input, option) =>
                  (option?.label?.toString().toLowerCase() ?? '').includes(
                    input.toLowerCase(),
                  )
                }
                options={employeeOptions}
                onKeyDown={(e) => handleKeyDown(e, task.id, 'assignees')}
                status={error ? 'error' : ''}
                maxTagCount={1}
                maxTagPlaceholder={(omitted) => `+${omitted.length}`}
              />
            </FormItem>
          </div>
        )
      },
    },
    {
      id: 'progress',
      accessorFn: (row) => row.progress ?? undefined,
      header: 'Progress',
      size: 90,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: numberRangeFilter,
      cell: (info) => {
        const task = info.row.original as ProjectTaskTreeDto
        const value = task.progress
        const isSelected = selectedRowId === task.id
        const isDraft = task.id.startsWith('draft-')
        const cellId = `${task.id}-progress`
        const isMilestone = isSelected
          ? isDraft
            ? isSelectedRowMilestone
            : task.type?.name === 'Milestone'
          : task.type?.name === 'Milestone'

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
      sortingFn: 'basic',
      sortUndefined: -1,
    },
    {
      id: 'estimatedEffortHours',
      accessorFn: (row) => row.estimatedEffortHours ?? undefined,
      header: 'Est Effort (hrs)',
      size: 90,
      enableGlobalFilter: true,
      enableColumnFilter: true,
      filterFn: numberRangeFilter,
      cell: (info) => {
        const task = info.row.original as ProjectTaskTreeDto
        const value = task.estimatedEffortHours
        const isSelected = selectedRowId === task.id
        const isDraft = task.id.startsWith('draft-')
        const cellId = `${task.id}-estimatedEffortHours`
        const isMilestone = isSelected
          ? isDraft
            ? isSelectedRowMilestone
            : task.type?.name === 'Milestone'
          : task.type?.name === 'Milestone'

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
      sortingFn: 'basic',
      sortUndefined: -1,
    },
  ]
}
