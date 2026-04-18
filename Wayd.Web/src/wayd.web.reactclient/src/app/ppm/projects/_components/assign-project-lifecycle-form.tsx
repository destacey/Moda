'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'
import {
  AssignProjectLifecycleRequest,
  ProjectDetailsDto,
  ProjectLifecycleState,
} from '@/src/services/wayd-api'
import {
  useGetProjectLifecycleQuery,
  useGetProjectLifecyclesQuery,
} from '@/src/store/features/ppm/project-lifecycles-api'
import { useAssignProjectLifecycleMutation } from '@/src/store/features/ppm/projects-api'
import { toFormErrors } from '@/src/utils'
import { Card, Flex, Form, Modal, Select, Timeline, Typography } from 'antd'
import { useEffect } from 'react'

const { Item } = Form
const { Text } = Typography

export interface AssignProjectLifecycleFormProps {
  project: ProjectDetailsDto
  onFormComplete: () => void
  onFormCancel: () => void
}

interface AssignProjectLifecycleFormValues {
  lifecycleId: string
}

const AssignProjectLifecycleForm = ({
  project,
  onFormComplete,
  onFormCancel,
}: AssignProjectLifecycleFormProps) => {
  const messageApi = useMessage()

  const [assignProjectLifecycle] = useAssignProjectLifecycleMutation()

  const {
    data: lifecycleData,
    isLoading: lifecyclesLoading,
    error: lifecyclesError,
  } = useGetProjectLifecyclesQuery(ProjectLifecycleState.Active)

  const lifecycleOptions = !lifecycleData ? [] : [...lifecycleData]
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((lc) => ({
        label: lc.name,
        value: lc.id,
      }))

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<AssignProjectLifecycleFormValues>({
      onSubmit: async (values: AssignProjectLifecycleFormValues, form) => {
          try {
            const request: AssignProjectLifecycleRequest = {
              lifecycleId: values.lifecycleId,
            }
            const response = await assignProjectLifecycle({
              id: project.id,
              request,
              cacheKey: project.key,
            })
            if (response.error) throw response.error

            messageApi.success('Project lifecycle assigned successfully.')
            return true
          } catch (error) {
            if (error.status === 422 && error.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                error.detail ??
                  'An error occurred while assigning the lifecycle. Please try again.',
              )
            }
            return false
          }
        },
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while assigning the lifecycle. Please try again.',
      permission: 'Permissions.Projects.Update',
    })

  const selectedLifecycleId = Form.useWatch('lifecycleId', form)

  const { data: selectedLifecycle } = useGetProjectLifecycleQuery(
    selectedLifecycleId,
    { skip: !selectedLifecycleId },
  )

  const phaseItems = !selectedLifecycle?.phases ? [] : [...selectedLifecycle.phases]
      .sort((a, b) => a.order - b.order)
      .map((phase) => ({
        content: (
          <>
            <Text strong>{phase.name}</Text>
            <br />
            <Text type="secondary">{phase.description}</Text>
          </>
        ),
      }))

  useEffect(() => {
    if (lifecyclesError) {
      console.error(lifecyclesError)
      messageApi.error(
        lifecyclesError.detail ??
          'An error occurred while loading lifecycle options.',
      )
    }
  }, [lifecyclesError, messageApi])

  return (
    <Modal
      title="Assign Project Lifecycle"
      open={isOpen}
      width={'40vw'}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Assign"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      <Flex vertical gap="small">
        <Text type="secondary">
          Select a lifecycle to assign to this project. This will create the
          project phases from the lifecycle template.
        </Text>
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="assign-project-lifecycle-form"
        >
          <Item
            name="lifecycleId"
            label="Lifecycle"
            rules={[{ required: true, message: 'Lifecycle is required' }]}
          >
            <Select
              options={lifecycleOptions}
              placeholder="Select Lifecycle"
              loading={lifecyclesLoading}
            />
          </Item>
        </Form>
        {phaseItems.length > 0 && (
          <Card size="small" title="Phases">
            <Timeline items={phaseItems} />
          </Card>
        )}
      </Flex>
    </Modal>
  )
}

export default AssignProjectLifecycleForm
