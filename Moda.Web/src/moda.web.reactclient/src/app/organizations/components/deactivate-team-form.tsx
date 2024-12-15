'use client'

import { DeactivateTeamRequest, TeamDetailsDto } from '@/src/services/moda-api'
import { useDeactivateTeamMutation } from '@/src/store/features/organizations/team-api'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import { DatePicker, Form, Modal } from 'antd'
import { toFormErrors } from '@/src/utils'

const { Item } = Form

interface DeactivateTeamFormProps {
  team: TeamDetailsDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface DeactivateTeamFormValues {
  inactiveDate: Date
}

const mapToRequestValues = (
  id: string,
  inactiveDate: Date,
): DeactivateTeamRequest => {
  return {
    id,
    inactiveDate: (inactiveDate as any)?.format('YYYY-MM-DD'),
  } as DeactivateTeamRequest
}

const DeactivateTeamForm = (props: DeactivateTeamFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<DeactivateTeamFormValues>()
  const formValues = Form.useWatch([], form)

  const [deactivateTeam, { error: mutationError }] = useDeactivateTeamMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateTeam = hasPermissionClaim('Permissions.Teams.Update')

  const deactivate = async (id: string, inactiveDate: Date) => {
    try {
      const request = mapToRequestValues(id, inactiveDate)
      const response = await deactivateTeam(request)
      if (response.error) {
        throw response.error
      }

      props.messageApi.success('Team deactivated successfully')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
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
      props.messageApi.error(
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
      props.messageApi.error('You do not have permission to deactive teams.')
    }
  }, [canUpdateTeam, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Are you sure you want to deactivate this Team?"
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

export default DeactivateTeamForm
