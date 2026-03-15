'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'
import {
  CreateProjectRequest,
  ProjectLifecycleState,
} from '@/src/services/moda-api'
import {
  useGetProjectLifecycleQuery,
  useGetProjectLifecyclesQuery,
} from '@/src/store/features/ppm/project-lifecycles-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useGetExpenditureCategoryOptionsQuery } from '@/src/store/features/ppm/expenditure-categories-api'
import {
  useGetPortfolioOptionsQuery,
  useGetPortfolioProgramOptionsQuery,
} from '@/src/store/features/ppm/portfolios-api'
import { useCreateProjectMutation } from '@/src/store/features/ppm/projects-api'
import { useGetStrategicThemeOptionsQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { toFormErrors } from '@/src/utils'
import { Card, DatePicker, Form, Input, Modal, Select, Timeline, Typography } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { useCallback, useEffect, useMemo } from 'react'

const { Item } = Form

export interface CreateProjectFormProps {
  onFormComplete: () => void
  onFormCancel: () => void
}

interface CreateProjectFormValues {
  portfolioId: string
  programId?: string
  lifecycleId?: string
  key: string
  name: string
  description: string
  expenditureCategoryId: number
  start?: Date
  end?: Date
  sponsorIds: string[]
  ownerIds: string[]
  managerIds: string[]
  memberIds: string[]
  strategicThemeIds: string[]
}

const mapToRequestValues = (
  values: CreateProjectFormValues,
): CreateProjectRequest => {
  return {
    name: values.name,
    description: values.description,
    key: values.key,
    expenditureCategoryId: values.expenditureCategoryId,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    portfolioId: values.portfolioId,
    programId: values.programId,
    projectLifecycleId: values.lifecycleId,
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
    managerIds: values.managerIds,
    memberIds: values.memberIds,
    strategicThemeIds: values.strategicThemeIds,
  } as CreateProjectRequest
}

const keyPattern = /^[A-Z0-9]{2,20}$/

const CreateProjectForm = ({
  onFormComplete,
  onFormCancel,
}: CreateProjectFormProps) => {
  const messageApi = useMessage()

  const [createProject] = useCreateProjectMutation()

  const {
    data: lifecycleData,
    error: lifecycleOptionsError,
  } = useGetProjectLifecyclesQuery(ProjectLifecycleState.Active)

  const lifecycleOptions = useMemo(() => {
    if (!lifecycleData) return []
    return [...lifecycleData]
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((lc) => ({
        label: lc.name,
        value: lc.id,
      }))
  }, [lifecycleData])

  const {
    data: expenditureData,
    error: expenditureOptionsError,
  } = useGetExpenditureCategoryOptionsQuery(false)

  const {
    data: portfolioData,
    error: portfolioOptionsError,
  } = useGetPortfolioOptionsQuery()

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<CreateProjectFormValues>({
      onSubmit: useCallback(
        async (values: CreateProjectFormValues, form) => {
          try {
            const request = mapToRequestValues(values)
            const response = await createProject(request)
            if (response.error) throw response.error

            messageApi.success(
              'Project created successfully. Project key: ' +
                response.data.key,
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
                  'An error occurred while creating the project. Please try again.',
              )
            }
            return false
          }
        },
        [createProject, messageApi],
      ),
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while creating the project. Please try again.',
      permission: 'Permissions.Projects.Create',
    })

  const {
    data: programData,
    error: programOptionsError,
  } = useGetPortfolioProgramOptionsQuery(form.getFieldValue('portfolioId'), {
    skip: !form.getFieldValue('portfolioId'),
  })

  const {
    data: employeeData,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(false)

  const {
    data: strategicThemeData,
    error: strategicThemeOptionsError,
  } = useGetStrategicThemeOptionsQuery(false)

  useEffect(() => {
    const firstError =
      expenditureOptionsError ??
      portfolioOptionsError ??
      programOptionsError ??
      employeeOptionsError ??
      strategicThemeOptionsError ??
      lifecycleOptionsError
    if (firstError) {
      console.error(firstError)
      messageApi.error(
        firstError.detail ??
          'An error occurred while loading form data. Please try again.',
      )
    }
  }, [
    employeeOptionsError,
    expenditureOptionsError,
    lifecycleOptionsError,
    portfolioOptionsError,
    programOptionsError,
    messageApi,
    strategicThemeOptionsError,
  ])

  const selectedLifecycleId = Form.useWatch('lifecycleId', form)

  const { data: selectedLifecycle } = useGetProjectLifecycleQuery(
    selectedLifecycleId,
    { skip: !selectedLifecycleId },
  )

  const phaseItems = useMemo(() => {
    if (!selectedLifecycle?.phases) return []
    return [...selectedLifecycle.phases]
      .sort((a, b) => a.order - b.order)
      .map((phase) => ({
        content: (
          <>
            <Typography.Text strong>{phase.name}</Typography.Text>
            <br />
            <Typography.Text type="secondary">{phase.description}</Typography.Text>
          </>
        ),
      }))
  }, [selectedLifecycle?.phases])

  return (
    <Modal
      title="Create Project"
      open={isOpen}
      width={'60vw'}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Create"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden
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
          />
        </Item>
        <Item name="programId" label="Program">
          <Select
            allowClear
            options={programData ?? []}
            placeholder="Select Program"
            disabled={!form.getFieldValue('portfolioId')}
          />
        </Item>
        <Item
          name="key"
          label="Key"
          extra="2-20 uppercase alphanumeric characters (A-Z, 0-9)"
          rules={[
            { required: true, message: 'Key is required.' },
            {
              pattern: keyPattern,
              message:
                'Key must be 2-20 uppercase alphanumeric characters (A-Z, 0-9).',
            },
          ]}
          normalize={(value) => (value ?? '').toUpperCase()}
        >
          <Input
            placeholder="Enter new key"
            autoComplete="off"
            showCount
            maxLength={20}
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
        <Item name="memberIds" label="Members">
          <EmployeeSelect
            employees={employeeData ?? []}
            allowMultiple={true}
            placeholder="Select Members"
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
        <Item name="lifecycleId" label="Lifecycle">
          <Select
            allowClear
            options={lifecycleOptions}
            placeholder="Select Lifecycle"
          />
        </Item>
      </Form>
      {phaseItems.length > 0 && (
        <Card size="small" title="Phases">
          <Timeline items={phaseItems} />
        </Card>
      )}
    </Modal>
  )
}

export default CreateProjectForm
