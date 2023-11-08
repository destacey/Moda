'use client'

import { CreateHealthCheckRequest } from '@/src/services/moda-api'
import { DatePicker, Form, FormProps, Input, Modal, Radio, message } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import { toFormErrors } from '@/src/utils'
import {
  useCreateHealthCheckMutation,
  useGetHealthStatusOptions,
} from '@/src/services/queries/health-check-queries'
import dayjs from 'dayjs'
import { SystemContext } from '@/src/app/components/constants'
import { useAppSelector } from '@/src/app/hooks'
import withModalForm from '../../hoc/withModalForm'
import {
  createHealthCheck,
  selectHealthCheckEditContext,
  setEditMode,
} from '@/src/store/health-check-slice'

export interface CreateHealthCheckFormProps {
  showForm: boolean
  objectId: string
  context: SystemContext
  onFormCreate: () => void
  onFormCancel: () => void
}

export interface CreateHealthCheckFormValues {
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

const CreateHealthCheckForm = ({
  form,
}: FormProps<CreateHealthCheckFormValues>) => {
  const { hasClaim } = useAuth()
  const canCreateHealthChecks = hasClaim(
    'Permission',
    'Permissions.HealthChecks.Create',
  )

  const { data: statusOptions } = useGetHealthStatusOptions()

  return (
    <Form
      form={form}
      size="small"
      layout="vertical"
      name="create-health-check-form"
      initialValues={{
        expiration: dayjs().add(2, 'week').endOf('day'),
      }}
    >
      <Form.Item name="statusId" label="Status" rules={[{ required: true }]}>
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
  )
}

export const ModalCreateHealthCheckForm = withModalForm(CreateHealthCheckForm, {
  title: 'Create Health Check',
  okText: 'Create',
  useFormState: () => useAppSelector(selectHealthCheckEditContext),
  onOk: (values: CreateHealthCheckFormValues) => createHealthCheck(values),
  onCancel: setEditMode(false),
})

export default ModalCreateHealthCheckForm
