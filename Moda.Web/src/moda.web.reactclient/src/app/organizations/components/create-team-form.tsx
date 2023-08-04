'use client'

import { Form, Input, Modal, Radio, message } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import {
  CreateTeamOfTeamsRequest,
  CreateTeamRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'

export interface CreateTeamFormProps {
  showForm: boolean
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateTeamFormValues {
  type: 'Team' | 'Team of Teams'
  name: string
  code: string
  description: string
}

const CreateTeamForm = ({
  showForm,
  onFormCreate,
  onFormCancel,
}: CreateTeamFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateTeamFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canCreateTeam = hasClaim('Permission', 'Permissions.Teams.Create')

  useEffect(() => {
    if (canCreateTeam) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to create teams.')
    }
  }, [canCreateTeam, onFormCancel, showForm, messageApi])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false)
    )
  }, [form, formValues])

  const create = async (values: CreateTeamFormValues): Promise<boolean> => {
    try {
      if (values.type === 'Team') {
        await createTeam(values)
      } else if (values.type === 'Team of Teams') {
        await createTeamOfTeams(values)
      } else {
        throw new Error(`Invalid team type: ${values.type}`)
      }
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          `An unexpected error occurred while creating the ${values.type}.`
        )
        console.error(error)
      }

      return false
    }
  }

  const createTeam = async (values: CreateTeamFormValues) => {
    const teamsClient = await getTeamsClient()
    const localId = await teamsClient.create(values as CreateTeamRequest)
  }

  const createTeamOfTeams = async (values: CreateTeamFormValues) => {
    const teamsOfTeamsClient = await getTeamsOfTeamsClient()
    const localId = await teamsOfTeamsClient.create(
      values as CreateTeamOfTeamsRequest
    )
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await create(values)) {
        setIsOpen(false)
        setIsSaving(false)
        form.resetFields()
        onFormCreate()
        messageApi.success(`Successfully created ${values.type}.`)
      } else {
        setIsSaving(false)
      }
    } catch (errorInfo) {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }

  return (
    <>
      {contextHolder}
      <Modal
        title="Create Team"
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
          name="create-team-form"
        >
          <Form.Item label="Team Type" name="type" rules={[{ required: true }]}>
            <Radio.Group>
              <Radio value="Team">Team</Radio>
              <Radio value="Team of Teams">Team of Teams</Radio>
            </Radio.Group>
          </Form.Item>
          <Form.Item label="Name" name="name" rules={[{ required: true }]}>
            <Input showCount maxLength={128} />
          </Form.Item>
          <Form.Item
            label="Code"
            name="code"
            rules={[
              { required: true, message: 'The Code field is required.' },
              {
                min: 2,
                max: 10,
                message: 'The Code field must be between 2-10 characters.',
              },
              {
                pattern: /^[A-Z0-9]+$/,
                message:
                  'The Code field is invalid. Uppercase letters and numbers only.',
              },
            ]}
          >
            <Input
              showCount
              maxLength={10}
              onInput={(e) =>
                ((e.target as HTMLInputElement).value = (
                  e.target as HTMLInputElement
                ).value.toUpperCase())
              }
            />
          </Form.Item>
          <Form.Item
            name="description"
            label="Description"
            help="Markdown enabled"
          >
            <Input.TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateTeamForm
