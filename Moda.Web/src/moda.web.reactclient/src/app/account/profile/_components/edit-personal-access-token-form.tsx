'use client'

import { Form, Input, Modal, DatePicker } from 'antd'
import { useEffect, useState } from 'react'
import { useMessage } from '@/src/components/contexts/messaging'
import dayjs, { Dayjs } from 'dayjs'
import { useUpdatePersonalAccessTokenMutation } from '@/src/store/features/user-management/personal-access-tokens-api'
import { PersonalAccessTokenDto } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'

const { Item } = Form

export interface EditPersonalAccessTokenFormProps {
  token: PersonalAccessTokenDto
  showForm: boolean
  onFormUpdate: () => void
  onFormCancel: () => void
}

interface EditTokenFormValues {
  name: string
  expiresAt: Dayjs
}

const EditPersonalAccessTokenForm = ({
  token,
  showForm,
  onFormUpdate,
  onFormCancel,
}: EditPersonalAccessTokenFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditTokenFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const [updateToken] = useUpdatePersonalAccessTokenMutation()

  useEffect(() => {
    setIsOpen(showForm)
    if (showForm && token) {
      form.setFieldsValue({
        name: token.name!,
        expiresAt: dayjs(token.expiresAt),
      })
    }
  }, [showForm, token, form])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const update = async (values: EditTokenFormValues): Promise<boolean> => {
    try {
      const response = await updateToken({
        id: token.id!,
        request: {
          name: values.name,
          expiresAt: values.expiresAt.toDate(),
        },
      })

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the PAT. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      const success = await update(values)

      if (success) {
        setIsOpen(false)
        setIsSaving(false)
        form.resetFields()
        onFormUpdate()
        messageApi.success('Personal access token updated successfully')
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
      title="Edit Personal Access Token"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden={true}
      width={600}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="edit-personal-access-token-form"
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

export default EditPersonalAccessTokenForm
