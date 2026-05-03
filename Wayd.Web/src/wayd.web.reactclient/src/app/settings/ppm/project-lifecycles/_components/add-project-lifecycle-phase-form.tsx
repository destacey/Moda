'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { ProjectLifecyclePhaseRequest } from '@/src/services/wayd-api'
import { useAddProjectLifecyclePhaseMutation } from '@/src/store/features/ppm/project-lifecycles-api'
import { toFormErrors, isApiError } from '@/src/utils'
import { Form, Input, Modal } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useModalForm } from '@/src/hooks'

const { Item } = Form

export interface AddProjectLifecyclePhaseFormProps {
  lifecycleId: string
  onFormComplete: () => void
  onFormCancel: () => void
}

interface AddProjectLifecyclePhaseFormValues {
  name: string
  description: string
}

const mapToRequestValues = (
  values: AddProjectLifecyclePhaseFormValues,
): ProjectLifecyclePhaseRequest => {
  return {
    name: values.name,
    description: values.description,
  } as ProjectLifecyclePhaseRequest
}

const AddProjectLifecyclePhaseForm = ({
  lifecycleId,
  onFormComplete,
  onFormCancel,
}: AddProjectLifecyclePhaseFormProps) => {
  const messageApi = useMessage()

  const [addProjectLifecyclePhase] = useAddProjectLifecyclePhaseMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<AddProjectLifecyclePhaseFormValues>({
      onSubmit: async (values: AddProjectLifecyclePhaseFormValues, form) => {
          try {
            const request = mapToRequestValues(values)
            const response = await addProjectLifecyclePhase({
              lifecycleId,
              ...request,
            })
            if (response.error) {
              throw response.error
            }
            messageApi.success('Phase added successfully.')
            return true
          } catch (error) {
            const apiError = isApiError(error) ? error : {}
            if (apiError.status === 422 && apiError.errors) {
              const formErrors = toFormErrors(apiError.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                apiError.detail ??
                  'An error occurred while adding the phase. Please try again.',
              )
            }
            return false
          }
        },
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while adding the phase. Please try again.',
      permission: 'Permissions.ProjectLifecycles.Update',
    })

  return (
    <Modal
      title="Add Phase"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Add"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="add-project-lifecycle-phase-form"
      >
        <Item
          label="Name"
          name="name"
          rules={[
            { required: true, message: 'Name is required' },
            { max: 32 },
          ]}
        >
          <Input showCount maxLength={32} />
        </Item>
        <Item
          name="description"
          label="Description"
          rules={[{ max: 1024 }]}
        >
          <TextArea
            autoSize={{ minRows: 4, maxRows: 6 }}
            showCount
            maxLength={1024}
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default AddProjectLifecyclePhaseForm
