'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { CreateStrategicThemeRequest } from '@/src/services/moda-api'
import { useCreateStrategicThemeMutation } from '@/src/store/features/strategic-management/strategic-themes-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface CreateStrategicThemeFormProps {
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreateStrategicThemeFormValues {
  name: string
  description: string
}

const mapToRequestValues = (
  values: CreateStrategicThemeFormValues,
): CreateStrategicThemeRequest => {
  return {
    name: values.name,
    description: values.description,
  } as CreateStrategicThemeRequest
}

const CreateStrategicThemeForm = (props: CreateStrategicThemeFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateStrategicThemeFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [createStrategicTheme, { error: mutationError }] =
    useCreateStrategicThemeMutation()

  const { hasPermissionClaim } = useAuth()
  const canCreateStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Create',
  )

  const create = async (values: CreateStrategicThemeFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await createStrategicTheme(request)
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        'Strategic Theme created successfully. Strategic Theme key: ' +
          response.data.key,
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
            'An error occurred while creating the strategic theme. Please try again.',
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
        'An error occurred while creating the strategic theme. Please try again.',
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
    if (canCreateStrategicTheme) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to create strategic themes.')
    }
  }, [canCreateStrategicTheme, messageApi, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Create Strategic Theme"
        open={isOpen}
        width={'60vw'}
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
          name="create-strategic-theme-form"
        >
          <Item
            label="Name"
            name="name"
            rules={[
              { required: true, message: 'Name is required' },
              { max: 64 },
            ]}
          >
            <Input maxLength={64} showCount />
          </Item>
          <Item
            name="description"
            label="Description"
            rules={[
              { required: true, message: 'Description is required' },
              { max: 1024 },
            ]}
          >
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={1024}
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateStrategicThemeForm
