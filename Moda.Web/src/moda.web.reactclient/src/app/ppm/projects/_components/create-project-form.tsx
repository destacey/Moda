'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import useAuth from '@/src/components/contexts/auth'
import { CreateProjectRequest } from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useGetExpenditureCategoryOptionsQuery } from '@/src/store/features/ppm/expenditure-categories-api'
import { useGetPortfolioOptionsQuery } from '@/src/store/features/ppm/portfolios-api'
import { useCreateProjectMutation } from '@/src/store/features/ppm/projects-api'
import { useGetStrategicThemeOptionsQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Modal, Select } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface CreateProjectFormProps {
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface CreateProjectFormValues {
  portfolioId: string
  name: string
  description: string
  expenditureCategoryId: number
  start?: Date
  end?: Date
  sponsorIds: string[]
  ownerIds: string[]
  managerIds: string[]
  strategicThemeIds: string[]
}

const mapToRequestValues = (
  values: CreateProjectFormValues,
): CreateProjectRequest => {
  return {
    name: values.name,
    description: values.description,
    expenditureCategoryId: values.expenditureCategoryId,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    portfolioId: values.portfolioId,
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
    managerIds: values.managerIds,
    strategicThemeIds: values.strategicThemeIds,
  } as CreateProjectRequest
}

const CreateProjectForm = (props: CreateProjectFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateProjectFormValues>()
  const formValues = Form.useWatch([], form)

  const [createProject, { error: mutationError }] = useCreateProjectMutation()

  const {
    data: expenditureData,
    isLoading: expenditureOptionsIsLoading,
    error: expenditureOptionsError,
  } = useGetExpenditureCategoryOptionsQuery(false)

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
  const canCreateProject = hasPermissionClaim('Permissions.Projects.Create')

  useEffect(() => {
    if (
      expenditureOptionsError ||
      portfolioOptionsError ||
      employeeOptionsError ||
      strategicThemeOptionsError
    ) {
      console.error(
        expenditureOptionsError ??
          portfolioOptionsError ??
          employeeOptionsError ??
          strategicThemeOptionsError,
      )
      props.messageApi.error(
        expenditureOptionsError.detail ??
          portfolioOptionsError.detail ??
          employeeOptionsError.detail ??
          strategicThemeOptionsError.detail ??
          'An error occurred while loading form data. Please try again.',
      )
    }
  }, [
    employeeOptionsError,
    expenditureOptionsError,
    portfolioOptionsError,
    props.messageApi,
    strategicThemeOptionsError,
  ])

  const create = async (values: CreateProjectFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await createProject(request)
      if (response.error) {
        throw response.error
      }
      props.messageApi.success(
        'Project created successfully. Project key: ' + response.data.key,
      )

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.detail ??
            'An error occurred while creating the project. Please try again.',
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
      props.messageApi.error(
        'An error occurred while creating the project. Please try again.',
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
    if (canCreateProject) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      props.messageApi.error('You do not have permission to create projects.')
    }
  }, [canCreateProject, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Create Project"
        open={isOpen}
        width={'60vw'}
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
          name="create-project-form"
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
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.label?.toLowerCase() ?? '').includes(
                  input.toLowerCase(),
                )
              }
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
          <Item
            name="expenditureCategoryId"
            label="Expenditure Category"
            rules={[
              { required: true, message: 'Expenditure Category is required' },
            ]}
          >
            <Select
              allowClear
              options={expenditureData ?? []}
              placeholder="Select Expenditure Category"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.label?.toLowerCase() ?? '').includes(
                  input.toLowerCase(),
                )
              }
            />
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
              optionFilterProp="children"
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

export default CreateProjectForm
