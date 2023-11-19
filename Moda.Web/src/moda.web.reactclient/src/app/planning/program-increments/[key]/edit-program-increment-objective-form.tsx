'use client'

import {
  Alert,
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
import {
  ProgramIncrementObjectiveDetailsDto,
  UpdateProgramIncrementObjectiveRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { RangePickerProps } from 'antd/es/date-picker'
import _ from 'lodash'
import {
  UpdateProgramIncrementObjectiveMutationRequest,
  useGetProgramIncrementById,
  useGetProgramIncrementObjectiveById,
  useGetProgramIncrementObjectiveStatusOptions,
  useUpdateProgramIncrementObjectiveMutation,
} from '@/src/services/queries/planning-queries'

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

const mapToRequestValues = (
  values: EditProgramIncrementObjectiveFormValues,
  programIncrementKey: number,
) => {
  const objective = {
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
  return {
    objective,
    programIncrementKey,
  } as UpdateProgramIncrementObjectiveMutationRequest
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

  const { data: programIncrementData } =
    useGetProgramIncrementById(programIncrementId)
  const { data: objectiveData } = useGetProgramIncrementObjectiveById(
    programIncrementId,
    objectiveId,
  )
  const { data: statusOptions } = useGetProgramIncrementObjectiveStatusOptions()
  const updateObjective = useUpdateProgramIncrementObjectiveMutation()

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.ProgramIncrementObjectives.Manage',
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
    [form],
  )

  const update = async (
    values: EditProgramIncrementObjectiveFormValues,
    programIncrementKey: number,
  ) => {
    try {
      const request = mapToRequestValues(values, programIncrementKey)
      await updateObjective.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while updating the PI objective.',
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
      if (await update(values, objectiveData?.programIncrement.key)) {
        setIsOpen(false)
        onFormSave()
        form.resetFields()
        messageApi.success('Successfully updated PI objective.')
      }
    } catch (errorInfo) {
      console.error('handleOk error', errorInfo)
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
    if (!objectiveData) return
    if (canManageObjectives) {
      setIsOpen(showForm)
      if (showForm) {
        mapToFormValues(objectiveData)
      }
    } else {
      handleCancel()
      messageApi.error('You do not have permission to update PI objectives.')
    }
  }, [
    canManageObjectives,
    handleCancel,
    mapToFormValues,
    messageApi,
    objectiveData,
    showForm,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

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
        dayjs(programIncrementData?.start) <= dayjs(date) &&
        dayjs(date) < dayjs(programIncrementData?.end).add(1, 'day')
      )
    },
    [programIncrementData?.start, programIncrementData?.end],
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
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="update-objective-form"
        >
          {programIncrementData?.objectivesLocked && (
            <Alert message="PI Objectives are locked." type="info" showIcon />
          )}
          <Descriptions size="small" column={1}>
            <Descriptions.Item label="Number">
              {objectiveData?.key}
            </Descriptions.Item>
            <Descriptions.Item label="Team">
              {objectiveData?.team.name}
            </Descriptions.Item>
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
              autoSize={{ minRows: 2, maxRows: 4 }}
              showCount
              maxLength={256}
              disabled={programIncrementData?.objectivesLocked}
            />
          </Form.Item>
          <Form.Item
            name="description"
            label="Description"
            extra="Markdown enabled"
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
            <Switch
              checkedChildren="Yes"
              unCheckedChildren="No"
              disabled={programIncrementData?.objectivesLocked}
            />
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

export default EditProgramIncrementObjectiveForm
