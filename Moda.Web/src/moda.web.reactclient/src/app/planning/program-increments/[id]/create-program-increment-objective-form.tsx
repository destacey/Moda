'use client'

import { DatePicker, Form, Input, Modal, Select, Switch, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../components/contexts/auth'
import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  CreateProgramIncrementObjectiveRequest,
  ProgramIncrementDetailsDto,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { RangePickerProps } from 'antd/es/date-picker'

export interface CreateProgramIncrementObjectiveFormProps {
  showForm: boolean
  programIncrementId: string
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateProgramIncrementObjectiveFormValues {
  programIncrementId: string
  teamId: string
  name: string
  statusId: number
  description?: string | null
  isStretch: boolean
  startDate?: Date | null
  targetDate?: Date | null
}

interface ProgramIncrementTeamSelectItem {
  value: string
  label: string
}

const CreateProgramIncrementObjectiveForm = ({
  showForm,
  programIncrementId,
  onFormCreate,
  onFormCancel,
}: CreateProgramIncrementObjectiveFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateProgramIncrementObjectiveFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()
  const [programIncrement, setProgramIncrement] =
    useState<ProgramIncrementDetailsDto | null>(null)
  const [teams, setTeams] = useState<ProgramIncrementTeamSelectItem[]>([])
  const [defaultStatusId, setDefaultStatusId] = useState<number>(null)
  const [newObjectiveLocalId, setNewObjectiveLocalId] = useState<number>(null)

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage'
  )

  const mapToFormValues = useCallback(
    (programIncrementId: string, statusId: number) => {
      form.setFieldsValue({
        programIncrementId: programIncrementId,
        statusId: statusId,
        isStretch: false,
      })
    },
    [form]
  )

  const mapToRequestValues = useCallback(
    (values: CreateProgramIncrementObjectiveFormValues) => {
      return {
        programIncrementId: values.programIncrementId,
        teamId: values.teamId,
        name: values.name,
        statusId: values.statusId,
        description: values.description,
        isStretch: values.isStretch,
        startDate: (values.startDate as any)?.format('YYYY-MM-DD'),
        targetDate: (values.targetDate as any)?.format('YYYY-MM-DD'),
      } as CreateProgramIncrementObjectiveRequest
    },
    []
  )

  const getProgramIncrement = async (id: string) => {
    const programIncrementsClient = await getProgramIncrementsClient()
    return await programIncrementsClient.getById(id)
  }

  // currently only allowing PI objectives for Teams.
  const getProgramIncrementTeams = async (id: string) => {
    const programIncrementsClient = await getProgramIncrementsClient()
    var teams = await programIncrementsClient.getTeams(id)
    return teams
      .filter((t) => t.type === 'Team')
      .map((t) => ({ value: t.id, label: t.name }))
  }

  const getProgramIncrementStatuses = async () => {
    const programIncrementsClient = await getProgramIncrementsClient()
    return await programIncrementsClient.getObjectiveStatuses()
  }

  const createObjective = useCallback(
    async (values: CreateProgramIncrementObjectiveFormValues) => {
      const request = mapToRequestValues(values)
      const programIncrementsClient = await getProgramIncrementsClient()
      const localId = await programIncrementsClient.createObjective(
        values.programIncrementId,
        request
      )
      return localId
    },
    [mapToRequestValues]
  )

  const create = useCallback(
    async (
      values: CreateProgramIncrementObjectiveFormValues
    ): Promise<boolean> => {
      try {
        const localId = await createObjective(values)
        console.log(`localId from create: ${localId}`)
        setNewObjectiveLocalId(localId)
        return true
      } catch (error) {
        if (error.status === 422 && error.errors) {
          const formErrors = toFormErrors(error.errors)
          form.setFields(formErrors)
          messageApi.error('Correct the validation error(s) to continue.')
        } else {
          messageApi.error(
            'An unexpected error occurred while creating the PI objective.'
          )
          console.error(error)
        }
        return false
      }
    },
    [createObjective, form, messageApi]
  )

  const handleOk = useCallback(async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      await create(values)
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }, [form, create])

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }, [form, onFormCancel])

  useEffect(() => {
    if (canManageObjectives) {
      setIsOpen(showForm)
      if (showForm === true) {
        try {
          const loadData = async () => {
            setProgramIncrement(await getProgramIncrement(programIncrementId))
            setTeams(await getProgramIncrementTeams(programIncrementId))
            const statuses = await getProgramIncrementStatuses()
            setDefaultStatusId(statuses.find((s) => s.order === 1)?.id)
          }
          loadData()
          mapToFormValues(programIncrementId, defaultStatusId)
        } catch (error) {
          handleCancel()
          messageApi.error(
            'An unexpected error occurred while loading form data.'
          )
          console.error(error)
        }
      }
    } else {
      handleCancel()
      messageApi.error('You do not have permission to create PI objectives.')
    }
  }, [
    canManageObjectives,
    handleCancel,
    showForm,
    messageApi,
    mapToFormValues,
    programIncrementId,
    defaultStatusId,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false)
    )
  }, [form, formValues])

  useEffect(() => {
    console.log(`useEffect for closing the form.`)
    if (newObjectiveLocalId) {
      setIsOpen(false)
      form.resetFields()
      onFormCreate()
      messageApi.success('Successfully created PI objective.')
    }
    // we don't want a trigger on the other dependencies
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [newObjectiveLocalId])

  const disabledDate: RangePickerProps['disabledDate'] = useCallback(
    (current) => {
      return (
        current < dayjs(programIncrement?.start) ||
        current > dayjs(programIncrement?.end).add(1, 'day')
      )
    },
    [programIncrement?.end, programIncrement?.start]
  )

  return (
    <>
      {contextHolder}
      <Modal
        title="Create PI Objective"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        closable={false}
        destroyOnClose={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="create-objective-form"
        >
          <Form.Item name="programIncrementId" hidden={true}>
            <Input />
          </Form.Item>
          <Form.Item name="statusId" hidden={true}>
            <Input />
          </Form.Item>
          <Form.Item name="teamId" label="Team" rules={[{ required: true }]}>
            <Select
              showSearch
              placeholder="Select a team"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.label.toLowerCase() ?? '').includes(
                  input.toLowerCase()
                )
              }
              filterSort={(optionA, optionB) =>
                (optionA?.label ?? '')
                  .toLowerCase()
                  .localeCompare((optionB?.label ?? '').toLowerCase())
              }
              options={teams}
            />
          </Form.Item>
          <Form.Item label="Name" name="name" rules={[{ required: true }]}>
            <Input showCount maxLength={128} />
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
          <Form.Item
            label="Is Stretch?"
            name="isStretch"
            valuePropName="checked"
          >
            <Switch checkedChildren="Yes" unCheckedChildren="No" />
          </Form.Item>
          <Form.Item label="Start" name="startDate">
            <DatePicker disabledDate={disabledDate} />
          </Form.Item>
          <Form.Item label="Target" name="targetDate">
            <DatePicker disabledDate={disabledDate} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateProgramIncrementObjectiveForm
