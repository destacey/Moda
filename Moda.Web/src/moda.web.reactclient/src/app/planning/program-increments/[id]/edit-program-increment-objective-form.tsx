'use client'

import {
  DatePicker,
  Descriptions,
  Form,
  Input,
  Modal,
  Radio,
  Slider,
  Switch,
  message,
} from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../components/contexts/auth'
import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveDetailsDto,
  UpdateProgramIncrementObjectiveRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { RangePickerProps } from 'antd/es/date-picker'
import _ from 'lodash'

export interface EditProgramIncrementObjectiveFormProps {
  showForm: boolean
  programIncrementId: string
  objectiveId: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface EditProgramIncrementObjectiveFormValues {
  objectiveId: string
  programIncrementId: string
  teamId: string
  name: string
  statusId: number
  description?: string | null
  isStretch: boolean
  progress: number
  startDate?: Date | null
  targetDate?: Date | null
}

interface OptionModel<T = string> {
  value: T
  label: string
}

const EditProgramIncrementObjectiveForm = ({
  showForm,
  programIncrementId,
  objectiveId,
  onFormSave,
  onFormCancel,
}: EditProgramIncrementObjectiveFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditProgramIncrementObjectiveFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const [objectiveNumber, setObjectiveNumber] = useState<number>(undefined)
  const [teamName, setTeamName] = useState<string>('')
  const [programIncrement, setProgramIncrement] =
    useState<ProgramIncrementDetailsDto>(undefined)
  const [statusOptions, setStatusOptions] = useState<OptionModel<number>[]>([])

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage'
  )

  const mapToFormValues = useCallback(
    (objective: ProgramIncrementObjectiveDetailsDto) => {
      if (!objective) {
        throw new Error('Objective not found')
      }
      form.setFieldsValue({
        objectiveId: objective.id,
        programIncrementId: objective.programIncrement.id,
        teamId: objective.team.id,
        statusId: objective.status.id,
        name: objective.name,
        description: objective.description,
        startDate: objective.startDate ? dayjs(objective.startDate) : undefined,
        targetDate: objective.targetDate
          ? dayjs(objective.targetDate)
          : undefined,
        progress: objective.progress,
        isStretch: objective.isStretch,
      })
    },
    [form]
  )

  const mapToRequestValues = useCallback(
    (values: EditProgramIncrementObjectiveFormValues) => {
      return {
        objectiveId: values.objectiveId,
        programIncrementId: values.programIncrementId,
        teamId: values.teamId,
        name: values.name,
        statusId: values.statusId,
        description: values.description,
        isStretch: values.isStretch,
        progress: values.progress,
        startDate: (values.startDate as any)?.format('YYYY-MM-DD'),
        targetDate: (values.targetDate as any)?.format('YYYY-MM-DD'),
      } as UpdateProgramIncrementObjectiveRequest
    },
    []
  )

  const getObjective = useCallback(
    async (programIncrementId: string, objectiveId: string) => {
      const programIncrementsClient = await getProgramIncrementsClient()
      return await programIncrementsClient.getObjectiveById(
        programIncrementId,
        objectiveId
      )
    },
    []
  )

  const getProgramIncrement = useCallback(async (id: string) => {
    const programIncrementsClient = await getProgramIncrementsClient()
    return await programIncrementsClient.getById(id)
  }, [])

  const getProgramIncrementStatuses = useCallback(async () => {
    const programIncrementsClient = await getProgramIncrementsClient()
    const statusDtos = await programIncrementsClient.getObjectiveStatuses()
    const statuses: OptionModel<number>[] = statusDtos.map((r) => ({
      value: r.id,
      label: r.name,
    }))
    return _.sortBy(statuses, ['order'])
  }, [])

  const updateObjective = useCallback(
    async (values: EditProgramIncrementObjectiveFormValues) => {
      const request = mapToRequestValues(values)
      const programIncrementsClient = await getProgramIncrementsClient()
      await programIncrementsClient.updateObjective(
        values.programIncrementId,
        values.objectiveId,
        request
      )
    },
    [mapToRequestValues]
  )

  const update = useCallback(
    async (values: EditProgramIncrementObjectiveFormValues) => {
      try {
        await updateObjective(values)
        return true
      } catch (error) {
        if (error.status === 422 && error.errors) {
          const formErrors = toFormErrors(error.errors)
          form.setFields(formErrors)
          messageApi.error('Correct the validation error(s) to continue.')
        } else {
          messageApi.error(
            'An unexpected error occurred while updating the PI objective.'
          )
          console.error(error)
        }
        return false
      }
    },
    [updateObjective, form, messageApi]
  )

  const handleOk = useCallback(async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        onFormSave()
        form.resetFields()
        messageApi.success('Successfully updated PI objective.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }, [form, messageApi, onFormSave, update])

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }, [form, onFormCancel])

  const loadData = useCallback(async () => {
    try {
      const objectiveData = await getObjective(programIncrementId, objectiveId)
      setObjectiveNumber(objectiveData.localId)
      setTeamName(objectiveData.team.name)
      mapToFormValues(objectiveData)
      setProgramIncrement(await getProgramIncrement(programIncrementId))
      setStatusOptions(await getProgramIncrementStatuses())
    } catch (error) {
      handleCancel()
      messageApi.error('An unexpected error occurred while loading form data.')
      console.error(error)
    }
  }, [
    getObjective,
    getProgramIncrement,
    getProgramIncrementStatuses,
    handleCancel,
    mapToFormValues,
    messageApi,
    objectiveId,
    programIncrementId,
  ])

  useEffect(() => {
    if (canManageObjectives) {
      setIsOpen(showForm)
      if (showForm) {
        loadData()
      }
    } else {
      handleCancel()
      messageApi.error('You do not have permission to update PI objectives.')
    }
  }, [canManageObjectives, showForm, loadData, handleCancel, messageApi])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false)
    )
  }, [form, formValues])

  const disabledDate: RangePickerProps['disabledDate'] = useCallback(
    (current) => {
      return (
        current < dayjs(programIncrement?.start) ||
        current > dayjs(programIncrement?.end).add(1, 'day')
      )
    },
    [programIncrement?.end, programIncrement?.start]
  )

  const isDateWithinPiRange = useCallback(
    (date: Date) => {
      return (
        dayjs(programIncrement.start) <= dayjs(date) &&
        dayjs(date) < dayjs(programIncrement.end).add(1, 'day')
      )
    },
    [programIncrement]
  )

  return (
    <>
      {contextHolder}
      <Modal
        title="Edit PI Objective"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        closable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="update-objective-form"
        >
          <Descriptions size="small" column={1}>
            <Descriptions.Item label="Number">
              {objectiveNumber}
            </Descriptions.Item>
            <Descriptions.Item label="Team">{teamName}</Descriptions.Item>
          </Descriptions>
          <Form.Item name="objectiveId" hidden={true}>
            <Input />
          </Form.Item>
          <Form.Item name="programIncrementId" hidden={true}>
            <Input />
          </Form.Item>
          <Form.Item name="teamId" hidden={true}>
            <Input />
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
            name="statusId"
            label="Status"
            rules={[{ required: true }]}
          >
            <Radio.Group
              options={statusOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </Form.Item>
          <Form.Item label="Progress" name="progress">
            <Slider />
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
                        'The start date must be within the Program Increment dates.'
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
                        'The target date must be within the Program Increment dates.'
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

export default EditProgramIncrementObjectiveForm
