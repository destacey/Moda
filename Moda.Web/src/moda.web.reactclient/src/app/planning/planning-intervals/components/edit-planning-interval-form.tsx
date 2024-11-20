'use client'

import { Form, Input, Modal, Switch, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../components/contexts/auth'
import {
  PlanningIntervalDetailsDto,
  UpdatePlanningIntervalRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import {
  useGetPlanningInterval,
  useUpdatePlanningIntervalMutation,
} from '@/src/services/queries/planning-queries'

const { Item } = Form
const { TextArea } = Input

export interface EditPlanningIntervalFormProps {
  showForm: boolean
  id: string
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
  id,
  onFormUpdate,
  onFormCancel,
}: EditPlanningIntervalFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditPlanningIntervalFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { data: planningIntervalData } = useGetPlanningInterval(id)
  const updatePlanningInterval = useUpdatePlanningIntervalMutation()

  const { hasClaim } = useAuth()
  const canUpdatePlanningInterval = hasClaim(
    'Permission',
    'Permissions.PlanningIntervals.Update',
  )
  const mapToFormValues = useCallback(
    (planningInterval: PlanningIntervalDetailsDto) => {
      form.setFieldsValue({
        id: planningInterval.id,
        name: planningInterval.name,
        description: planningInterval.description,
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
      await updatePlanningInterval.mutateAsync(request)
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
        messageApi.success('Successfully updated planning interval.')
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
    <>
      {contextHolder}
      <Modal
        title="Edit Planning Interval"
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
          <Item name="description" label="Description" extra="Markdown enabled">
            <TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
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
    </>
  )
}

export default EditPlanningIntervalForm
