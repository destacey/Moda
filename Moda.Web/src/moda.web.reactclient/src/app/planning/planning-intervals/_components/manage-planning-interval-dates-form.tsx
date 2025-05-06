'use client'

import {
  Button,
  DatePicker,
  Divider,
  Flex,
  Form,
  Input,
  Modal,
  Select,
} from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '@/src/components/contexts/auth'
import {
  PlanningIntervalDetailsDto,
  ManagePlanningIntervalDatesRequest,
  PlanningIntervalIterationListDto,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { MinusCircleOutlined, PlusOutlined } from '@ant-design/icons'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useGetPlanningIntervalIterationsQuery,
  useGetPlanningIntervalIterationTypeOptionsQuery,
  useGetPlanningIntervalQuery,
  useUpdatePlanningIntervalDatesMutation,
} from '@/src/store/features/planning/planning-interval-api'

const { Item, List } = Form

export interface ManagePlanningIntervalDatesFormProps {
  showForm: boolean
  id: string
  planningIntervalKey: number
  onFormSave: () => void
  onFormCancel: () => void
}

interface UpsertIterationFormValues {
  iterationId?: string
  name: string
  typeId: number
  start: Date
  end: Date
}

interface ManagePlanningIntervalDatesFormValues {
  start: Date
  end: Date
  iterations: UpsertIterationFormValues[]
}

const mapToRequestValues = (
  values: ManagePlanningIntervalDatesFormValues,
  id: string,
) => {
  return {
    id,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    iterations: values.iterations.map((iteration) => ({
      iterationId: iteration.iterationId,
      name: iteration.name,
      typeId: iteration.typeId,
      start: (iteration.start as any)?.format('YYYY-MM-DD'),
      end: (iteration.end as any)?.format('YYYY-MM-DD'),
    })),
  } as ManagePlanningIntervalDatesRequest
}

const ManagePlanningIntervalDatesForm = ({
  showForm,
  id,
  planningIntervalKey,
  onFormSave,
  onFormCancel,
}: ManagePlanningIntervalDatesFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)

  const [form] = Form.useForm<ManagePlanningIntervalDatesFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const { data: planningIntervalData } =
    useGetPlanningIntervalQuery(planningIntervalKey)
  const { data: iterationsData } =
    useGetPlanningIntervalIterationsQuery(planningIntervalKey)
  const { data: iterationTypesOptions } =
    useGetPlanningIntervalIterationTypeOptionsQuery()

  const [managePlanningIntervalDates, { error: mutationError }] =
    useUpdatePlanningIntervalDatesMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdatePlanningInterval = hasPermissionClaim(
    'Permissions.PlanningIntervals.Update',
  )

  const mapToFormValues = useCallback(
    (
      planningInterval: PlanningIntervalDetailsDto,
      iterationsData: PlanningIntervalIterationListDto[],
    ) => {
      form.setFieldsValue({
        start: dayjs(planningInterval.start),
        end: dayjs(planningInterval.end),
        iterations: iterationsData.map((iteration) => ({
          iterationId: iteration.id,
          name: iteration.name,
          typeId: iteration.type.id,
          start: dayjs(iteration.start),
          end: dayjs(iteration.end),
        })),
      })
    },
    [form],
  )

  const update = async (
    values: ManagePlanningIntervalDatesFormValues,
    id: string,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values, id)
      const response = await managePlanningIntervalDates({
        request,
        cacheKey: planningIntervalKey,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success('Planning interval dates updated successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the planning interval dates.',
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
      if (await update(values, id)) {
        setIsOpen(false)
        form.resetFields()
        onFormSave()
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
  }, [onFormCancel, form])

  const loadData = useCallback(async () => {
    try {
      mapToFormValues(planningIntervalData, iterationsData)
      setIsValid(true)
    } catch (error) {
      handleCancel()
      messageApi.error('An unexpected error occurred while loading form data.')
      console.error(error)
    }
  }, [
    handleCancel,
    iterationsData,
    mapToFormValues,
    messageApi,
    planningIntervalData,
  ])

  useEffect(() => {
    if (!planningIntervalData || !iterationsData || !iterationTypesOptions)
      return
    if (canUpdatePlanningInterval) {
      setIsOpen(showForm)
      if (showForm) {
        loadData()
      }
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to edit planning intervals.')
    }
  }, [
    canUpdatePlanningInterval,
    iterationTypesOptions,
    iterationsData,
    loadData,
    messageApi,
    onFormCancel,
    planningIntervalData,
    showForm,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <Modal
      title="Manage PI Dates"
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
        name="manage-planning-interval-dates-form"
      >
        <Item label="Start" name="start" rules={[{ required: true }]}>
          <DatePicker />
        </Item>
        <Item
          label="End"
          name="end"
          dependencies={['start']}
          rules={[
            { required: true },
            ({ getFieldValue }) => ({
              validator(_, value) {
                const start = getFieldValue('start')
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
          <DatePicker />
        </Item>

        <Divider orientation="left">Iterations</Divider>
        <List name="iterations">
          {(fields, { add, remove }) => (
            <>
              {fields.map(({ key, name, ...restField }) => (
                <div key={key}>
                  <Flex align="center" justify="space-between">
                    <Flex vertical style={{ width: '90%' }}>
                      <Flex gap="small">
                        <Item
                          {...restField}
                          name={[name, 'iterationId']}
                          hidden={true}
                        >
                          <Input hidden={true} />
                        </Item>
                        <Item
                          {...restField}
                          style={{ width: '100%', marginBottom: '10px' }}
                          name={[name, 'name']}
                          rules={[{ required: true }]}
                        >
                          <Input
                            showCount
                            maxLength={128}
                            placeholder="Iteration Name"
                          />
                        </Item>
                      </Flex>
                      <Flex gap="small">
                        <Item
                          {...restField}
                          style={{ margin: '0', width: '35%' }}
                          name={[name, 'typeId']}
                          rules={[{ required: true }]}
                        >
                          <Select
                            placeholder="Select Type"
                            options={iterationTypesOptions}
                          />
                        </Item>
                        <Item
                          {...restField}
                          style={{ margin: '0' }}
                          name={[name, 'start']}
                          rules={[{ required: true, message: 'Missing start' }]}
                        >
                          <DatePicker />
                        </Item>
                        <Item
                          {...restField}
                          style={{ margin: '0' }}
                          name={[name, 'end']}
                          rules={[{ required: true, message: 'Missing end' }]}
                        >
                          <DatePicker />
                        </Item>
                      </Flex>
                    </Flex>
                    <MinusCircleOutlined
                      onClick={() => remove(name)}
                      title="Remove Iteration"
                    />
                  </Flex>
                  <Divider dashed />
                </div>
              ))}
              <Item>
                <Button
                  type="dashed"
                  onClick={() => add()}
                  block
                  icon={<PlusOutlined />}
                >
                  Add Iteration
                </Button>
              </Item>
            </>
          )}
        </List>
      </Form>
    </Modal>
  )
}

export default ManagePlanningIntervalDatesForm
