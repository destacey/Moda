'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { useMessage } from '@/src/components/contexts/messaging'
import { UpdateStrategicThemeRequest } from '@/src/services/moda-api'
import {
  useGetStrategicThemeQuery,
  useUpdateStrategicThemeMutation,
} from '@/src/store/features/strategic-management/strategic-themes-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal } from 'antd'
import { useCallback, useEffect } from 'react'
import { useModalForm } from '@/src/hooks'

const { Item } = Form

export interface EditStrategicThemeFormProps {
  strategicThemeKey: number
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

const EditStrategicThemeForm = ({
  strategicThemeKey,
  onFormComplete,
  onFormCancel,
}: EditStrategicThemeFormProps) => {
  const messageApi = useMessage()

  const { data: strategicThemeData, error } =
    useGetStrategicThemeQuery(strategicThemeKey)

  const [updateStrategicTheme] = useUpdateStrategicThemeMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<UpdateStrategicThemeFormValues>({
      onSubmit: useCallback(
        async (values: UpdateStrategicThemeFormValues, form) => {
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
        },
        [updateStrategicTheme, strategicThemeData, messageApi],
      ),
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while updating the Strategic Theme. Please try again.',
      permission: 'Permissions.StrategicThemes.Update',
    })

  useEffect(() => {
    if (!strategicThemeData) return
    form.setFieldsValue({
      name: strategicThemeData.name,
      description: strategicThemeData.description,
    })
  }, [strategicThemeData, form])

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading the Strategic Theme. Please try again.',
      )
    }
  }, [error, messageApi])

  return (
    <Modal
      title="Edit Strategic Theme"
      open={isOpen}
      width={'60vw'}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
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
          rules={[{ required: true, message: 'Name is required' }, { max: 64 }]}
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
            onChange={(value) => form.setFieldValue('description', value || '')}
            maxLength={1024}
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default EditStrategicThemeForm
