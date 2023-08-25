'use client'

import { Form, Input, Modal, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import {
  UpdateTeamOfTeamsRequest,
  UpdateTeamRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'

export interface EditTeamFormProps {
  showForm: boolean
  key: number
  type: string
  onFormUpdate: () => void
  onFormCancel: () => void
}

interface EditTeamFormValues {
  id: string
  name: string
  code: string
  description: string
}

const EditTeamForm = ({
  showForm,
  key,
  type,
  onFormUpdate,
  onFormCancel,
}: EditTeamFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditTeamFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')

  const mapTeamToFormValues = useCallback(
    (team: EditTeamFormValues) => {
      form.setFieldsValue({
        id: team.id,
        name: team.name,
        code: team.code,
        description: team.description,
      })
    },
    [form]
  )

  const getTeamData = useCallback(async (teamLocalId: number) => {
    const teamsClient = await getTeamsClient()
    return (await teamsClient.getById(teamLocalId)) as EditTeamFormValues
  }, [])

  const getTeamOfTeamsData = useCallback(async (teamLocalId: number) => {
    const teamsOfTeamsClient = await getTeamsOfTeamsClient()
    return (await teamsOfTeamsClient.getById(teamLocalId)) as EditTeamFormValues
  }, [])

  const loadData = useCallback(async () => {
    try {
      let teamData: EditTeamFormValues = null
      if (type === 'Team') {
        teamData = await getTeamData(key)
      } else if (type === 'Team of Teams') {
        teamData = await getTeamOfTeamsData(key)
      } else {
        throw new Error(`Invalid team type: ${type}`)
      }
      mapTeamToFormValues(teamData)
    } catch (error) {
      messageApi.error(
        `An unexpected error occurred while retrieving the ${type}.`
      )
      console.error(error)
    }
  }, [
    key,
    mapTeamToFormValues,
    messageApi,
    getTeamData,
    getTeamOfTeamsData,
    type,
  ])

  useEffect(() => {
    if (canUpdateTeam) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to update teams.')
    }

    if (showForm) {
      loadData()
    }
  }, [canUpdateTeam, loadData, messageApi, onFormCancel, showForm])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false)
    )
  }, [form, formValues])

  const update = async (values: EditTeamFormValues): Promise<boolean> => {
    try {
      if (type === 'Team') {
        await updateTeam(values)
      } else if (type === 'Team of Teams') {
        await updateTeamOfTeams(values)
      } else {
        throw new Error(`Invalid team type: ${type}`)
      }
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          `An unexpected error occurred while creating the ${type}.`
        )
        console.error(error)
      }

      return false
    }
  }

  const updateTeam = async (values: EditTeamFormValues) => {
    const teamsClient = await getTeamsClient()
    await teamsClient.update(values.id, values as UpdateTeamRequest)
  }

  const updateTeamOfTeams = async (values: EditTeamFormValues) => {
    const teamsOfTeamsClient = await getTeamsOfTeamsClient()
    await teamsOfTeamsClient.update(
      values.id,
      values as UpdateTeamOfTeamsRequest
    )
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        setIsSaving(false)
        form.resetFields()
        onFormUpdate()
        messageApi.success(`Successfully updated ${type}.`)
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
        title="Edit Team"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
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
          name="update-team-form"
        >
          <Form.Item name="id" hidden={true}>
            <Input />
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

export default EditTeamForm
