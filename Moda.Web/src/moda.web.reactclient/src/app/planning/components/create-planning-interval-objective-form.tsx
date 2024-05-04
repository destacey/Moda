'use client'

import { DatePicker, Form, Input, Modal, Select, Switch, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import { CreatePlanningIntervalObjectiveRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { RangePickerProps } from 'antd/es/date-picker'
import {
  CreatePlanningIntervalObjectiveMutationRequest,
  useCreatePlanningIntervalObjectiveMutation,
  useGetPlanningIntervalById,
  useGetPlanningIntervalObjectiveStatuses,
  useGetPlanningIntervalTeams,
} from '@/src/services/queries/planning-queries'

const { Item } = Form
const { TextArea } = Input

export interface CreatePlanningIntervalObjectiveFormProps {
  showForm: boolean
  planningIntervalId: string
  teamId?: string
  order?: number
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreatePlanningIntervalObjectiveFormValues {
  planningIntervalId: string
  teamId: string
  name: string
  statusId: number
  description?: string | null
  isStretch: boolean
  startDate?: Date | null
  targetDate?: Date | null
}

interface PlanningIntervalTeamSelectItem {
  value: string
  label: string
}

const mapToRequestValues = (
  values: CreatePlanningIntervalObjectiveFormValues,
  planningIntervalKey: number,
  order?: number,
) => {
  const objective = {
    planningIntervalId: values.planningIntervalId,
    teamId: values.teamId,
    name: values.name,
    statusId: values.statusId,
    description: values.description,
    isStretch: values.isStretch,
    startDate: (values.startDate as any)?.format('YYYY-MM-DD'),
    targetDate: (values.targetDate as any)?.format('YYYY-MM-DD'),
    order: order,
  } as CreatePlanningIntervalObjectiveRequest
  return {
    objective,
    planningIntervalKey,
  } as CreatePlanningIntervalObjectiveMutationRequest
}

const CreatePlanningIntervalObjectiveForm = ({
  showForm,
  planningIntervalId,
  teamId,
  order,
  onFormCreate,
  onFormCancel,
}: CreatePlanningIntervalObjectiveFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreatePlanningIntervalObjectiveFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()
  const [teams, setTeams] = useState<PlanningIntervalTeamSelectItem[]>([])
  const [defaultStatusId, setDefaultStatusId] = useState<number>(null)

  const { data: planningIntervalData } =
    useGetPlanningIntervalById(planningIntervalId)
  const { data: teamData } = useGetPlanningIntervalTeams(planningIntervalId)
  const { data: statusData } = useGetPlanningIntervalObjectiveStatuses()

  const createObjective = useCreatePlanningIntervalObjectiveMutation()

  const { hasClaim } = useAuth()
  const canManageObjectives = hasClaim(
    'Permission',
    'Permissions.PlanningIntervalObjectives.Manage',
  )

  console.log('order', order)

  const mapToFormValues = useCallback(
    (planningIntervalId: string, statusId: number, teamId?: string) => {
      form.setFieldsValue({
        planningIntervalId: planningIntervalId,
        teamId: teamId,
        statusId: statusId,
        isStretch: false,
      })
    },
    [form],
  )

  const create = async (
    values: CreatePlanningIntervalObjectiveFormValues,
    planningIntervalKey: number,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values, planningIntervalKey, order)
      const key = await createObjective.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.supportMessage ??
            'An unexpected error occurred while creating the planning interval.',
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
      if (await create(values, planningIntervalData?.key)) {
        setIsOpen(false)
        form.resetFields()
        onFormCreate()
        // TODO: this message is not displaying
        messageApi.success('Successfully created PI objective.')
      }
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
          mapToFormValues(planningIntervalId, defaultStatusId, teamId)
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
    planningIntervalId,
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

  const disabledDate: RangePickerProps['disabledDate'] = useCallback(
    (current) => {
      return (
        current < dayjs(planningIntervalData?.start) ||
        current > dayjs(planningIntervalData?.end).add(1, 'day')
      )
    },
    [planningIntervalData?.start, planningIntervalData?.end],
  )

  const isDateWithinPiRange = useCallback(
    (date: Date) => {
      return (
        dayjs(planningIntervalData.start) <= dayjs(date) &&
        dayjs(date) < dayjs(planningIntervalData.end).add(1, 'day')
      )
    },
    [planningIntervalData?.start, planningIntervalData?.end],
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
          <Item name="planningIntervalId" hidden={true}>
            <Input />
          </Item>
          <Item name="statusId" hidden={true}>
            <Input />
          </Item>
          <Item name="teamId" label="Team" rules={[{ required: true }]}>
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
          </Item>
          <Item label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 2, maxRows: 4 }}
              showCount
              maxLength={256}
            />
          </Item>
          <Item name="description" label="Description" extra="Markdown enabled">
            <TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Item>
          <Item label="Is Stretch?" name="isStretch" valuePropName="checked">
            <Switch checkedChildren="Yes" unCheckedChildren="No" />
          </Item>
          <Item
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
          </Item>
          <Item
            label="Target"
            name="targetDate"
            rules={[
              {
                validator: (_, value: Date) =>
                  !value || isDateWithinPiRange(value)
                    ? Promise.resolve()
                    : Promise.reject(
                        'The target date must be within the Planning Interval dates.',
                      ),
              },
            ]}
          >
            <DatePicker disabledDate={disabledDate} />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreatePlanningIntervalObjectiveForm
