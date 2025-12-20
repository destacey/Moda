'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { CreateProjectTaskRequest } from '@/src/services/moda-api'
import {
  useCreateProjectTaskMutation,
  useGetTaskPriorityOptionsQuery,
  useGetTaskTypeOptionsQuery,
  useGetParentTaskOptionsQuery,
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
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio
const { RangePicker } = DatePicker

export interface CreateProjectTaskFormProps {
  projectIdOrKey: string
  parentTaskId?: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreateProjectTaskFormValues {
  name: string
  description?: string
  type: number
  priority?: number
  parentId?: string
  plannedRange?: any[]
  plannedDate?: Date
  estimatedEffortHours?: number
}

const mapToRequestValues = (
  values: CreateProjectTaskFormValues,
): CreateProjectTaskRequest => {
  return {
    name: values.name,
    description: values.description,
    typeId: values.type,
    priorityId: values.priority,
    parentId: values.parentId,
    plannedStart: (values.plannedRange?.[0] as any)?.format('YYYY-MM-DD'),
    plannedEnd: (values.plannedRange?.[1] as any)?.format('YYYY-MM-DD'),
    plannedDate: (values.plannedDate as any)?.format('YYYY-MM-DD'),
    estimatedEffortHours: values.estimatedEffortHours,
  } as CreateProjectTaskRequest
}

const CreateProjectTaskForm = (props: CreateProjectTaskFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateProjectTaskFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [createProjectTask, { error: mutationError }] =
    useCreateProjectTaskMutation()

  const { data: priorityOptions = [] } = useGetTaskPriorityOptionsQuery({
    projectIdOrKey: props.projectIdOrKey,
  })

  const { data: typeOptions = [] } = useGetTaskTypeOptionsQuery({
    projectIdOrKey: props.projectIdOrKey,
  })

  const { data: parentTaskOptions = [] } = useGetParentTaskOptionsQuery({
    projectIdOrKey: props.projectIdOrKey,
  })

  const { hasPermissionClaim } = useAuth()
  const canCreateTask = hasPermissionClaim('Permissions.Projects.Create')

  const selectedType = Form.useWatch('type', form)
  // Find the Milestone value dynamically from options
  const milestoneValue = typeOptions.find(
    (opt) => opt.label === 'Milestone',
  )?.value
  const isMilestone = selectedType === milestoneValue

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const create = async (values: CreateProjectTaskFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await createProjectTask({
        projectIdOrKey: props.projectIdOrKey,
        request,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        'Task created successfully. Task key: ' + response.data.taskKey,
      )

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while creating the project. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await create(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while creating the task. Please try again.',
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
    if (canCreateTask) {
      setIsOpen(props.showForm)
      if (props.showForm && props.parentTaskId) {
        form.setFieldValue('parentId', props.parentTaskId)
      }
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to create tasks.')
    }
  }, [canCreateTask, form, messageApi, props])

  return (
    <Modal
      title="Create Task"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid || isSaving }}
      okText="Create"
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
        name="create-project-task-form"
        initialValues={{
          type: typeOptions.find((opt) => opt.label === 'Task')?.value ?? 1,
          priority:
            priorityOptions.find((opt) => opt.label === 'Medium')?.value ?? 2,
          parentId: props.parentTaskId,
        }}
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
          name="type"
          label="Type"
          rules={[{ required: true, message: 'Please select a type' }]}
        >
          <RadioGroup
            options={typeOptions}
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
                placeholder="Estimated hours"
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

export default CreateProjectTaskForm
