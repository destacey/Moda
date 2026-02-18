'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  StrategicThemeDetailsDto,
  UpdateStrategicThemeRequest,
} from '@/src/services/moda-api'
import {
  useGetStrategicThemeQuery,
  useUpdateStrategicThemeMutation,
} from '@/src/store/features/strategic-management/strategic-themes-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface EditStrategicThemeFormProps {
  strategicThemeKey: number
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface UpdateStrategicThemeFormValues {
  name: string
  description: string
}

const mapToRequestValues = (
  values: UpdateStrategicThemeFormValues,
  strategicThemeId: string,
): UpdateStrategicThemeRequest => {
  return {
    id: strategicThemeId,
    name: values.name,
    description: values.description,
  } as UpdateStrategicThemeRequest
}

const EditStrategicThemeForm = (props: EditStrategicThemeFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<UpdateStrategicThemeFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const {
    data: strategicThemeData,
    isLoading,
    error,
  } = useGetStrategicThemeQuery(props.strategicThemeKey)

  const [updateStrategicTheme, { error: mutationError }] =
    useUpdateStrategicThemeMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Update',
  )

  const mapToFormValues = useCallback(
    (strategicTheme: StrategicThemeDetailsDto) => {
      if (!strategicTheme) {
        throw new Error('Strategic Theme not found')
      }
      form.setFieldsValue({
        name: strategicTheme.name,
        description: strategicTheme.description,
      })
    },
    [form],
  )

  const update = async (values: UpdateStrategicThemeFormValues) => {
    try {
      const request = mapToRequestValues(values, strategicThemeData.id)
      const response = await updateStrategicTheme({
        request,
        cacheKey: strategicThemeData.key,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success('Strategic Theme updated successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the Strategic Theme. Please try again.',
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
      console.error('update error', error)
      messageApi.error(
        'An error occurred while updating the Strategic Theme. Please try again.',
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
    if (!strategicThemeData) return

    if (canUpdateStrategicTheme) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(strategicThemeData)
      }
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to update Strategic Themes.')
    }
  }, [
    canUpdateStrategicTheme,
    mapToFormValues,
    messageApi,
    props,
    strategicThemeData,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading the Strategic Theme. Please try again.',
      )
    }
  }, [error, messageApi])

  return (
    <>
      <Modal
        title="Edit Strategic Theme"
        open={isOpen}
        width={'60vw'}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="update-strategic-theme-form"
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

export default EditStrategicThemeForm
