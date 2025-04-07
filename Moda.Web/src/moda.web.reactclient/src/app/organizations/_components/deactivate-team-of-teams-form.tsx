'use client'

import {
  DeactivateTeamRequest as DeactivateTeamOfTeamsRequest,
  TeamDetailsDto,
} from '@/src/services/moda-api'
import { useDeactivateTeamOfTeamsMutation } from '@/src/store/features/organizations/team-api'
import { useEffect, useState } from 'react'
import useAuth from '../../../components/contexts/auth'
import { DatePicker, Form, Modal } from 'antd'
import { toFormErrors } from '@/src/utils'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Form

interface DeactivateTeamOfTeamsFormProps {
  team: TeamDetailsDto
  showForm: boolean
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

const DeactivateTeamOfTeamsForm = (props: DeactivateTeamOfTeamsFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<DeactivateTeamFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [deactivateTeam, { error: mutationError }] =
    useDeactivateTeamOfTeamsMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateTeam = hasPermissionClaim('Permissions.Teams.Update')

  const deactivate = async (id: string, inactiveDate: Date) => {
    try {
      const request = mapToRequestValues(id, inactiveDate)
      const response = await deactivateTeam(request)
      if (response.error) {
        throw response.error
      }

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
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await deactivate(props.team.id, values.inactiveDate)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error(
        'An unexpected error occurred while deactivating the team.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  useEffect(() => {
    if (!props.team) return
    if (canUpdateTeam) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to deactive teams.')
    }
  }, [canUpdateTeam, messageApi, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Are you sure you want to deactivate this Team of Teams?"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Deactivate"
        okType="danger"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
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
    </>
  )
}

export default DeactivateTeamOfTeamsForm
