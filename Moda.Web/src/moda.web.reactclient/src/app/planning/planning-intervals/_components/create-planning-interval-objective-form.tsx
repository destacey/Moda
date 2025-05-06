'use client'

import { DatePicker, Form, Input, Modal, Select, Switch } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../../components/contexts/auth'
import { CreatePlanningIntervalObjectiveRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { RangePickerProps } from 'antd/es/date-picker'
import { MarkdownEditor } from '@/src/components/common/markdown'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useCreatePlanningIntervalObjectiveMutation,
  useGetPlanningIntervalObjectiveStatusesQuery,
  useGetPlanningIntervalQuery,
  useGetPlanningIntervalTeamsQuery,
} from '@/src/store/features/planning/planning-interval-api'

const { Item: FormItem } = Form
const { TextArea } = Input

export interface CreatePlanningIntervalObjectiveFormProps {
  showForm: boolean
  planningIntervalKey: number
  teamId?: string
  order?: number
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreatePlanningIntervalObjectiveFormValues {
  planningIntervalId: string
  teamId: string
  name: string
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
  order?: number,
): CreatePlanningIntervalObjectiveRequest => {
  return {
    planningIntervalId: values.planningIntervalId,
    teamId: values.teamId,
    name: values.name,
    description: values.description,
    isStretch: values.isStretch,
    startDate: (values.startDate as any)?.format('YYYY-MM-DD'),
    targetDate: (values.targetDate as any)?.format('YYYY-MM-DD'),
    order: order,
  }
}

const CreatePlanningIntervalObjectiveForm = ({
  showForm,
  planningIntervalKey,
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
  const messageApi = useMessage()
  const [teams, setTeams] = useState<PlanningIntervalTeamSelectItem[]>([])

  const { data: planningIntervalData } =
    useGetPlanningIntervalQuery(planningIntervalKey)
  const { data: teamData } =
    useGetPlanningIntervalTeamsQuery(planningIntervalKey)
  const { data: statusData } = useGetPlanningIntervalObjectiveStatusesQuery()

  const [createObjective, { error: mutationError }] =
    useCreatePlanningIntervalObjectiveMutation()

  const { hasPermissionClaim } = useAuth()
  const canManageObjectives = hasPermissionClaim(
    'Permissions.PlanningIntervalObjectives.Manage',
  )

  const mapToFormValues = useCallback(
    (planningIntervalId: string, teamId?: string) => {
      form.setFieldsValue({
        planningIntervalId: planningIntervalId,
        teamId: teamId,
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
      const request = mapToRequestValues(values, order)
      const response = await createObjective({
        request,
        planningIntervalKey,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success('Successfully created PI objective.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An unexpected error occurred while creating the PI objective.',
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
          setTeams(
            teamData
              .filter((t) => t.type === 'Team')
              .map((t) => ({ value: t.id, label: t.name })),
          )
          mapToFormValues(planningIntervalData?.id, teamId)
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
    teamId,
    statusData,
    teamData,
    planningIntervalData?.id,
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
          initialValues={{ isStretch: false }} // used to set default value for switch
        >
          <FormItem name="planningIntervalId" hidden={true}>
            <Input />
          </FormItem>
          <FormItem name="teamId" label="Team" rules={[{ required: true }]}>
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
          </FormItem>
          <FormItem label="Name" name="name" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 2, maxRows: 4 }}
              showCount
              maxLength={256}
            />
          </FormItem>
          <FormItem
            name="description"
            label="Description"
            initialValue=""
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
            <Switch checkedChildren="Yes" unCheckedChildren="No" />
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

export default CreatePlanningIntervalObjectiveForm
