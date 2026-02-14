'use client'

import { ProjectTaskTreeDto } from '@/src/services/moda-api'
import { Button, Form } from 'antd'
import { PlusOutlined } from '@ant-design/icons'
import { useCallback, useEffect, useMemo, useRef } from 'react'
import dayjs from 'dayjs'

import { useMessage } from '@/src/components/contexts/messaging'
import {
  type DraftItem,
  type MoveValidator,
  type TreeGridHandle,
  defaultMoveValidator,
  findNodeById,
  TreeGrid,
} from '@/src/components/common/tree-grid'
import CreateProjectTaskForm from './create-project-task-form'
import DeleteProjectTaskForm from './delete-project-task-form'
import EditProjectTaskForm from './edit-project-task-form'
import { buildProjectTaskPatchOperations } from './project-task-patch'
import { getProjectTasksTableColumns } from './project-tasks-table.columns'
import { ProjectTasksHelp } from './project-tasks-table.keyboard-shortcuts'
import {
  useGetTaskStatusOptionsQuery,
  useGetTaskPriorityOptionsQuery,
  useGetTaskTypeOptionsQuery,
} from '@/src/store/features/ppm/project-tasks-api'
import {
  usePatchProjectTaskMutation,
  useUpdateProjectTaskPlacementMutation,
  useCreateProjectTaskMutation,
} from '@/src/store/features/ppm/project-tasks-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useState } from 'react'

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
  const [form] = Form.useForm()
  const treeGridRef = useRef<TreeGridHandle>(null)
  const messageApi = useMessage()

  // Modal form state
  const [openCreateTaskForm, setOpenCreateTaskForm] = useState(false)
  const [openEditTaskForm, setOpenEditTaskForm] = useState(false)
  const [openDeleteTaskForm, setOpenDeleteTaskForm] = useState(false)
  const [selectedTaskId, setSelectedTaskId] = useState<string | undefined>()

  // Field errors (owned here for Form.useWatch access)
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({})

  // Keep a ref to drafts for use in handleUpdateTask
  const draftsRef = useRef<DraftItem[]>([])

  const { data: taskStatusOptions = [] } = useGetTaskStatusOptionsQuery()
  const { data: taskStatusOptionsForMilestone = [] } =
    useGetTaskStatusOptionsQuery({ forMilestone: true })
  const { data: taskPriorityOptions = [] } = useGetTaskPriorityOptionsQuery()
  const { data: taskTypeOptions = [] } = useGetTaskTypeOptionsQuery()
  const { data: employeeOptions = [] } = useGetEmployeeOptionsQuery(false)

  const milestoneTypeValue = useMemo(
    () => taskTypeOptions.find((opt) => opt.label === 'Milestone')?.value,
    [taskTypeOptions],
  )

  const [patchProjectTask] = usePatchProjectTaskMutation()
  const [updateProjectTaskPlacement] = useUpdateProjectTaskPlacementMutation()
  const [createProjectTask] = useCreateProjectTaskMutation()

  // Track the selected row's type to dynamically show/hide milestone-specific fields
  const selectedTypeId = Form.useWatch('typeId', form)
  const isSelectedRowMilestone = selectedTypeId === milestoneTypeValue

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

  // Clear date-pair validation errors as soon as the user fixes the values
  const watchedPlannedStart = Form.useWatch('plannedStart', form)
  const watchedPlannedEnd = Form.useWatch('plannedEnd', form)
  const selectedRowId = treeGridRef.current?.selectedRowId ?? null

  useEffect(() => {
    if (!selectedRowId) return
    if (!fieldErrors.plannedStart && !fieldErrors.plannedEnd) return

    const task = findNodeById(tasks, selectedRowId) as ProjectTaskTreeDto | null
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
    tasks,
    watchedPlannedEnd,
    watchedPlannedStart,
  ])

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

  const projectTaskMoveValidator: MoveValidator<ProjectTaskTreeDto> =
    useCallback((activeNode, targetParentNode, targetParentId) => {
      const result = defaultMoveValidator(
        activeNode,
        targetParentNode,
        targetParentId,
      )
      if (!result.canMove) return result
      if (targetParentNode?.type?.name === 'Milestone') {
        return { canMove: false, reason: 'Milestones cannot have child tasks' }
      }
      return { canMove: true }
    }, [])

  const handleNodeMove = useCallback(
    async (nodeId: string, parentId: string | null, order: number) => {
      try {
        await updateProjectTaskPlacement({
          projectIdOrKey: projectKey,
          id: nodeId,
          request: {
            taskId: nodeId,
            parentId: parentId ?? undefined,
            order,
          },
        }).unwrap()
        await refetch()
      } catch (error: any) {
        messageApi.error(
          error?.data?.detail || 'Failed to move task. Please try again.',
        )
      }
    },
    [projectKey, updateProjectTaskPlacement, refetch, messageApi],
  )

  const handleUpdateTask = useCallback(
    async (taskId: string, updates: Partial<any>): Promise<boolean> => {
      if (!projectKey) return false

      const isDraft = taskId.startsWith('draft-')

      if (isDraft) {
        try {
          const draft = draftsRef.current.find((d) => d.id === taskId)
          if (!draft) return false

          const request: any = {
            name: updates.name || '',
            typeId: updates.typeId || 1,
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

          if (response.error) throw response.error

          messageApi.success(`Task created: ${response.data.key}`)
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
                  const input = cellElement.querySelector(
                    'input',
                  ) as HTMLElement
                  input?.focus()
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
        const task = findNodeById(tasks || [], taskId)
        if (!task) return false

        try {
          const patchOperations = buildProjectTaskPatchOperations(updates)
          const response = await patchProjectTask({
            projectIdOrKey: projectKey,
            taskId,
            patchOperations,
            cacheKey: taskId,
          })
          if (response.error) throw response.error
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
                  const input = cellElement.querySelector(
                    'input',
                  ) as HTMLElement
                  input?.focus()
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
      createProjectTask,
    ],
  )

  const handleEditTask = useCallback((task: any) => {
    setSelectedTaskId(task.id)
    setOpenEditTaskForm(true)
  }, [])

  const handleDeleteTask = useCallback((task: any) => {
    setSelectedTaskId(task.id)
    setOpenDeleteTaskForm(true)
  }, [])

  const onEditTaskFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditTaskForm(false)
      setSelectedTaskId(undefined)
      if (wasSaved) refetch()
    },
    [refetch],
  )

  const onDeleteTaskFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteTaskForm(false)
      setSelectedTaskId(undefined)
      if (wasDeleted) refetch()
    },
    [refetch],
  )

  const onCreateTaskFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCreateTaskForm(false)
      if (wasSaved) refetch()
    },
    [refetch],
  )

  const getFormValues = useCallback(
    (rowId: string, data: ProjectTaskTreeDto[]) => {
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
    [],
  )

  const computeChanges = useCallback(
    (
      rowId: string,
      formValues: Record<string, any>,
      data: ProjectTaskTreeDto[],
    ) => {
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
    [isSelectedRowMilestone],
  )

  const validateFields = useCallback(
    (rowId: string, formValues: Record<string, any>) => {
      const isDraft = rowId.startsWith('draft-')
      const task = findNodeById(tasks, rowId) as ProjectTaskTreeDto | null
      const isMilestone = isDraft
        ? isSelectedRowMilestone
        : task?.type?.name === 'Milestone'

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
    [isSelectedRowMilestone, tasks],
  )

  return (
    <>
      <Form form={form} component={false}>
        <TreeGrid<ProjectTaskTreeDto>
          ref={treeGridRef}
          data={tasks}
          isLoading={isLoading}
          columns={(ctx) =>
            getProjectTasksTableColumns({
              canManageTasks,
              selectedRowId: ctx.selectedRowId,
              handleEditTask,
              handleDeleteTask,
              handleUpdateTask,
              getFieldError: ctx.getFieldError,
              handleKeyDown: ctx.handleKeyDown,
              taskStatusOptions,
              taskStatusOptionsForMilestone,
              taskPriorityOptions,
              taskTypeOptions,
              employeeOptions,
              isDragEnabled: ctx.isDragEnabled,
              enableDragAndDrop,
              addDraftTaskAsChild: ctx.addDraftAsChild,
              canCreateTasks: ctx.canCreateDraft,
              isSelectedRowMilestone,
              taskTypeFilterOptions,
              taskStatusFilterOptions,
              taskPriorityFilterOptions,
            })
          }
          onRefresh={refetch}
          enableDragAndDrop={enableDragAndDrop}
          onNodeMove={handleNodeMove}
          onMoveRejected={(reason) =>
            messageApi.warning(reason || 'Cannot move task to this location')
          }
          moveValidator={projectTaskMoveValidator}
          editingConfig={{
            canEdit: canManageTasks,
            form,
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
              return rowId?.startsWith('draft-')
                ? ['name', 'type', ...base.slice(1)]
                : base
            },
            onSave: handleUpdateTask,
            getFormValues,
            computeChanges,
            validateFields,
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
          }}
          fieldErrors={fieldErrors}
          onFieldErrorsChange={setFieldErrors}
          createDraftNode={createDraftProjectTask}
          onDraftsChange={(drafts) => {
            draftsRef.current = drafts
          }}
          csvFileName="project-tasks"
          leftSlot={(ctx) =>
            canManageTasks && (
              <Button
                icon={<PlusOutlined />}
                onClick={() => ctx.addDraftAtRoot()}
                disabled={!ctx.canCreateDraft}
              >
                Create Task
              </Button>
            )
          }
          helpContent={<ProjectTasksHelp />}
          emptyMessage="No tasks found"
        />
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
