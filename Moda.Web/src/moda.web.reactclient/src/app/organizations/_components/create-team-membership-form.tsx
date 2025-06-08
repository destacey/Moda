'use client'

import { DatePicker, Form, Modal, Select } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../components/contexts/auth'
import { AddTeamMembershipRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import {
  useCreateTeamMembershipMutation,
  useGetTeamOfTeamsOptionsQuery,
} from '@/src/store/features/organizations/team-api'
import { TeamTypeName } from '../types'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item: FormItem } = Form

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
  teamId: string,
) => {
  const membership = {
    teamId: teamId,
    parentTeamId: values.parentTeamId,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
  } as AddTeamMembershipRequest
  return { membership, teamType }
}

const CreateTeamMembershipForm = (props: CreateTeamMembershipFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateTeamMembershipFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()
  // TODO: only get teams that are not in the hierarchy
  const { data: teamOptions } = useGetTeamOfTeamsOptionsQuery(false)

  const [createTeamMembership] = useCreateTeamMembershipMutation()

  const { hasClaim } = useAuth()
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Teams.ManageTeamMemberships',
  )

  const create = async (
    values: CreateTeamMembershipFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values, props.teamType, props.teamId)
      await createTeamMembership(request).unwrap()
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
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
      destroyOnHidden={true}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="create-team-membership-form"
      >
        <FormItem
          name="parentTeamId"
          label="Parent Team"
          rules={[{ required: true }]}
        >
          <Select
            showSearch
            placeholder="Select a parent team"
            optionFilterProp="children"
            filterOption={(input, option) =>
              (option?.label.toLowerCase() ?? '').includes(input.toLowerCase())
            }
            filterSort={(optionA, optionB) =>
              (optionA?.label ?? '')
                .toLowerCase()
                .localeCompare((optionB?.label ?? '').toLowerCase())
            }
            options={teamOptions?.filter((t) => t.value !== props.teamId)}
          />
        </FormItem>
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

export default CreateTeamMembershipForm
