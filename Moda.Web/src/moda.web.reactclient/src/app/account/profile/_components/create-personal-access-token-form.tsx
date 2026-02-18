'use client'

import { Form, Input, Modal, DatePicker, Alert } from 'antd'
import { useEffect, useState } from 'react'
import { useMessage } from '@/src/components/contexts/messaging'
import dayjs, { Dayjs } from 'dayjs'
import { ExclamationCircleOutlined } from '@ant-design/icons'
import { useCreatePersonalAccessTokenMutation } from '@/src/store/features/user-management/personal-access-tokens-api'
import { toFormErrors } from '@/src/utils'

const { Item } = Form

export interface CreatePersonalAccessTokenFormProps {
  showForm: boolean
  onFormCreate: (token: string) => void
  onFormCancel: () => void
}

interface CreateTokenFormValues {
  name: string
  expiresAt: Dayjs
}

const CreatePersonalAccessTokenForm = ({
  showForm,
  onFormCreate,
  onFormCancel,
}: CreatePersonalAccessTokenFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateTokenFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const [createToken] = useCreatePersonalAccessTokenMutation()

  useEffect(() => {
    setIsOpen(showForm)
  }, [showForm])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const create = async (
    values: CreateTokenFormValues,
  ): Promise<string | null> => {
    try {
      const response = await createToken({
        name: values.name,
        expiresAt: values.expiresAt.toDate(),
      })

      if (response.error) {
        throw response.error
      }

      return response.data.token!
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while creating the PAT. Please try again.',
        )
      }
      return null
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      const token = await create(values)

      if (token) {
        setIsOpen(false)
        setIsSaving(false)
        form.resetFields()
        onFormCreate(token)
        messageApi.success('Personal access token created successfully')
      } else {
        setIsSaving(false)
      }
    } catch (errorInfo) {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }

  return (
    <Modal
      title="Create Personal Access Token"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Create"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden={true}
      width={600}
    >
      <Alert
        title="Important"
        description="After creation, the token will only be shown once. Make sure to copy it to a secure location."
        type="warning"
        showIcon
        icon={<ExclamationCircleOutlined />}
        style={{ marginBottom: 16 }}
      />
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="create-personal-access-token-form"
        initialValues={{ expiresAt: dayjs().add(1, 'year') }}
      >
        <Item
          label="Token Name"
          name="name"
          rules={[
            { required: true, message: 'Please enter a name for this token' },
            { max: 100, message: 'Name must be 100 characters or less' },
          ]}
        >
          <Input placeholder="e.g., CI/CD Pipeline, Personal Script" />
        </Item>

        <Item
          name="expiresAt"
          label="Expiration Date"
          help="Maximum: 2 years from now."
          rules={[
            {
              required: true,
              message: 'Please enter an expiration date for this token',
            },
          ]}
        >
          <DatePicker
            showTime
            disabledDate={(current) => {
              const minDate = dayjs()
              const maxDate = dayjs().add(2, 'year')
              return current && (current < minDate || current > maxDate)
            }}
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default CreatePersonalAccessTokenForm
