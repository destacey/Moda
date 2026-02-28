'use client'

import {
  DeactivateTeamRequest as DeactivateTeamOfTeamsRequest,
  TeamDetailsDto,
} from '@/src/services/moda-api'
import { useDeactivateTeamOfTeamsMutation } from '@/src/store/features/organizations/team-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Modal } from 'antd'
import { useCallback } from 'react'
import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'

const { Item } = Form

interface DeactivateTeamOfTeamsFormProps {
  team: TeamDetailsDto
  onFormComplete: () => void
  onFormCancel: () => void
}

interface DeactivateTeamFormValues {
  inactiveDate: Date
}

const mapToRequestValues = (
  id: string,
  inactiveDate: Date,
): DeactivateTeamOfTeamsRequest => {
  return {
    id,
    inactiveDate: (inactiveDate as any)?.format('YYYY-MM-DD'),
  } as DeactivateTeamOfTeamsRequest
}

const DeactivateTeamOfTeamsForm = ({
  team,
  onFormComplete,
  onFormCancel,
}: DeactivateTeamOfTeamsFormProps) => {
  const messageApi = useMessage()

  const [deactivateTeam] = useDeactivateTeamOfTeamsMutation()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<DeactivateTeamFormValues>({
      onSubmit: useCallback(
        async (values: DeactivateTeamFormValues, form) => {
          try {
            const request = mapToRequestValues(team.id, values.inactiveDate)
            const response = await deactivateTeam(request)
            if (response.error) throw response.error

            messageApi.success('Team deactivated successfully')
            return true
          } catch (error) {
            if (error.status === 422 && error.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                error.detail ??
                  'An error occurred while deactivating the team. Please try again.',
              )
            }
            return false
          }
        },
        [deactivateTeam, team.id, messageApi],
      ),
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An unexpected error occurred while deactivating the team.',
      permission: 'Permissions.Teams.Update',
    })

  return (
    <Modal
      title="Are you sure you want to deactivate this Team of Teams?"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Deactivate"
      okType="danger"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="deactivate-team-form"
      >
        <Item
          name="inactiveDate"
          label="Inactive Date"
          rules={[
            { required: true, message: 'Please select an inactive date' },
          ]}
        >
          <DatePicker />
        </Item>
      </Form>
    </Modal>
  )
}

export default DeactivateTeamOfTeamsForm
