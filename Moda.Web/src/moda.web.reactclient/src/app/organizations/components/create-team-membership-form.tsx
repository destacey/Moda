'use client'

import { DatePicker, Form, Modal, Select, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import { AddTeamMembershipRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import {
  CreateTeamMembershipMutationRequest,
  useCreateTeamMembershipMutation,
  useGetTeamOfTeamsOptions,
} from '@/src/services/queries/organization-queries'
import { TeamTypeName } from '../types'

export interface CreateTeamMembershipFormProps {
  showForm: boolean
  teamId: string
  teamType: TeamTypeName
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateTeamMembershipFormValues {
  parentTeamId: string
  start: Date
  end: Date | null
}

const mapToRequestValues = (
  values: CreateTeamMembershipFormValues,
  teamType: TeamTypeName,
) => {
  const membership = {
    parentTeamId: values.parentTeamId,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
  } as AddTeamMembershipRequest
  return { membership, teamType } as CreateTeamMembershipMutationRequest
}

const CreateTeamMembershipForm = (props: CreateTeamMembershipFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateTeamMembershipFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  // TODO: only get teams that are not in the hierarchy
  const { data: teamOptions } = useGetTeamOfTeamsOptions(true)

  const createTeamMembership = useCreateTeamMembershipMutation()

  const { hasClaim } = useAuth()
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Teams.ManageTeamMemberships',
  )

  const create = async (
    values: CreateTeamMembershipFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values, props.teamType)
      request.membership.teamId = props.teamId
      await createTeamMembership.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.supportMessage ??
            'An unexpected error occurred while creating the team membership.',
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
      if (await create(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormCreate()
        // TODO: this message is not displaying
        messageApi.success('Successfully created team membership.')
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
    if (!teamOptions) return

    if (canManageTeamMemberships) {
      setIsOpen(props.showForm)
    } else {
      handleCancel()
      messageApi.error('You do not have permission to manage Team Memberships.')
    }
  }, [
    canManageTeamMemberships,
    handleCancel,
    messageApi,
    props.showForm,
    teamOptions,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      {contextHolder}
      <Modal
        title="Create Team Membership"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
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
          name="create-team-membership-form"
        >
          <Form.Item
            name="parentTeamId"
            label="Parent Team"
            rules={[{ required: true }]}
          >
            <Select
              showSearch
              placeholder="Select a parent team"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.label.toLowerCase() ?? '').includes(
                  input.toLowerCase(),
                )
              }
              filterSort={(optionA, optionB) =>
                (optionA?.label ?? '')
                  .toLowerCase()
                  .localeCompare((optionB?.label ?? '').toLowerCase())
              }
              options={teamOptions?.filter((t) => t.value !== props.teamId)}
            />
          </Form.Item>
          <Form.Item label="Start" name="start" rules={[{ required: true }]}>
            <DatePicker />
          </Form.Item>
          <Form.Item label="End" name="end">
            <DatePicker />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateTeamMembershipForm
