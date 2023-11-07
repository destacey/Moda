'use client'

import { CreateHealthCheckRequest } from '@/src/services/moda-api'
import { DatePicker, Form, Input, Modal, Radio, message } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import { toFormErrors } from '@/src/utils'
import {
  useCreateHealthCheckMutation,
  useGetHealthStatusOptions,
} from '@/src/services/queries/health-check-queries'
import dayjs from 'dayjs'
import { SystemContext } from '@/src/app/components/constants'

export interface CreateHealthCheckFormProps {
  showForm: boolean
  objectId: string
  context: SystemContext
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateHealthCheckFormValues {
  statusId: number
  expiration: Date
  note?: string | undefined
}

const mapToRequestValues = (values: CreateHealthCheckFormValues) => {
  return {
    statusId: values.statusId,
    expiration: values.expiration,
    note: values.note,
  } as CreateHealthCheckRequest
}

const datePresets = [
  { label: 'Tomorrow', value: dayjs().add(1, 'd').endOf('day') },
  { label: '1 Week', value: dayjs().add(7, 'd').endOf('day') },
  { label: '2 Weeks', value: dayjs().add(14, 'd').endOf('day') },
  { label: '1 Month', value: dayjs().add(30, 'd').endOf('day') },
]

const CreateHealthCheckForm = (props: CreateHealthCheckFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateHealthCheckFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canCreateHealthChecks = hasClaim(
    'Permission',
    'Permissions.HealthChecks.Create',
  )

  const { data: statusOptions } = useGetHealthStatusOptions()

  const createHealthCheck = useCreateHealthCheckMutation()

  const create = async (
    values: CreateHealthCheckFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      request.objectId = props.objectId
      request.contextId = props.context
      console.log(request)
      await createHealthCheck.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while creating the health check.',
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
      if (await create(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormCreate()
        messageApi.success('Successfully created health check.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
    form.resetFields()
  }

  useEffect(() => {
    if (!statusOptions) return

    if (canCreateHealthChecks) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to create links.')
    }
  }, [canCreateHealthChecks, messageApi, statusOptions, props])

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
        title="Create Health Check"
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
          name="create-health-check-form"
          initialValues={{
            expiration: dayjs().add(2, 'week').endOf('day'),
          }}
        >
          <Form.Item
            name="statusId"
            label="Status"
            rules={[{ required: true }]}
          >
            <Radio.Group
              options={statusOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </Form.Item>
          <Form.Item label="Note" name="note" help="Markdown enabled">
            <Input.TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Form.Item>
          <Form.Item
            label="Expiration"
            name="expiration"
            rules={[
              { required: true },
              {
                validator: (_, value) =>
                  value && dayjs() <= value
                    ? Promise.resolve()
                    : Promise.reject(
                        new Error('The Expiration must be in the future.'),
                      ),
              },
            ]}
          >
            <DatePicker
              presets={datePresets}
              showTime
              format="YYYY-MM-DD HH:mm"
              disabledDate={(value) => value && value < dayjs().startOf('day')}
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateHealthCheckForm
