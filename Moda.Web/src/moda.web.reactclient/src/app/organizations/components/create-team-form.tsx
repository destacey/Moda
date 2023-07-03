'use client'

import { Form, Input, Modal, Radio, message } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import {
  CreateTeamOfTeamsRequest,
  CreateTeamRequest,
} from '@/src/services/moda-api'

export interface CreateTeamFormProps {
  showForm: boolean
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateTeamFormValues {
  type: 'Team' | 'TeamOfTeams'
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
  const [form] = Form.useForm()

  const { hasClaim } = useAuth()
  const canCreateTeam = hasClaim('Permission', 'Permissions.Teams.Create')

  useEffect(() => {
    if (canCreateTeam) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      console.log('User does not have permission to create teams.')
      // TODO: show an antd error message
    }
  }, [canCreateTeam, onFormCancel, showForm])

  const onCreate = async (values: CreateTeamFormValues) => {
    console.log('Received values of form: ', values)
    if (values.type === 'Team') {
      await createTeam(values)
    } else if (values.type === 'TeamOfTeams') {
      await createTeamOfTeams(values)
    }
  }

  const createTeam = async (values: CreateTeamFormValues) => {
    const teamsClient = await getTeamsClient()
    const response = await teamsClient.create(values as CreateTeamRequest)
    console.log('createTeam response', response)
  }

  const createTeamOfTeams = async (values: CreateTeamFormValues) => {
    const teamsOfTeamsClient = await getTeamsOfTeamsClient()
    const response = await teamsOfTeamsClient.create(
      values as CreateTeamOfTeamsRequest
    )
    console.log('createTeamOfTeams response', response)
  }

  const handleOk = () => {
    setIsSaving(true)
    form
      .validateFields()
      .then(async (values) => {
        await onCreate(values)

        setIsOpen(false)
        setIsSaving(false)
        form.resetFields()

        onFormCreate()
      })
      .catch((info) => {
        console.log('Validate Failed:', info)
        // TODO: handle 422 status code and show validation errors
        message.error('Please correct the errors and try again.')
        setIsSaving(false)
      })
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }

  return (
    <Modal
      title="Create Team"
      open={isOpen}
      onOk={handleOk}
      okText="Create"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      closable={false}
      destroyOnClose={true}
    >
      <Form form={form} size="small" layout="vertical" name="create-team-form">
        <Form.Item label="Team Type" name="type" rules={[{ required: true }]}>
          <Radio.Group>
            <Radio value="Team">Team</Radio>
            <Radio value="TeamOfTeams">Team of Teams</Radio>
          </Radio.Group>
        </Form.Item>
        <Form.Item
          label="Name"
          name="name"
          rules={[{ required: true, message: 'The Name field is required.' }]}
        >
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
            autoSize={{ minRows: 6 }}
            showCount
            maxLength={1024}
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}

export default CreateTeamForm
