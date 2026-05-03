'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import {
  ProjectLifecyclePhaseDto,
  ProjectLifecyclePhaseRequest,
} from '@/src/services/wayd-api'
import { useUpdateProjectLifecyclePhaseMutation } from '@/src/store/features/ppm/project-lifecycles-api'
import { toFormErrors, isApiError } from '@/src/utils'
import { Form, Input, Modal } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useEffect } from 'react'
import { useModalForm } from '@/src/hooks'

const { Item } = Form

export interface EditProjectLifecyclePhaseFormProps {
  lifecycleId: string
  phase: ProjectLifecyclePhaseDto
  onFormComplete: () => void
  onFormCancel: () => void
}

interface UpdateProjectLifecyclePhaseFormValues {
  name: string
  description: string
}

const mapToRequestValues = (
  values: UpdateProjectLifecyclePhaseFormValues,
): ProjectLifecyclePhaseRequest => {
  return {
    name: values.name,
    description: values.description,
  } as ProjectLifecyclePhaseRequest
}

const EditProjectLifecyclePhaseForm = ({
  lifecycleId,
  phase,
  onFormComplete,
  onFormCancel,
}: EditProjectLifecyclePhaseFormProps) => {
  const messageApi = useMessage()

  const [updateProjectLifecyclePhase] =
    useUpdateProjectLifecyclePhaseMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<UpdateProjectLifecyclePhaseFormValues>({
      onSubmit: async (values: UpdateProjectLifecyclePhaseFormValues, form) => {
          try {
            const request = mapToRequestValues(values)
            const response = await updateProjectLifecyclePhase({
              lifecycleId,
              phaseId: phase.id,
              ...request,
            })
            if (response.error) {
              throw response.error
            }
            messageApi.success('Phase updated successfully.')
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
                  'An error occurred while updating the phase. Please try again.',
              )
            }
            return false
          }
        },
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while updating the phase. Please try again.',
      permission: 'Permissions.ProjectLifecycles.Update',
    })

  useEffect(() => {
    if (!phase) return
    form.setFieldsValue({
      name: phase.name,
      description: phase.description,
    })
  }, [phase, form])

  return (
    <Modal
      title="Edit Phase"
      open={isOpen}
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
        name="update-project-lifecycle-phase-form"
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

export default EditProjectLifecyclePhaseForm
