'use client'

import { createTypedFormItem } from '@/src/components/common/forms/utils'
import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { CreateStrategicInitiativeKpiRequest } from '@/src/services/moda-api'
import {
  useCreateStrategicInitiativeKpiMutation,
  useGetStrategicInitiativeKpiTargetDirectionOptionsQuery,
  useGetStrategicInitiativeKpiUnitOptionsQuery,
} from '@/src/store/features/ppm/strategic-initiatives-api'
import { toFormErrors } from '@/src/utils'
import { Form, InputNumber, Modal, Select } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect, useState } from 'react'

export interface CreateStrategicInitiativeKpiFormProps {
  strategicInitiativeId: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: any
}

interface CreateStrategicInitiativeKpiFormValues {
  name: string
  description: string
  targetValue: number
  unitId: number
  targetDirectionId: number
}

const TypedFormItem =
  createTypedFormItem<CreateStrategicInitiativeKpiFormValues>()

const mapToRequestValues = (
  values: CreateStrategicInitiativeKpiFormValues,
  strategicInitiativeId: string,
): CreateStrategicInitiativeKpiRequest => {
  return {
    strategicInitiativeId: strategicInitiativeId,
    name: values.name,
    description: values.description,
    targetValue: values.targetValue,
    unitId: values.unitId,
    targetDirectionId: values.targetDirectionId,
  }
}

const CreateStrategicInitiativeKpiForm = (
  props: CreateStrategicInitiativeKpiFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateStrategicInitiativeKpiFormValues>()
  const formValues = Form.useWatch([], form)

  const {
    strategicInitiativeId,
    showForm,
    onFormComplete,
    onFormCancel,
    messageApi,
  } = props

  const { hasPermissionClaim } = useAuth()
  const canCreateKpis = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )

  const [createKpi, { error: mutationError }] =
    useCreateStrategicInitiativeKpiMutation()

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

  const formAction = async (
    values: CreateStrategicInitiativeKpiFormValues,
    strategicInitiativeId: string,
  ) => {
    try {
      const request = mapToRequestValues(values, strategicInitiativeId)
      const response = await createKpi(request)
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        'KPI created successfully. KPI key: ' + response.data.key,
      )

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while creating the KPI. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await formAction(values, strategicInitiativeId)) {
        setIsOpen(false)
        form.resetFields()
        onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while creating the KPI. Please try again.',
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
    if (canCreateKpis) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to create KPIs.')
    }
  }, [canCreateKpis, messageApi, onFormCancel, showForm])

  useEffect(() => {
    if (unitsError || targetDirectionError) {
      console.error(unitsError || targetDirectionError)
      messageApi.error(
        unitsError.detail ||
          targetDirectionError.detail ||
          'An error occurred while loading form data.',
      )
      onFormCancel()
    }
  }, [messageApi, onFormCancel, targetDirectionError, unitsError])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Create Strategic Initiative KPI"
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
          name="create-strategic-initiative-kpi-form"
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

export default CreateStrategicInitiativeKpiForm
