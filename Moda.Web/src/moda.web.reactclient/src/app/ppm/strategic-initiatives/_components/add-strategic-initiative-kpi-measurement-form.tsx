'use client'

import { createTypedFormItem } from '@/src/components/common/forms/utils'
import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { AddStrategicInitiativeKpiMeasurementRequest } from '@/src/services/moda-api'
import {
  useAddStrategicInitiativeKpiMeasurementMutation,
  useGetStrategicInitiativeKpiQuery,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Descriptions, Form, InputNumber, Modal } from 'antd'
import dayjs from 'dayjs'
import { useCallback, useEffect, useState } from 'react'

const { Item: DescriptionItem } = Descriptions

export interface AddStrategicInitiativeKpiMeasurementFormProps {
  strategicInitiativeId: string
  kpiId: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: any
}

interface AddStrategicInitiativeKpiMeasurementFormValues {
  actualValue: number
  measurementDate: Date
  note: string
}

const TypedFormItem =
  createTypedFormItem<AddStrategicInitiativeKpiMeasurementFormValues>()

const mapToRequestValues = (
  values: AddStrategicInitiativeKpiMeasurementFormValues,
  strategicInitiativeId: string,
  kpiId: string,
): AddStrategicInitiativeKpiMeasurementRequest => {
  return {
    strategicInitiativeId: strategicInitiativeId,
    kpiId: kpiId,
    actualValue: values.actualValue,
    measurementDate: values.measurementDate,
    note: values.note,
  }
}

const AddStrategicInitiativeKpiMeasurementForm = (
  props: AddStrategicInitiativeKpiMeasurementFormProps,
) => {
  const {
    strategicInitiativeId,
    kpiId,
    showForm,
    onFormComplete,
    onFormCancel,
    messageApi,
  } = props

  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<AddStrategicInitiativeKpiMeasurementFormValues>()
  const formValues = Form.useWatch([], form)

  const { hasPermissionClaim } = useAuth()
  const canUpdateKpis = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )

  const {
    data: kpiData,
    isLoading: kpiIsLoading,
    error: kpiError,
  } = useGetStrategicInitiativeKpiQuery({ strategicInitiativeId, kpiId })

  const [addKpiMeasurement, { error: mutationError }] =
    useAddStrategicInitiativeKpiMeasurementMutation()

  const formAction = async (
    values: AddStrategicInitiativeKpiMeasurementFormValues,
    strategicInitiativeId: string,
    kpiId: string,
  ) => {
    try {
      const request = mapToRequestValues(values, strategicInitiativeId, kpiId)
      const response = await addKpiMeasurement(request)

      if (response.error) {
        throw response.error
      }

      messageApi.success(`KPI measurement added successfully.`)

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while adding a measurement to the KPI. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await formAction(values, strategicInitiativeId, kpiId)) {
        setIsOpen(false)
        form.resetFields()
        onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while adding a measurement to the KPI. Please try again.',
      )
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
    if (kpiError) {
      console.error(kpiError)
      messageApi.error(kpiError || 'An error occurred while loading form data.')
      onFormCancel()
    }
  }, [kpiError, messageApi, onFormCancel])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Add KPI Measurement"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Add"
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
          name="add-strategic-initiative-kpi-measurement-form"
        >
          <Descriptions size="small" column={1}>
            <DescriptionItem label="KPI">{`${kpiData?.key} - ${kpiData?.name}`}</DescriptionItem>
          </Descriptions>
          <br />
          <TypedFormItem
            name="actualValue"
            label="Actual Value"
            rules={[{ required: true, message: 'Actual Value is required' }]}
          >
            <InputNumber style={{ width: 200 }} />
          </TypedFormItem>
          <TypedFormItem
            name="measurementDate"
            label="Measurement Date"
            rules={[
              { required: true },
              {
                validator: (_, value) =>
                  value && dayjs() >= value
                    ? Promise.resolve()
                    : Promise.reject(
                        new Error('The Measurement Date must be in the Past.'),
                      ),
              },
            ]}
          >
            <DatePicker
              showTime
              format="YYYY-MM-DD h:mm A"
              disabledDate={(value) => value && value > dayjs()}
            />
          </TypedFormItem>
          <TypedFormItem name="note" label="Note" rules={[{ max: 1024 }]}>
            <MarkdownEditor maxLength={1024} />
          </TypedFormItem>
        </Form>
      </Modal>
    </>
  )
}

export default AddStrategicInitiativeKpiMeasurementForm
