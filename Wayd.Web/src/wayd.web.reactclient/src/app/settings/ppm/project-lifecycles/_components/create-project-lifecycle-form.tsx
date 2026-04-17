'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { CreateProjectLifecycleRequest } from '@/src/services/moda-api'
import { useCreateProjectLifecycleMutation } from '@/src/store/features/ppm/project-lifecycles-api'
import { toFormErrors } from '@/src/utils'
import { Form, Modal } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useModalForm } from '@/src/hooks'

const { Item } = Form

export interface CreateProjectLifecycleFormProps {
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreateProjectLifecycleFormValues {
  name: string
  description: string
}

const mapToRequestValues = (
  values: CreateProjectLifecycleFormValues,
): CreateProjectLifecycleRequest => {
  return {
    name: values.name,
    description: values.description,
  } as CreateProjectLifecycleRequest
}

const CreateProjectLifecycleForm = ({
  onFormComplete,
  onFormCancel,
}: CreateProjectLifecycleFormProps) => {
  const messageApi = useMessage()

  const [createProjectLifecycle] = useCreateProjectLifecycleMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<CreateProjectLifecycleFormValues>({
      onSubmit: async (values: CreateProjectLifecycleFormValues, form) => {
          try {
            const request = mapToRequestValues(values)
            const response = await createProjectLifecycle(request)
            if (response.error) {
              throw response.error
            }
            messageApi.success(
              'Project Lifecycle created successfully. Key: ' + response.data,
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
                  'An error occurred while creating the project lifecycle. Please try again.',
              )
            }
            return false
          }
        },
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while creating the project lifecycle. Please try again.',
      permission: 'Permissions.ProjectLifecycles.Create',
    })

  return (
    <Modal
      title="Create Project Lifecycle"
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
        name="create-project-lifecycle-form"
      >
        <Item
          label="Name"
          name="name"
          rules={[
            { required: true, message: 'Name is required' },
            { max: 128 },
          ]}
        >
          <TextArea
            autoSize={{ minRows: 1, maxRows: 2 }}
            showCount
            maxLength={128}
          />
        </Item>
        <Item
          name="description"
          label="Description"
          rules={[
            { required: true, message: 'Description is required' },
            { max: 1024 },
          ]}
        >
          <TextArea
            autoSize={{ minRows: 6, maxRows: 8 }}
            showCount
            maxLength={1024}
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default CreateProjectLifecycleForm
