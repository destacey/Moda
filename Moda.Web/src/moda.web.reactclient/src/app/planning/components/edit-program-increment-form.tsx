'use client'

import { DatePicker, Form, Input, Modal, Switch, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../components/contexts/auth'
import {
  ProgramIncrementDetailsDto,
  UpdateProgramIncrementRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import {
  useGetProgramIncrementById,
  useUpdateProgramIncrementMutation,
} from '@/src/services/queries/planning-queries'

export interface EditProgramIncrementFormProps {
  showForm: boolean
  id: string
  onFormUpdate: () => void
  onFormCancel: () => void
}

interface EditProgramIncrementFormValues {
  id: string
  name: string
  description?: string
  start: Date
  end: Date
  objectivesLocked: boolean
}

const mapToRequestValues = (values: EditProgramIncrementFormValues) => {
  return {
    id: values.id,
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    objectivesLocked: values.objectivesLocked,
  } as UpdateProgramIncrementRequest
}

const EditProgramIncrementForm = ({
  showForm,
  id,
  onFormUpdate,
  onFormCancel,
}: EditProgramIncrementFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditProgramIncrementFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { data: programIncrementData } = useGetProgramIncrementById(id)
  const updateProgramIncrement = useUpdateProgramIncrementMutation()

  const { hasClaim } = useAuth()
  const canUpdateProgramIncrement = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Update',
  )
  const mapToFormValues = useCallback(
    (programIncrement: ProgramIncrementDetailsDto) => {
      form.setFieldsValue({
        id: programIncrement.id,
        name: programIncrement.name,
        description: programIncrement.description,
        start: dayjs(programIncrement.start),
        end: dayjs(programIncrement.end),
        objectivesLocked: programIncrement.objectivesLocked,
      })
    },
    [form],
  )

  const update = async (
    values: EditProgramIncrementFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      await updateProgramIncrement.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An error occurred while updating the program increment.',
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
        messageApi.success('Successfully updated program increment.')
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
      mapToFormValues(programIncrementData)
      setIsValid(true)
    } catch (error) {
      handleCancel()
      messageApi.error('An unexpected error occurred while loading form data.')
      console.error(error)
    }
  }, [handleCancel, mapToFormValues, messageApi, programIncrementData])

  useEffect(() => {
    if (!programIncrementData) return
    if (canUpdateProgramIncrement) {
      setIsOpen(showForm)
      if (showForm) {
        loadData()
      }
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to edit program increments.')
    }
  }, [
    canUpdateProgramIncrement,
    loadData,
    messageApi,
    onFormCancel,
    programIncrementData,
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
        title="Edit Program Increment"
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
          name="edit-program-increment-form"
        >
          <Form.Item name="id" hidden={true}>
            <Input />
          </Form.Item>
          <Form.Item label="Name" name="name" rules={[{ required: true }]}>
            <Input.TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Form.Item>
          <Form.Item
            name="description"
            label="Description"
            help="Markdown enabled"
          >
            <Input.TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Form.Item>
          <Form.Item label="Start" name="start" rules={[{ required: true }]}>
            <DatePicker />
          </Form.Item>
          <Form.Item label="End" name="end" rules={[{ required: true }]}>
            <DatePicker />
          </Form.Item>
          <Form.Item
            label="Objectives Locked?"
            name="objectivesLocked"
            valuePropName="checked"
          >
            <Switch checkedChildren="Yes" unCheckedChildren="No" />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditProgramIncrementForm
