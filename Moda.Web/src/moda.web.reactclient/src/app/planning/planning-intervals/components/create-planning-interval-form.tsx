'use client'

import {
  DatePicker,
  Form,
  Input,
  InputNumber,
  Modal,
  Typography,
  message,
} from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../../components/contexts/auth'
import { CreatePlanningIntervalRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import { useCreatePlanningIntervalMutation } from '@/src/services/queries/planning-queries'
import { MarkdownEditor } from '@/src/app/components/common/markdown'

const { Item } = Form
const { TextArea } = Input
const { Text } = Typography

export interface CreatePlanningIntervalFormProps {
  showForm: boolean
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreatePlanningIntervalFormValues {
  name: string
  description?: string
  start: Date
  end: Date
  iterationWeeks: number
  iterationPrefix?: string
}

const mapToRequestValues = (
  values: CreatePlanningIntervalFormValues,
): CreatePlanningIntervalRequest => {
  return {
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    iterationWeeks: values.iterationWeeks,
    iterationPrefix: values.iterationPrefix,
  }
}

const CreatePlanningIntervalForm = ({
  showForm,
  onFormCreate,
  onFormCancel,
}: CreatePlanningIntervalFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreatePlanningIntervalFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const createPlanningInterval = useCreatePlanningIntervalMutation()

  const { hasClaim } = useAuth()
  const canCreatePlanningInterval = hasClaim(
    'Permission',
    'Permissions.PlanningIntervals.Create',
  )

  const create = async (
    values: CreatePlanningIntervalFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      await createPlanningInterval.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while creating the planning interval.',
        )
        console.error(error)
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
        onFormCreate()
        messageApi.success('Successfully created planning interval.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }

  useEffect(() => {
    if (canCreatePlanningInterval) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error(
        'You do not have permission to create planning intervals.',
      )
    }
  }, [canCreatePlanningInterval, onFormCancel, showForm, messageApi])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      {contextHolder}
      <Modal
        title="Create Planning Interval"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="create-planning-interval-form"
        >
          <Item label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Item>
          <Item
            name="description"
            label="Description"
            initialValue=""
            rules={[{ max: 2048 }]}
          >
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={2048}
            />
          </Item>
          <Item label="Start" name="start" rules={[{ required: true }]}>
            <DatePicker />
          </Item>
          <Item label="End" name="end" rules={[{ required: true }]}>
            <DatePicker />
          </Item>
          <Item
            label="Iteration Weeks"
            name="iterationWeeks"
            rules={[{ required: true }]}
          >
            <InputNumber min={1} max={10} />
          </Item>
          <Item
            label="Iteration Prefix"
            name="iterationPrefix"
            extra="Iteration Name Template: Iteration Prefix + Iteration Number"
          >
            <Input />
          </Item>
          <>
            {formValues &&
              formValues.iterationPrefix &&
              formValues.iterationPrefix != null && (
                <Text type="secondary">
                  Iteration name format: {formValues.iterationPrefix}1,{' '}
                  {formValues.iterationPrefix}2, ...
                </Text>
              )}
          </>
        </Form>
      </Modal>
    </>
  )
}

export default CreatePlanningIntervalForm
