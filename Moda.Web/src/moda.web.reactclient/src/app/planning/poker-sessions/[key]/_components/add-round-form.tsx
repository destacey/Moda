'use client'

import { Form, Input, Modal } from 'antd'
import { useCallback } from 'react'
import { toFormErrors } from '@/src/utils'
import { useMessage } from '@/src/components/contexts/messaging'
import { useAddPokerRoundMutation } from '@/src/store/features/planning/poker-sessions-api'
import { useModalForm } from '@/src/hooks'

const { Item } = Form
const { TextArea } = Input

export interface AddRoundFormProps {
  sessionId: string
  sessionKey: number
  onFormCreate: () => void
  onFormCancel: () => void
}

interface AddRoundFormValues {
  label: string
}

const AddRoundForm = ({
  sessionId,
  sessionKey,
  onFormCreate,
  onFormCancel,
}: AddRoundFormProps) => {
  const messageApi = useMessage()

  const [addRound] = useAddPokerRoundMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<AddRoundFormValues>({
      onSubmit: useCallback(
        async (values: AddRoundFormValues, form) => {
          try {
            const response = await addRound({
              sessionId,
              sessionKey,
              request: { label: values.label },
            })
            if (response.error) {
              throw response.error
            }
            messageApi.success('Round added.')
            return true
          } catch (error) {
            if (error.status === 422 && error.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                'An error occurred while adding the round. Please try again.',
              )
              console.error(error)
            }
            return false
          }
        },
        [addRound, sessionId, sessionKey, messageApi],
      ),
      onComplete: onFormCreate,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while adding the round. Please try again.',
      permission: 'Permissions.PokerSessions.Update',
    })

  return (
    <Modal
      title="Add Round"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Add"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      <Form form={form} size="small" layout="vertical" name="add-round-form">
        <Item label="Label" name="label" rules={[{ required: true }]}>
          <TextArea
            autoSize={{ minRows: 1, maxRows: 2 }}
            showCount
            maxLength={512}
            placeholder="e.g., WI-123: Implement user login"
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default AddRoundForm
