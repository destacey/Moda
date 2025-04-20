'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import {
  BackgroundJobTypeDto,
  CreateRecurringJobRequest,
} from '@/src/services/moda-api'
import { useCreateRecurringJobMutation } from '@/src/store/features/admin/background-jobs-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, Select } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface CreateRecurringJobFormProps {
  showForm: boolean
  jobTypes: BackgroundJobTypeDto[]
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateRecurringJobFormValues {
  jobId: string
  jobTypeId: number
  cronExpression: string
}

const mapToRequestValues = (values: CreateRecurringJobRequest) => {
  const request = {
    jobId: values.jobId,
    jobTypeId: values.jobTypeId,
    cronExpression: values.cronExpression,
  } as CreateRecurringJobRequest
  return request
}

const CreateRecurringJobForm = (props: CreateRecurringJobFormProps) => {
  const [isOpen, setIsOpen] = useState(props.showForm)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateRecurringJobFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const [createRecurringJob, { error: mutationError }] =
    useCreateRecurringJobMutation()

  const create = async (values: CreateRecurringJobFormValues) => {
    setIsSaving(true)
    try {
      const request = mapToRequestValues(values)
      const response = await createRecurringJob(request)
      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        console.error('Mutation error:', mutationError)
        messageApi.error(
          error.detail ??
            'An unexpected error occurred while creating a recurring job.',
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

        messageApi.success('Successfully created recurring job.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    props.onFormCancel()
    form.resetFields()
  }, [form, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <Modal
      title="Create Recurring Job"
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
        name="create-recurring-job-form"
      >
        <Item label="Job Id" name="jobId" rules={[{ required: true }]}>
          <Input />
        </Item>
        <Item name="jobTypeId" label="Job Type" rules={[{ required: true }]}>
          <Select
            showSearch
            placeholder="Select a Job Type"
            optionFilterProp="children"
            filterOption={(input, option) =>
              (option?.label.toLowerCase() ?? '').includes(input.toLowerCase())
            }
            filterSort={(optionA, optionB) =>
              (optionA?.label ?? '')
                .toLowerCase()
                .localeCompare((optionB?.label ?? '').toLowerCase())
            }
            options={props.jobTypes.map((jobType) => ({
              value: jobType.id,
              label: jobType.name,
            }))}
          />
        </Item>
        <Item
          label="Cron Expression"
          name="cronExpression"
          rules={[{ required: true }]}
        >
          <Input placeholder="Example for Every 5 minutes: */5 * * * *" />
        </Item>
      </Form>
    </Modal>
  )
}

export default CreateRecurringJobForm
