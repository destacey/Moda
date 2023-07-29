'use client'

import { DatePicker, Form, Input, Modal, message } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import { CreateProgramIncrementRequest } from '@/src/services/moda-api'
import { getProgramIncrementsClient } from '@/src/services/clients'
import { toFormErrors } from '@/src/utils'

export interface CreateProgramIncrementFormProps {
  showForm: boolean
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateProgramIncrementFormValues {
  name: string
  description?: string
  start: Date
  end: Date
}

const CreateProgramIncrementForm = ({
  showForm,
  onFormCreate,
  onFormCancel,
}: CreateProgramIncrementFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateProgramIncrementFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canCreateProgramIncrement = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Create'
  )

  const mapToRequestValues = (values: CreateProgramIncrementFormValues) => {
    return {
      name: values.name,
      description: values.description,
      start: (values.start as any)?.format('YYYY-MM-DD'),
      end: (values.end as any)?.format('YYYY-MM-DD'),
    } as CreateProgramIncrementRequest
  }

  const createProgramIncrement = async (
    values: CreateProgramIncrementFormValues
  ) => {
    const request = mapToRequestValues(values)
    const programIncrementsClient = await getProgramIncrementsClient()
    return await programIncrementsClient.create(request)
  }

  const create = async (
    values: CreateProgramIncrementFormValues
  ): Promise<boolean> => {
    try {
      await createProgramIncrement(values)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while creating the program increment.'
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
        messageApi.success('Successfully created program increment.')
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
    if (canCreateProgramIncrement) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error(
        'You do not have permission to create program increments.'
      )
    }
  }, [canCreateProgramIncrement, onFormCancel, showForm, messageApi])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false)
    )
  }, [form, formValues])

  return (
    <>
      {contextHolder}
      <Modal
        title="Create Program Increment"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        closable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="create-program-increment-form"
        >
          <Form.Item label="Name" name="name" rules={[{ required: true }]}>
            <Input.TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Form.Item>
          <Form.Item
            name="description"
            label="Description"
            help="Markdown enabled"
          >
            <Input.TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Form.Item>
          <Form.Item label="Start" name="start" rules={[{ required: true }]}>
            <DatePicker />
          </Form.Item>
          <Form.Item label="End" name="end" rules={[{ required: true }]}>
            <DatePicker />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateProgramIncrementForm
