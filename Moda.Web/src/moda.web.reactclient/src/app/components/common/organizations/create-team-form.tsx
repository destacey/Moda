'use client'

import { Form, Input, Modal, Radio } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'

export interface CreateTeamFormProps {
  showForm: boolean
  onFormCreate: () => void
  onFormCancel: () => void
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

  const onCreate = (values) => {
    console.log('Received values of form: ', values)
  }

  const handleOk = () => {
    setIsSaving(true)
    form
      .validateFields()
      .then((values) => {
        form.resetFields()
        onCreate(values)

        setTimeout(() => {
          setIsOpen(false)
          onFormCreate()
          setIsSaving(false)
        }, 2000)
      })
      .catch((info) => {
        console.log('Validate Failed:', info)
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
            <Radio value="team">Team</Radio>
            <Radio value="teamOfTeams">Team of Teams</Radio>
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
        <Form.Item name="description" label="Description">
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
