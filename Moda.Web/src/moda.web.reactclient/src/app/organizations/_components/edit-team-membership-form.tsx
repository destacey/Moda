'use client'

import { DatePicker, Descriptions, Form, Modal } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../components/contexts/auth'
import {
  TeamMembershipDto,
  UpdateTeamMembershipRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import { useUpdateTeamMembershipMutation } from '@/src/store/features/organizations/team-api'
import { TeamTypeName } from '../types'
import dayjs from 'dayjs'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Descriptions
const { Item: FormItem } = Form

export interface UpdateTeamMembershipFormProps {
  showForm: boolean
  membership: TeamMembershipDto
  teamType: TeamTypeName
  onFormSave: () => void
  onFormCancel: () => void
}

interface UpdateTeamMembershipFormValues {
  start: Date
  end: Date | null
}

const mapToRequestValues = (
  values: UpdateTeamMembershipFormValues,
  originalMembership: TeamMembershipDto,
  teamType: TeamTypeName,
) => {
  const membership = {
    teamMembershipId: originalMembership.id,
    teamId: originalMembership.child.id,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
  } as UpdateTeamMembershipRequest
  return {
    membership,
    parentTeamId: originalMembership.parent.id,
    teamType,
  }
}

const EditTeamMembershipForm = (props: UpdateTeamMembershipFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<UpdateTeamMembershipFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const [updateTeamMembership] = useUpdateTeamMembershipMutation()

  const { hasClaim } = useAuth()
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Teams.ManageTeamMemberships',
  )

  const mapToFormValues = useCallback(
    (membership: TeamMembershipDto) => {
      if (!membership) {
        throw new Error('Membership is required.')
      }
      form.setFieldsValue({
        start: membership.start ? dayjs(membership.start) : null,
        end: membership.end ? dayjs(membership.end) : null,
      })
    },
    [form],
  )

  const update = async (
    values: UpdateTeamMembershipFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(
        values,
        props.membership,
        props.teamType,
      )
      await updateTeamMembership(request).unwrap()
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An unexpected error occurred while updating the team membership.',
        )
        console.error(error)
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormSave()

        messageApi.success('Successfully updated team membership.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    props.onFormCancel()
    form.resetFields()
  }, [form, props])

  useEffect(() => {
    if (!props.membership) return
    if (canManageTeamMemberships) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(props.membership)
      }
    } else {
      handleCancel()
      messageApi.error('You do not have permission to manage team memberships.')
    }
  }, [
    canManageTeamMemberships,
    handleCancel,
    mapToFormValues,
    messageApi,
    props.membership,
    props.showForm,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <Modal
      title="Edit Team Membership"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="edit-team-membership-form"
      >
        <Descriptions size="small" column={1}>
          <Item label="Team">{props.membership?.child.name}</Item>
          <Item label="Parent Team">{props.membership?.parent.name}</Item>
        </Descriptions>
        <FormItem label="Start" name="start" rules={[{ required: true }]}>
          <DatePicker />
        </FormItem>
        <FormItem
          label="End"
          name="end"
          dependencies={['start']}
          rules={[
            ({ getFieldValue }) => ({
              validator(_, value) {
                const start = getFieldValue('start')
                if (!value || !start || start < value) {
                  return Promise.resolve()
                }
                return Promise.reject(
                  new Error('End date must be after start date'),
                )
              },
            }),
          ]}
        >
          <DatePicker />
        </FormItem>
      </Form>
    </Modal>
  )
}

export default EditTeamMembershipForm
