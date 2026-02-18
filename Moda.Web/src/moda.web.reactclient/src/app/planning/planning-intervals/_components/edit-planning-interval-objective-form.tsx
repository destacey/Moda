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
} from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../../components/contexts/auth'
import {
  PlanningIntervalObjectiveDetailsDto,
  UpdatePlanningIntervalObjectiveRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { RangePickerProps } from 'antd/es/date-picker'
import { MarkdownEditor } from '@/src/components/common/markdown'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useGetPlanningIntervalObjectiveQuery,
  useGetPlanningIntervalObjectiveStatusOptionsQuery,
  useGetPlanningIntervalQuery,
  useUpdatePlanningIntervalObjectiveMutation,
} from '@/src/store/features/planning/planning-interval-api'

const { Item } = Descriptions
const { Item: FormItem } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

export interface EditPlanningIntervalObjectiveFormProps {
  showForm: boolean
  planningIntervalKey: number
  objectiveKey: number
  onFormSave: () => void
  onFormCancel: () => void
}

interface EditPlanningIntervalObjectiveFormValues {
  objectiveId: string
  planningIntervalId: string
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
  values: EditPlanningIntervalObjectiveFormValues,
  objectiveKey: number,
  planningIntervalKey: number,
  teamId: string,
) => {
  const objective = {
    planningIntervalId: values.planningIntervalId,
    objectiveId: values.objectiveId,
    name: values.name,
    description: values.description,
    statusId: values.statusId,
    progress: values.progress,
    startDate: (values.startDate as any)?.format('YYYY-MM-DD'),
    targetDate: (values.targetDate as any)?.format('YYYY-MM-DD'),
    isStretch: values.isStretch,
  } as UpdatePlanningIntervalObjectiveRequest

  return {
    request: objective,
    objectiveKey,
    planningIntervalKey,
    teamId,
  }
}

const EditPlanningIntervalObjectiveForm = ({
  showForm,
  planningIntervalKey,
  objectiveKey,
  onFormSave,
  onFormCancel,
}: EditPlanningIntervalObjectiveFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditPlanningIntervalObjectiveFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const { data: planningIntervalData } =
    useGetPlanningIntervalQuery(planningIntervalKey)

  const { data: objectiveData } = useGetPlanningIntervalObjectiveQuery({
    planningIntervalKey: planningIntervalKey.toString(),
    objectiveKey: objectiveKey.toString(),
  })

  const { data: statusOptions } =
    useGetPlanningIntervalObjectiveStatusOptionsQuery()
  const [updateObjective, { error: mutationError }] =
    useUpdatePlanningIntervalObjectiveMutation()

  const { hasPermissionClaim } = useAuth()
  const canManageObjectives = hasPermissionClaim(
    'Permissions.PlanningIntervalObjectives.Manage',
  )

  const mapToFormValues = useCallback(
    (objective: PlanningIntervalObjectiveDetailsDto) => {
      if (!objective) {
        throw new Error('Objective not found')
      }
      form.setFieldsValue({
        objectiveId: objective.id,
        planningIntervalId: objective.planningInterval.id,
        teamId: objective.team.id,
        statusId: objective.status.id,
        name: objective.name,
        description: objective.description || '',
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
    values: EditPlanningIntervalObjectiveFormValues,
    objectiveKey: number,
    planningIntervalKey: number,
    teamId: string,
  ) => {
    try {
      const request = mapToRequestValues(
        values,
        objectiveKey,
        planningIntervalKey,
        teamId,
      )
      const response = await updateObjective(request)
      if (response.error) {
        throw response.error
      }

      messageApi.success('PI objective updated successfully.')

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
      if (
        await update(
          values,
          objectiveKey,
          planningIntervalKey,
          objectiveData?.team.id,
        )
      ) {
        setIsOpen(false)
        onFormSave()
        form.resetFields()
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
        current < dayjs(planningIntervalData?.start) ||
        current > dayjs(planningIntervalData?.end)
      )
    },
    [planningIntervalData?.start, planningIntervalData?.end],
  )

  const isDateWithinPiRange = useCallback(
    (date: Date) => {
      return (
        dayjs(planningIntervalData.start) <= dayjs(date) &&
        dayjs(date) <= dayjs(planningIntervalData.end)
      )
    },
    [planningIntervalData?.start, planningIntervalData?.end],
  )

  return (
    <>
      <Modal
        title="Edit PI Objective"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="update-objective-form"
        >
          {planningIntervalData?.objectivesLocked && (
            <Alert title="PI Objectives are locked." type="info" showIcon />
          )}
          <Descriptions size="small" column={1}>
            <Item label="Key">{objectiveData?.key}</Item>
            <Item label="Team">{objectiveData?.team.name}</Item>
          </Descriptions>
          <FormItem name="objectiveId" hidden={true}>
            <Input />
          </FormItem>
          <FormItem name="planningIntervalId" hidden={true}>
            <Input />
          </FormItem>
          <FormItem name="teamId" hidden={true}>
            <Input />
          </FormItem>
          <FormItem label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 2, maxRows: 4 }}
              showCount
              maxLength={256}
              disabled={planningIntervalData?.objectivesLocked}
            />
          </FormItem>
          <FormItem
            name="description"
            label="Description"
            rules={[{ max: 1024 }]}
          >
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={1024}
            />
          </FormItem>
          <FormItem
            label="Is Stretch?"
            name="isStretch"
            valuePropName="checked"
          >
            <Switch
              checkedChildren="Yes"
              unCheckedChildren="No"
              disabled={planningIntervalData?.objectivesLocked}
            />
          </FormItem>
          <FormItem name="statusId" label="Status" rules={[{ required: true }]}>
            <RadioGroup
              options={statusOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </FormItem>
          <FormItem label="Progress" name="progress">
            <Slider />
          </FormItem>
          <FormItem
            label="Start"
            name="startDate"
            rules={[
              {
                validator: (_, value: Date) =>
                  !value || isDateWithinPiRange(value)
                    ? Promise.resolve()
                    : Promise.reject(
                        'The start date must be within the Planning Interval dates.',
                      ),
              },
            ]}
          >
            <DatePicker disabledDate={disabledDate} />
          </FormItem>
          <FormItem
            label="Target"
            name="targetDate"
            dependencies={['startDate']}
            rules={[
              {
                validator: (_, value: Date) =>
                  !value || isDateWithinPiRange(value)
                    ? Promise.resolve()
                    : Promise.reject(
                        'The target date must be within the Planning Interval dates.',
                      ),
              },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  const start = getFieldValue('startDate')
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
            <DatePicker disabledDate={disabledDate} />
          </FormItem>
        </Form>
      </Modal>
    </>
  )
}

export default EditPlanningIntervalObjectiveForm
