'use client'

import { DatePicker, Form, Input, Modal, Select, Switch, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../components/contexts/auth'
import { CreateProgramIncrementObjectiveRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { RangePickerProps } from 'antd/es/date-picker'
import {
  useCreateProgramIncrementObjectiveMutation,
  useGetProgramIncrementById,
  useGetProgramIncrementObjectiveStatuses,
  useGetProgramIncrementTeams,
} from '@/src/services/queries/planning-queries'

export interface CreateProgramIncrementObjectiveFormProps {
  showForm: boolean
  programIncrementId: string
  teamId?: string
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

const mapToRequestValues = (
  values: CreateProgramIncrementObjectiveFormValues,
) => {
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
}

const CreateProgramIncrementObjectiveForm = ({
  showForm,
  programIncrementId,
  teamId,
  onFormCreate,
  onFormCancel,
}: CreateProgramIncrementObjectiveFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateProgramIncrementObjectiveFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()
  const [teams, setTeams] = useState<ProgramIncrementTeamSelectItem[]>([])
  const [defaultStatusId, setDefaultStatusId] = useState<number>(null)
  const [newObjectiveKey, setNewObjectiveKey] = useState<number>(null)

  const { data: programIncrementData } =
    useGetProgramIncrementById(programIncrementId)
  const { data: teamData } = useGetProgramIncrementTeams(programIncrementId)
  const { data: statusData } = useGetProgramIncrementObjectiveStatuses()

  const createObjective = useCreateProgramIncrementObjectiveMutation()

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage',
  )

  const mapToFormValues = useCallback(
    (programIncrementId: string, statusId: number, teamId?: string) => {
      form.setFieldsValue({
        programIncrementId: programIncrementId,
        teamId: teamId,
        statusId: statusId,
        isStretch: false,
      })
    },
    [form],
  )

  const create = async (
    values: CreateProgramIncrementObjectiveFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      const key = await createObjective.mutateAsync(request)
      setNewObjectiveKey(key)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.supportMessage ??
            'An unexpected error occurred while creating the program increment.',
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
      await create(values)
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }, [form, onFormCancel])

  useEffect(() => {
    if (!teamData || !statusData) return

    if (canManageObjectives) {
      setIsOpen(showForm)
      if (showForm === true) {
        try {
          setDefaultStatusId(statusData.find((s) => s.order === 1)?.id)
          setTeams(
            teamData
              .filter((t) => t.type === 'Team')
              .map((t) => ({ value: t.id, label: t.name })),
          )
          mapToFormValues(programIncrementId, defaultStatusId, teamId)
        } catch (error) {
          handleCancel()
          messageApi.error(
            'An unexpected error occurred while loading form data.',
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
    teamId,
    statusData,
    teamData,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (newObjectiveKey) {
      setIsOpen(false)
      form.resetFields()
      onFormCreate()
      messageApi.success('Successfully created PI objective.')
    }
    // we don't want a trigger on the other dependencies
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [newObjectiveKey])

  const disabledDate: RangePickerProps['disabledDate'] = useCallback(
    (current) => {
      return (
        current < dayjs(programIncrementData?.start) ||
        current > dayjs(programIncrementData?.end).add(1, 'day')
      )
    },
    [programIncrementData?.start, programIncrementData?.end],
  )

  const isDateWithinPiRange = useCallback(
    (date: Date) => {
      return (
        dayjs(programIncrementData.start) <= dayjs(date) &&
        dayjs(date) < dayjs(programIncrementData.end).add(1, 'day')
      )
    },
    [programIncrementData?.start, programIncrementData?.end],
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
        keyboard={false} // disable esc key to close modal
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
              disabled={teamId !== undefined}
              placeholder="Select a team"
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
              options={teams}
            />
          </Form.Item>
          <Form.Item label="Name" name="name" rules={[{ required: true }]}>
            <Input.TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
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
          <Form.Item
            label="Is Stretch?"
            name="isStretch"
            valuePropName="checked"
          >
            <Switch checkedChildren="Yes" unCheckedChildren="No" />
          </Form.Item>
          <Form.Item
            label="Start"
            name="startDate"
            rules={[
              {
                validator: (_, value: Date) =>
                  !value || isDateWithinPiRange(value)
                    ? Promise.resolve()
                    : Promise.reject(
                        'The start date must be within the Program Increment dates.',
                      ),
              },
            ]}
          >
            <DatePicker disabledDate={disabledDate} />
          </Form.Item>
          <Form.Item
            label="Target"
            name="targetDate"
            rules={[
              {
                validator: (_, value: Date) =>
                  !value || isDateWithinPiRange(value)
                    ? Promise.resolve()
                    : Promise.reject(
                        'The target date must be within the Program Increment dates.',
                      ),
              },
            ]}
          >
            <DatePicker disabledDate={disabledDate} />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateProgramIncrementObjectiveForm
