'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  KpiCheckpointPlanItemRequest,
  KpiTargetDirection,
  ManageStrategicInitiativeKpiCheckpointPlanRequest,
} from '@/src/services/moda-api'
import {
  useGetStrategicInitiativeKpiCheckpointsQuery,
  useGetStrategicInitiativeKpiQuery,
  useManageStrategicInitiativeKpiCheckpointPlanMutation,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { toFormErrors } from '@/src/utils'
import { MinusCircleOutlined, PlusOutlined } from '@ant-design/icons'
import {
  Button,
  DatePicker,
  Descriptions,
  Divider,
  Flex,
  Form,
  Input,
  InputNumber,
  InputRef,
  Modal,
} from 'antd'
import dayjs from 'dayjs'
import { useCallback, useEffect, useRef, useState } from 'react'

type InputNumberRef = { focus: () => void } | null

const { Item } = Form
const { Item: DescriptionItem } = Descriptions

export interface ManageStrategicInitiativeKpiCheckpointPlanFormProps {
  strategicInitiativeId: string
  kpiId: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CheckpointPlanItemFormValues {
  checkpointId?: string
  targetValue: number
  atRiskValue?: number
  checkpointDate: Date
  dateLabel: string
}

interface ManageKpiCheckpointPlanFormValues {
  checkpoints: CheckpointPlanItemFormValues[]
}

const mapToRequestValues = (
  values: ManageKpiCheckpointPlanFormValues,
  strategicInitiativeId: string,
  kpiId: string,
): ManageStrategicInitiativeKpiCheckpointPlanRequest => {
  return {
    strategicInitiativeId,
    kpiId,
    checkpoints: values.checkpoints.map(
      (c): KpiCheckpointPlanItemRequest => ({
        checkpointId: c.checkpointId,
        targetValue: c.targetValue,
        atRiskValue: c.atRiskValue ?? undefined,
        checkpointDate: (c.checkpointDate as any)?.toISOString(),
        dateLabel: c.dateLabel,
      }),
    ),
  }
}

const ManageStrategicInitiativeKpiCheckpointPlanForm = (
  props: ManageStrategicInitiativeKpiCheckpointPlanFormProps,
) => {
  const {
    strategicInitiativeId,
    kpiId,
    showForm,
    onFormComplete,
    onFormCancel,
  } = props

  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)

  const [form] = Form.useForm<ManageKpiCheckpointPlanFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()
  const newRowDateLabelRef = useRef<InputRef>(null)
  const targetValueRefs = useRef<InputNumberRef[]>([])

  const { hasPermissionClaim } = useAuth()
  const canUpdateKpis = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )

  const { data: kpiData } = useGetStrategicInitiativeKpiQuery({
    strategicInitiativeId,
    kpiId,
  })

  const unit = kpiData?.unit as unknown as string
  const targetValuePrefix = unit === 'USD' ? '$' : undefined
  const targetValueSuffix = unit === 'Percentage' ? '%' : undefined

  const { data: checkpointPlanData } =
    useGetStrategicInitiativeKpiCheckpointsQuery(
      { strategicInitiativeId, kpiId },
      { skip: !isOpen },
    )

  const [manageCheckpointPlan] =
    useManageStrategicInitiativeKpiCheckpointPlanMutation()

  const mapToFormValues = useCallback(() => {
    form.setFieldsValue({
      checkpoints: (checkpointPlanData ?? []).map((c) => ({
        checkpointId: c.id,
        targetValue: c.targetValue,
        atRiskValue: c.atRiskValue ?? undefined,
        checkpointDate: dayjs(c.checkpointDate),
        dateLabel: c.dateLabel,
      })),
    })
  }, [form, checkpointPlanData])

  const formAction = async (
    values: ManageKpiCheckpointPlanFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values, strategicInitiativeId, kpiId)
      const response = await manageCheckpointPlan(request)

      if (response.error) {
        throw response.error
      }

      messageApi.success('KPI checkpoint plan updated successfully.')
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the KPI checkpoint plan. Please try again.',
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
      if (await formAction(values)) {
        setIsOpen(false)
        form.resetFields()
        onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    form.resetFields()
    onFormCancel()
  }, [form, onFormCancel])

  useEffect(() => {
    if (!kpiData) return
    if (canUpdateKpis) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to update KPIs.')
    }
  }, [canUpdateKpis, kpiData, messageApi, onFormCancel, showForm])

  useEffect(() => {
    if (isOpen && checkpointPlanData !== undefined) {
      mapToFormValues()
    }
  }, [isOpen, checkpointPlanData, mapToFormValues])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <Modal
      title="Manage KPI Checkpoint Plan"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden={true}
      width={750}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="manage-strategic-initiative-kpi-checkpoint-plan-form"
      >
        <Descriptions size="small" column={1}>
          <DescriptionItem label="KPI">{`${kpiData?.key} - ${kpiData?.name}`}</DescriptionItem>
          <DescriptionItem label="Target Value">
            {kpiData?.targetValue}
          </DescriptionItem>
          <DescriptionItem label="Unit">
            {kpiData?.unit as unknown as string}
          </DescriptionItem>
          <DescriptionItem label="Target Direction">
            {kpiData?.targetDirection as unknown as string}
          </DescriptionItem>
        </Descriptions>
        <br />
        <Divider titlePlacement="start">Checkpoints</Divider>
        <Form.List name="checkpoints">
          {(fields, { add, remove }) => (
            <>
              {fields.map(({ key, name, ...restField }, index) => (
                <div key={key}>
                  <Flex align="center" justify="space-between">
                    <Flex vertical style={{ width: '90%' }}>
                      <Item
                        {...restField}
                        name={[name, 'checkpointId']}
                        hidden={true}
                      >
                        <Input hidden={true} />
                      </Item>
                      <Flex gap="small" wrap>
                        <Item
                          {...restField}
                          style={{ margin: '0' }}
                          name={[name, 'dateLabel']}
                          rules={[
                            {
                              required: true,
                              message: 'Date label is required',
                            },
                            { max: 16, message: 'Max 16 characters' },
                          ]}
                        >
                          <Input
                            showCount
                            maxLength={16}
                            placeholder="Date Label"
                            style={{ width: 150 }}
                            ref={
                              index === fields.length - 1
                                ? newRowDateLabelRef
                                : null
                            }
                          />
                        </Item>
                        <Item
                          {...restField}
                          style={{ margin: '0' }}
                          name={[name, 'checkpointDate']}
                          rules={[
                            {
                              required: true,
                              message: 'Checkpoint date is required',
                            },
                          ]}
                        >
                          <DatePicker
                            showTime
                            format="YYYY-MM-DD h:mm A"
                            placeholder="Checkpoint Date"
                            onOk={() =>
                              setTimeout(
                                () => targetValueRefs.current[index]?.focus(),
                                100,
                              )
                            }
                          />
                        </Item>
                        <Item
                          {...restField}
                          style={{ margin: '0' }}
                          name={[name, 'targetValue']}
                          rules={[
                            {
                              required: true,
                              message: 'Target value is required',
                            },
                          ]}
                        >
                          <InputNumber
                            ref={(el) => {
                              targetValueRefs.current[index] = el
                            }}
                            placeholder="Target Value"
                            prefix={targetValuePrefix}
                            suffix={targetValueSuffix}
                            style={{ width: 125 }}
                          />
                        </Item>
                        <Item
                          {...restField}
                          style={{ margin: '0' }}
                          name={[name, 'atRiskValue']}
                          rules={[
                            {
                              validator: (_, atRiskValue) => {
                                if (atRiskValue == null)
                                  return Promise.resolve()
                                const targetValue = form.getFieldValue([
                                  'checkpoints',
                                  name,
                                  'targetValue',
                                ]) as number | undefined
                                if (targetValue == null)
                                  return Promise.resolve()
                                if (atRiskValue === targetValue)
                                  return Promise.reject(
                                    'At risk value must differ from target value.',
                                  )
                                const direction =
                                  kpiData?.targetDirection as unknown as string
                                if (
                                  direction === KpiTargetDirection.Increase &&
                                  atRiskValue >= targetValue
                                )
                                  return Promise.reject(
                                    'At risk value must be less than target value when direction is Increase.',
                                  )
                                if (
                                  direction === KpiTargetDirection.Decrease &&
                                  atRiskValue <= targetValue
                                )
                                  return Promise.reject(
                                    'At risk value must be greater than target value when direction is Decrease.',
                                  )
                                return Promise.resolve()
                              },
                            },
                          ]}
                        >
                          <InputNumber
                            placeholder="At Risk Value"
                            prefix={targetValuePrefix}
                            suffix={targetValueSuffix}
                            style={{ width: 130 }}
                          />
                        </Item>
                      </Flex>
                    </Flex>
                    <MinusCircleOutlined
                      onClick={() => remove(name)}
                      title="Remove Checkpoint"
                    />
                  </Flex>
                  <Divider dashed />
                </div>
              ))}
              <Item>
                <Button
                  type="dashed"
                  onClick={() => {
                    add()
                    setTimeout(() => newRowDateLabelRef.current?.focus(), 0)
                  }}
                  block
                  icon={<PlusOutlined />}
                >
                  Add Checkpoint
                </Button>
              </Item>
            </>
          )}
        </Form.List>
      </Form>
    </Modal>
  )
}

export default ManageStrategicInitiativeKpiCheckpointPlanForm
