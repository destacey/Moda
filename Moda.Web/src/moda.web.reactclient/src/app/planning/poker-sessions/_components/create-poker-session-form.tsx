'use client'

import { Form, Input, Modal, Select } from 'antd'
import { useCallback } from 'react'
import { CreatePokerSessionRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import { useMessage } from '@/src/components/contexts/messaging'
import { useCreatePokerSessionMutation } from '@/src/store/features/planning/poker-sessions-api'
import { useGetEstimationScalesQuery } from '@/src/store/features/planning/estimation-scales-api'
import { useModalForm } from '@/src/hooks'

const { Item } = Form
const { TextArea } = Input

export interface CreatePokerSessionFormProps {
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreatePokerSessionFormValues {
  name: string
  estimationScaleId: number
}

const CreatePokerSessionForm = ({
  onFormCreate,
  onFormCancel,
}: CreatePokerSessionFormProps) => {
  const messageApi = useMessage()

  const [createPokerSession] = useCreatePokerSessionMutation()
  const { data: estimationScales, isLoading: scalesLoading } =
    useGetEstimationScalesQuery()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<CreatePokerSessionFormValues>({
      onSubmit: useCallback(
        async (values: CreatePokerSessionFormValues, form) => {
          try {
            const request: CreatePokerSessionRequest = {
              name: values.name,
              estimationScaleId: values.estimationScaleId,
            }
            const response = await createPokerSession(request)
            if (response.error) {
              throw response.error
            }
            messageApi.success('Successfully created poker session.')
            return true
          } catch (error) {
            if (error.status === 422 && error.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                'An error occurred while creating the poker session. Please try again.',
              )
              console.error(error)
            }
            return false
          }
        },
        [createPokerSession, messageApi],
      ),
      onComplete: onFormCreate,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while creating the poker session. Please try again.',
      permission: 'Permissions.PokerSessions.Create',
    })

  const scaleOptions = (estimationScales ?? [])
    .filter((s) => s.isActive)
    .map((s) => ({
      label: s.name,
      value: s.id,
    }))

  return (
    <Modal
      title="Create Poker Session"
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
        name="create-poker-session-form"
      >
        <Item label="Name" name="name" rules={[{ required: true }]}>
          <TextArea
            autoSize={{ minRows: 1, maxRows: 2 }}
            showCount
            maxLength={256}
          />
        </Item>
        <Item
          label="Estimation Scale"
          name="estimationScaleId"
          rules={[{ required: true }]}
        >
          <Select
            showSearch
            placeholder="Select an estimation scale"
            loading={scalesLoading}
            options={scaleOptions}
            optionFilterProp="label"
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default CreatePokerSessionForm
