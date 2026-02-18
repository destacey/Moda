'use client'

import { Form, Input, Modal, Switch } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../../components/contexts/auth'
import {
  PlanningIntervalDetailsDto,
  UpdatePlanningIntervalRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import { MarkdownEditor } from '@/src/components/common/markdown'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  useGetPlanningIntervalQuery,
  useUpdatePlanningIntervalMutation,
} from '@/src/store/features/planning/planning-interval-api'

const { Item } = Form
const { TextArea } = Input

export interface EditPlanningIntervalFormProps {
  showForm: boolean
  planningIntervalKey: number
  onFormUpdate: () => void
  onFormCancel: () => void
}

interface EditPlanningIntervalFormValues {
  id: string
  name: string
  description?: string
  objectivesLocked: boolean
}

const mapToRequestValues = (values: EditPlanningIntervalFormValues) => {
  return {
    id: values.id,
    name: values.name,
    description: values.description,
    objectivesLocked: values.objectivesLocked,
  } as UpdatePlanningIntervalRequest
}

const EditPlanningIntervalForm = ({
  showForm,
  planningIntervalKey,
  onFormUpdate,
  onFormCancel,
}: EditPlanningIntervalFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditPlanningIntervalFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const { data: planningIntervalData } =
    useGetPlanningIntervalQuery(planningIntervalKey)
  const [updatePlanningInterval, { error: mutationError }] =
    useUpdatePlanningIntervalMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdatePlanningInterval = hasPermissionClaim(
    'Permissions.PlanningIntervals.Update',
  )

  const mapToFormValues = useCallback(
    (planningInterval: PlanningIntervalDetailsDto) => {
      form.setFieldsValue({
        id: planningInterval.id,
        name: planningInterval.name,
        description: planningInterval.description || '',
        objectivesLocked: planningInterval.objectivesLocked,
      })
    },
    [form],
  )

  const update = async (
    values: EditPlanningIntervalFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      const response = await updatePlanningInterval({
        request,
        cacheKey: planningIntervalKey,
      })
      if (response.error) {
        throw response.error
      }
      messageApi.success('Planning interval updated successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An error occurred while updating the planning interval.',
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
      if (await update(values)) {
        setIsOpen(false)
        form.resetFields()
        onFormUpdate()
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
      mapToFormValues(planningIntervalData)
      setIsValid(true)
    } catch (error) {
      handleCancel()
      messageApi.error('An unexpected error occurred while loading form data.')
      console.error(error)
    }
  }, [handleCancel, mapToFormValues, messageApi, planningIntervalData])

  useEffect(() => {
    if (!planningIntervalData) return
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
      title="Edit Planning Interval"
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
        name="edit-planning-interval-form"
      >
        <Item name="id" hidden={true}>
          <Input />
        </Item>
        <Item label="Name" name="name" rules={[{ required: true }]}>
          <TextArea
            autoSize={{ minRows: 1, maxRows: 2 }}
            showCount
            maxLength={128}
          />
        </Item>
        <Item name="description" label="Description" rules={[{ max: 2048 }]}>
          <MarkdownEditor
            value={form.getFieldValue('description')}
            onChange={(value) => form.setFieldValue('description', value || '')}
            maxLength={2048}
          />
        </Item>
        <Item
          label="Objectives Locked?"
          name="objectivesLocked"
          valuePropName="checked"
        >
          <Switch checkedChildren="Yes" unCheckedChildren="No" />
        </Item>
      </Form>
    </Modal>
  )
}

export default EditPlanningIntervalForm
