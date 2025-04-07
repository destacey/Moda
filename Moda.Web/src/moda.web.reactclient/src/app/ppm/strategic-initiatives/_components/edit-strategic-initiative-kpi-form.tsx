'use client'

import { createTypedFormItem } from '@/src/components/common/forms/utils'
import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  StrategicInitiativeKpiDetailsDto,
  UpdateStrategicInitiativeKpiRequest,
} from '@/src/services/moda-api'
import {
  useGetStrategicInitiativeKpiQuery,
  useGetStrategicInitiativeKpiTargetDirectionOptionsQuery,
  useGetStrategicInitiativeKpiUnitOptionsQuery,
  useUpdateStrategicInitiativeKpiMutation,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { toFormErrors } from '@/src/utils'
import { Form, InputNumber, Modal, Select } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface EditStrategicInitiativeKpiFormProps {
  strategicInitiativeId: string
  kpiId: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditStrategicInitiativeKpiFormValues {
  name: string
  description: string
  targetValue: number
  unitId: number
  targetDirectionId: number
}

const TypedFormItem =
  createTypedFormItem<EditStrategicInitiativeKpiFormValues>()

const mapToRequestValues = (
  values: EditStrategicInitiativeKpiFormValues,
  strategicInitiativeId: string,
  kpiId: string,
): UpdateStrategicInitiativeKpiRequest => {
  return {
    strategicInitiativeId: strategicInitiativeId,
    kpiId: kpiId,
    name: values.name,
    description: values.description,
    targetValue: values.targetValue,
    unitId: values.unitId,
    targetDirectionId: values.targetDirectionId,
  }
}

const EditStrategicInitiativeKpiForm = (
  props: EditStrategicInitiativeKpiFormProps,
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
  const [form] = Form.useForm<EditStrategicInitiativeKpiFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canUpdateKpis = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )

  const {
    data: kpiData,
    isLoading: kpiIsLoading,
    error: kpiError,
  } = useGetStrategicInitiativeKpiQuery({ strategicInitiativeId, kpiId })

  const [updateKpi, { error: mutationError }] =
    useUpdateStrategicInitiativeKpiMutation()

  const {
    data: unitData,
    isLoading: unitsIsLoading,
    error: unitsError,
  } = useGetStrategicInitiativeKpiUnitOptionsQuery()

  const {
    data: targetDirectionData,
    isLoading: targetDirectionIsLoading,
    error: targetDirectionError,
  } = useGetStrategicInitiativeKpiTargetDirectionOptionsQuery()

  const mapToFormValues = useCallback(
    (kpi: StrategicInitiativeKpiDetailsDto) => {
      if (!kpi) {
        throw new Error('KPI not found')
      }
      form.setFieldsValue({
        name: kpi.name,
        description: kpi.description,
        targetValue: kpi.targetValue,
        unitId: kpi.unit.id,
        targetDirectionId: kpi.targetDirection.id,
      })
    },
    [form],
  )

  const formAction = async (
    values: EditStrategicInitiativeKpiFormValues,
    strategicInitiativeId: string,
    kpiId: string,
  ) => {
    try {
      const request = mapToRequestValues(values, strategicInitiativeId, kpiId)
      const response = await updateKpi(request)

      if (response.error) {
        throw response.error
      }

      messageApi.success(`KPI updated successfully.`)

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the KPI. Please try again.',
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
        'An error occurred while updating the KPI. Please try again.',
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
      if (showForm) {
        mapToFormValues(kpiData)
      }
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to update KPIs.')
    }
  }, [
    canUpdateKpis,
    kpiData,
    mapToFormValues,
    messageApi,
    onFormCancel,
    showForm,
  ])

  useEffect(() => {
    if (kpiError || unitsError || targetDirectionError) {
      console.error(kpiError || unitsError || targetDirectionError)
      messageApi.error(
        kpiError ||
          unitsError.detail ||
          targetDirectionError.detail ||
          'An error occurred while loading form data.',
      )
      onFormCancel()
    }
  }, [kpiError, messageApi, onFormCancel, targetDirectionError, unitsError])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Edit Strategic Initiative KPI"
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
          name="edit-strategic-initiative-kpi-form"
        >
          <TypedFormItem
            name="name"
            label="Name"
            rules={[
              { required: true, message: 'Name is required' },
              { max: 64 },
            ]}
          >
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={64}
            />
          </TypedFormItem>
          <TypedFormItem
            name="description"
            label="Description"
            rules={[
              { required: true, message: 'Description is required' },
              { max: 512 },
            ]}
          >
            <MarkdownEditor maxLength={512} />
          </TypedFormItem>
          <TypedFormItem
            name="targetValue"
            label="Target Value"
            rules={[{ required: true, message: 'Target Value is required' }]}
          >
            <InputNumber style={{ width: 200 }} />
          </TypedFormItem>
          <TypedFormItem
            name="unitId"
            label="Unit"
            rules={[{ required: true, message: 'Unit is required' }]}
          >
            <Select
              allowClear
              options={unitData ?? []}
              placeholder="Select Unit"
            />
          </TypedFormItem>
          <TypedFormItem
            name="targetDirectionId"
            label="Target Direction"
            rules={[
              { required: true, message: 'Target Direction is required' },
            ]}
          >
            <Select
              allowClear
              options={targetDirectionData ?? []}
              placeholder="Select Target Direction"
            />
          </TypedFormItem>
        </Form>
      </Modal>
    </>
  )
}

export default EditStrategicInitiativeKpiForm
