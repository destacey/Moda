'use client'

import { CreateHealthCheckRequest } from '@/src/services/moda-api'
import { DatePicker, Form, FormProps, Radio } from 'antd'
import useAuth from '../../contexts/auth'
import dayjs from 'dayjs'
import { SystemContext } from '@/src/app/components/constants'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import withModalForm from '../../hoc/withModalForm'
import {
  createHealthCheck,
  selectHealthCheckEditContext,
  cancelHealthCheckCreate,
  getHealthCheckStatusOptions,
} from '@/src/store/features/health-check-slice'
import { useEffect } from 'react'
import { MarkdownEditor } from '../markdown'

const { Item } = Form
const { Group: RadioGroup } = Radio

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
  const dispatch = useAppDispatch()

  const statusOptions = useAppSelector(
    (state) => state.healthCheck.statusOptions,
  )

  useEffect(() => {
    dispatch(getHealthCheckStatusOptions())
  }, [dispatch])

  if (!canCreateHealthChecks) return null

  return (
    <>
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="create-health-check-form"
        initialValues={{
          expiration: dayjs().add(2, 'week').endOf('day'),
        }}
      >
        <Item name="statusId" label="Status" rules={[{ required: true }]}>
          <RadioGroup
            options={statusOptions}
            optionType="button"
            buttonStyle="solid"
          />
        </Item>
        <Item name="note" label="Note" initialValue="" rules={[{ max: 1024 }]}>
          <MarkdownEditor
            value={form.getFieldValue('note')}
            onChange={(value) => form.setFieldValue('note', value || '')}
            maxLength={1024}
          />
        </Item>
        <Item
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
        </Item>
      </Form>
    </>
  )
}

export const ModalCreateHealthCheckForm = withModalForm(CreateHealthCheckForm, {
  title: 'Create Health Check',
  okText: 'Create',
  useFormState: () => useAppSelector(selectHealthCheckEditContext),
  onOk: (values: CreateHealthCheckFormValues) => createHealthCheck(values),
  onCancel: cancelHealthCheckCreate(),
})

export default ModalCreateHealthCheckForm
