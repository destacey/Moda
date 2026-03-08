'use client'

import { Form, Input, Modal, Switch } from 'antd'
import { useCallback } from 'react'
import { toFormErrors } from '@/src/utils'
import { useCreateFeatureFlagMutation } from '@/src/store/features/admin/feature-flags-api'
import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'

const { Item } = Form
const { TextArea } = Input

interface CreateFeatureFlagFormProps {
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateFeatureFlagFormValues {
  name: string
  displayName: string
  description?: string
  isEnabled: boolean
}

const CreateFeatureFlagForm = ({
  onFormCreate,
  onFormCancel,
}: CreateFeatureFlagFormProps) => {
  const messageApi = useMessage()
  const [createFeatureFlag] = useCreateFeatureFlagMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<CreateFeatureFlagFormValues>({
      onSubmit: useCallback(
        async (values: CreateFeatureFlagFormValues, form) => {
          try {
            const response = await createFeatureFlag({
              name: values.name,
              displayName: values.displayName,
              description: values.description || null,
              isEnabled: values.isEnabled ?? false,
            })

            if (response.error) {
              throw response.error
            }

            messageApi.success('Successfully created feature flag.')
            onFormCreate()
            return false
          } catch (error: any) {
            if (error.status === 422 && error.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                error?.detail ||
                  'An unexpected error occurred while creating the feature flag.',
              )
            }
            return false
          }
        },
        [createFeatureFlag, messageApi, onFormCreate],
      ),
      onComplete: () => {},
      onCancel: onFormCancel,
      errorMessage:
        'An unexpected error occurred while creating the feature flag.',
      permission: 'Permissions.FeatureFlags.Create',
    })

  return (
    <Modal
      title="Create Feature Flag"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Create"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="create-feature-flag-form"
        initialValues={{ isEnabled: false }}
      >
        <Item
          label="Name"
          name="name"
          rules={[
            { required: true },
            {
              pattern: /^[a-z0-9]+(-[a-z0-9]+)*$/,
              message: 'Name must be kebab-case (e.g., my-feature-name)',
            },
          ]}
          extra="Kebab-case identifier used in code (e.g., new-dashboard-layout)"
        >
          <Input showCount maxLength={128} />
        </Item>

        <Item
          label="Display Name"
          name="displayName"
          rules={[{ required: true }]}
        >
          <Input showCount maxLength={128} />
        </Item>

        <Item name="description" label="Description">
          <TextArea
            autoSize={{ minRows: 3, maxRows: 6 }}
            showCount
            maxLength={1024}
          />
        </Item>

        <Item name="isEnabled" label="Enabled" valuePropName="checked">
          <Switch />
        </Item>
      </Form>
    </Modal>
  )
}

export default CreateFeatureFlagForm
