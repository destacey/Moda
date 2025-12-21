'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { UpdateProjectTaskRequest } from '@/src/services/moda-api'
import {
  useGetProjectTaskQuery,
  useGetTaskStatusOptionsQuery,
  useGetTaskPriorityOptionsQuery,
  useGetParentTaskOptionsQuery,
  useUpdateProjectTaskMutation,
} from '@/src/store/features/ppm/project-tasks-api'
import { toFormErrors } from '@/src/utils'
import {
  DatePicker,
  Form,
  Input,
  InputNumber,
  Modal,
  Radio,
  TreeSelect,
} from 'antd'
import dayjs from 'dayjs'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio
const { RangePicker } = DatePicker

export interface EditProjectTaskFormProps {
  projectIdOrKey: string
  taskIdOrKey: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditProjectTaskFormValues {
  name: string
  description?: string
  status: number
  priority?: number
  parentId?: string
  plannedRange?: any[]
  plannedDate?: Date
  estimatedEffortHours?: number
}

const mapToRequestValues = (
  values: EditProjectTaskFormValues,
  taskId: string,
): UpdateProjectTaskRequest => {
  return {
    id: taskId,
    name: values.name,
    description: values.description,
    statusId: values.status,
    priorityId: values.priority,
    parentId: values.parentId,
    plannedStart: (values.plannedRange?.[0] as any)?.format('YYYY-MM-DD'),
    plannedEnd: (values.plannedRange?.[1] as any)?.format('YYYY-MM-DD'),
    plannedDate: (values.plannedDate as any)?.format('YYYY-MM-DD'),
    estimatedEffortHours: values.estimatedEffortHours,
  } as UpdateProjectTaskRequest
}

const EditProjectTaskForm = (props: EditProjectTaskFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditProjectTaskFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [updateProjectTask, { error: mutationError }] =
    useUpdateProjectTaskMutation()

  const {
    data: taskData,
    isLoading,
    error,
  } = useGetProjectTaskQuery({
    projectIdOrKey: props.projectIdOrKey,
    idOrTaskKey: props.taskIdOrKey,
  })

  const { data: statusOptions = [] } = useGetTaskStatusOptionsQuery()

  const { data: priorityOptions = [] } = useGetTaskPriorityOptionsQuery()

  const { data: parentTaskOptions = [] } = useGetParentTaskOptionsQuery({
    projectIdOrKey: props.projectIdOrKey,
    excludeTaskId: taskData?.id,
  })

  const { hasPermissionClaim } = useAuth()
  const canUpdateTask = hasPermissionClaim('Permissions.Projects.Update')

  const taskType = taskData?.type?.name
  const isMilestone = taskType === 'Milestone'

  const mapToFormValues = useCallback(
    (task: any) => {
      if (!task) {
        throw new Error('Task not found')
      }
      const plannedRange =
        task.plannedStart && task.plannedEnd
          ? [dayjs(task.plannedStart), dayjs(task.plannedEnd)]
          : undefined

      form.setFieldsValue({
        name: task.name,
        description: task.description,
        status: task.status.id,
        priority: task.priority?.id,
        parentId: task.parent?.id,
        plannedRange,
        plannedDate: task.plannedDate ? dayjs(task.plannedDate) : undefined,
        estimatedEffortHours: task.estimatedEffortHours,
      })
    },
    [form],
  )

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const update = async (values: EditProjectTaskFormValues) => {
    if (!taskData) {
      messageApi.error('Task data is not loaded.')
      return false
    }

    try {
      const request = mapToRequestValues(values, taskData.id)
      const response = await updateProjectTask({
        projectIdOrKey: props.projectIdOrKey,
        request,
        cacheKey: props.taskIdOrKey,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success('Task updated successfully.')

      return true
    } catch (error) {
      console.error('update error', error)
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the project. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while updating the task. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    form.resetFields()
    props.onFormCancel()
  }, [form, props])

  useEffect(() => {
    if (!taskData) return
    if (canUpdateTask) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(taskData)
      }
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to update tasks.')
    }
  }, [canUpdateTask, mapToFormValues, messageApi, taskData, props])

  if (isLoading) {
    return null
  }

  return (
    <Modal
      title="Edit Task"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid || isSaving }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      mask={{ blur: false }}
      maskClosable={false}
      keyboard={false}
      destroyOnHidden={true}
      width={500}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="edit-project-task-form"
      >
        <Item name="parentId" label="Parent Task">
          <TreeSelect
            allowClear
            placeholder="Select parent task (optional)"
            treeData={parentTaskOptions}
            treeDefaultExpandAll
            showSearch={{
              filterTreeNode: (input, node) =>
                node.title
                  ?.toString()
                  .toLowerCase()
                  .includes(input.toLowerCase()),
            }}
          />
        </Item>
        <Item
          label="Name"
          name="name"
          rules={[
            { required: true, message: 'Name is required' },
            { max: 256 },
          ]}
        >
          <TextArea
            autoSize={{ minRows: 1, maxRows: 2 }}
            showCount
            maxLength={256}
          />
        </Item>
        <Item name="description" label="Description" rules={[{ max: 2048 }]}>
          <MarkdownEditor maxLength={2048} />
        </Item>

        <Item
          name="status"
          label="Status"
          rules={[{ required: true, message: 'Please select a status' }]}
        >
          <RadioGroup
            options={statusOptions}
            optionType="button"
            buttonStyle="solid"
          />
        </Item>

        <Item
          name="priority"
          label="Priority"
          rules={[{ required: true, message: 'Please select a priority' }]}
        >
          <RadioGroup
            options={priorityOptions}
            optionType="button"
            buttonStyle="solid"
          />
        </Item>

        {!isMilestone ? (
          <>
            <Item name="plannedRange" label="Planned Date Range">
              <RangePicker style={{ width: '60%' }} format="MMM D, YYYY" />
            </Item>

            <Item name="estimatedEffortHours" label="Estimated Effort (hours)">
              <InputNumber
                min={0}
                step={0.25}
                style={{ width: '33%' }}
                placeholder="Enter estimated hours"
              />
            </Item>
          </>
        ) : (
          <Item name="plannedDate" label="Planned Date">
            <DatePicker style={{ width: '60%' }} format="MMM D, YYYY" />
          </Item>
        )}
      </Form>
    </Modal>
  )
}

export default EditProjectTaskForm
