'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { CreateProgramRequest } from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useGetPortfolioOptionsQuery } from '@/src/store/features/ppm/portfolios-api'
import { useCreateProgramMutation } from '@/src/store/features/ppm/programs-api'
import { useGetStrategicThemeOptionsQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Modal, Select } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface CreateProgramFormProps {
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreateProgramFormValues {
  portfolioId: string
  name: string
  description: string
  start?: Date
  end?: Date
  sponsorIds: string[]
  ownerIds: string[]
  managerIds: string[]
  strategicThemeIds: string[]
}

const mapToRequestValues = (
  values: CreateProgramFormValues,
): CreateProgramRequest => {
  return {
    name: values.name,
    description: values.description,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    portfolioId: values.portfolioId,
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
    managerIds: values.managerIds,
    strategicThemeIds: values.strategicThemeIds,
  } as CreateProgramRequest
}

const CreateProgramForm = (props: CreateProgramFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateProgramFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [createProgram, { error: mutationError }] = useCreateProgramMutation()

  const {
    data: portfolioData,
    isLoading: portfolioOptionsIsLoading,
    error: portfolioOptionsError,
  } = useGetPortfolioOptionsQuery()

  const {
    data: employeeData,
    isLoading: employeeOptionsIsLoading,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(false)

  const {
    data: strategicThemeData,
    isLoading: strategicThemeOptionsIsLoading,
    error: strategicThemeOptionsError,
  } = useGetStrategicThemeOptionsQuery(false)

  const { hasPermissionClaim } = useAuth()
  const canCreateProgram = hasPermissionClaim('Permissions.Programs.Create')

  useEffect(() => {
    if (
      portfolioOptionsError ||
      employeeOptionsError ||
      strategicThemeOptionsError
    ) {
      console.error(
        portfolioOptionsError ??
          employeeOptionsError ??
          strategicThemeOptionsError,
      )
      messageApi.error(
        portfolioOptionsError.detail ??
          employeeOptionsError.detail ??
          strategicThemeOptionsError.detail ??
          'An error occurred while loading form data. Please try again.',
      )
    }
  }, [
    employeeOptionsError,
    portfolioOptionsError,
    messageApi,
    strategicThemeOptionsError,
  ])

  const create = async (values: CreateProgramFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await createProgram(request)
      if (response.error) {
        throw response.error
      }
      messageApi.success(
        'Program created successfully. Program key: ' + response.data.key,
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
            'An error occurred while creating the program. Please try again.',
        )
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
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while creating the program. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    form.resetFields()
    props.onFormCancel()
  }, [form, props])

  useEffect(() => {
    if (canCreateProgram) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to create programs.')
    }
  }, [canCreateProgram, messageApi, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Create Program"
        open={isOpen}
        width={'60vw'}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="create-program-form"
        >
          <Item
            name="portfolioId"
            label="Portfolio"
            rules={[{ required: true, message: 'Portfolio is required' }]}
          >
            <Select
              allowClear
              options={portfolioData ?? []}
              placeholder="Select Portfolio"
            />
          </Item>
          <Item
            label="Name"
            name="name"
            rules={[
              { required: true, message: 'Name is required' },
              { max: 128 },
            ]}
          >
            <TextArea
              autoSize={{ minRows: 1, maxRows: 2 }}
              showCount
              maxLength={128}
            />
          </Item>
          <Item
            name="description"
            label="Description"
            rules={[
              { required: true, message: 'Description is required' },
              { max: 2048 },
            ]}
          >
            <MarkdownEditor maxLength={2048} />
          </Item>
          <Item name="start" label="Start">
            <DatePicker />
          </Item>
          <Item
            name="end"
            label="End"
            dependencies={['start']}
            rules={[
              ({ getFieldValue }) => ({
                validator(_, value) {
                  const start = getFieldValue('start')
                  if ((!start && !value) || (start && start <= value)) {
                    return Promise.resolve()
                  } else if ((!start && value) || (start && !value)) {
                    return Promise.reject(
                      new Error(
                        'Start and end date must be selected together or both left empty',
                      ),
                    )
                  }
                  return Promise.reject(
                    new Error('End date must be on or after start date'),
                  )
                },
              }),
            ]}
          >
            <DatePicker />
          </Item>
          <Item name="sponsorIds" label="Sponsors">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Sponsors"
            />
          </Item>
          <Item name="ownerIds" label="Owners">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Owners"
            />
          </Item>
          <Item name="managerIds" label="Managers">
            <EmployeeSelect
              employees={employeeData ?? []}
              allowMultiple={true}
              placeholder="Select Managers"
            />
          </Item>
          <Item name="strategicThemeIds" label="Strategic Themes">
            <Select
              mode="multiple"
              allowClear
              options={strategicThemeData ?? []}
              placeholder="Select Strategic Themes"
              optionFilterProp="label"
              filterOption={(input, option) =>
                (option?.label?.toLowerCase() ?? '').includes(
                  input.toLowerCase(),
                )
              }
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateProgramForm
