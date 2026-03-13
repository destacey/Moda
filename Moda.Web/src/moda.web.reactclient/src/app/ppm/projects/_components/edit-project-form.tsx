'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import { useMessage } from '@/src/components/contexts/messaging'
import { useModalForm } from '@/src/hooks'
import { UpdateProjectRequest } from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useGetExpenditureCategoryOptionsQuery } from '@/src/store/features/ppm/expenditure-categories-api'
import {
  useGetProjectQuery,
  useUpdateProjectMutation,
} from '@/src/store/features/ppm/projects-api'
import { useGetStrategicThemeOptionsQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { toFormErrors } from '@/src/utils'
import { DatePicker, Form, Modal, Select } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import dayjs from 'dayjs'
import { useCallback, useEffect } from 'react'

const { Item } = Form

export interface EditProjectFormProps {
  projectKey: string
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditProjectFormValues {
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
  values: EditProjectFormValues,
  projectId: string,
): UpdateProjectRequest => {
  return {
    id: projectId,
    name: values.name,
    description: values.description,
    expenditureCategoryId: values.expenditureCategoryId,
    start: (values.start as any)?.format('YYYY-MM-DD'),
    end: (values.end as any)?.format('YYYY-MM-DD'),
    sponsorIds: values.sponsorIds,
    ownerIds: values.ownerIds,
    managerIds: values.managerIds,
    memberIds: values.memberIds,
    strategicThemeIds: values.strategicThemeIds,
  } as UpdateProjectRequest
}

const EditProjectForm = ({
  projectKey,
  onFormComplete,
  onFormCancel,
}: EditProjectFormProps) => {
  const messageApi = useMessage()

  const [updateProject] = useUpdateProjectMutation()

  const { data: projectData, isLoading, error } = useGetProjectQuery(projectKey)

  const { data: expenditureData, error: expenditureOptionsError } =
    useGetExpenditureCategoryOptionsQuery(false)

  const { data: employeeData, error: employeeOptionsError } =
    useGetEmployeeOptionsQuery(true)

  const { data: strategicThemeData, error: strategicThemeOptionsError } =
    useGetStrategicThemeOptionsQuery(false)

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<EditProjectFormValues>({
      onSubmit: useCallback(
        async (values: EditProjectFormValues, form) => {
          try {
            const request = mapToRequestValues(values, projectData.id)
            const response = await updateProject({
              request,
              cacheKey: projectData.key,
            })
            if (response.error) throw response.error

            messageApi.success('Project updated successfully.')
            return true
          } catch (error) {
            if (error.status === 422 && error.errors) {
              const formErrors = toFormErrors(error.errors)
              form.setFields(formErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            } else {
              messageApi.error(
                error.detail ??
                  'An error occurred while updating the project. Please try again.',
              )
            }
            return false
          }
        },
        [updateProject, projectData, messageApi],
      ),
      onComplete: onFormComplete,
      onCancel: onFormCancel,
      errorMessage:
        'An error occurred while updating the project. Please try again.',
      permission: 'Permissions.Projects.Update',
    })

  // Initialize form values when data is loaded
  useEffect(() => {
    if (!projectData) return

    form.setFieldsValue({
      name: projectData.name,
      description: projectData.description,
      expenditureCategoryId: projectData.expenditureCategory.id,
      start: projectData.start ? dayjs(projectData.start) : undefined,
      end: projectData.end ? dayjs(projectData.end) : undefined,
      sponsorIds: projectData.projectSponsors.map((s) => s.id),
      ownerIds: projectData.projectOwners.map((o) => o.id),
      managerIds: projectData.projectManagers.map((m) => m.id),
      memberIds: projectData.projectMembers.map((m) => m.id),
      strategicThemeIds: projectData.strategicThemes.map((t) => t.id),
    })
  }, [projectData, form])

  useEffect(() => {
    if (
      error ||
      expenditureOptionsError ||
      employeeOptionsError ||
      strategicThemeOptionsError
    ) {
      console.error(
        expenditureOptionsError ??
          error ??
          employeeOptionsError ??
          strategicThemeOptionsError,
      )
      messageApi.error(
        expenditureOptionsError.detail ??
          error.detail ??
          employeeOptionsError.detail ??
          strategicThemeOptionsError.detail ??
          'An error occurred while loading form data. Please try again.',
      )
    }
  }, [
    employeeOptionsError,
    error,
    expenditureOptionsError,
    messageApi,
    strategicThemeOptionsError,
  ])

  return (
    <Modal
      title="Edit Project"
      open={isOpen}
      width={'60vw'}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden
    >
      <Form form={form} size="small" layout="vertical" name="edit-project-form">
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
          />
        </Item>
        <Item
          name="start"
          label="Start"
          rules={[
            {
              validator: (_, value) => {
                const status = projectData?.status.name
                if ((status === 'Active' || status === 'Completed') && !value) {
                  return Promise.reject(
                    new Error(
                      'Start date is required for active or completed projects',
                    ),
                  )
                }
                return Promise.resolve()
              },
            },
          ]}
        >
          <DatePicker />
        </Item>
        <Item
          name="end"
          label="End"
          dependencies={['start']}
          rules={[
            {
              validator: (_, value) => {
                const status = projectData?.status.name
                if ((status === 'Active' || status === 'Completed') && !value) {
                  return Promise.reject(
                    new Error(
                      'End date is required for active or completed projects',
                    ),
                  )
                }
                return Promise.resolve()
              },
            },
            ({ getFieldValue }) => ({
              validator(_, value) {
                const start = getFieldValue('start')
                if (!start || (start && start <= value)) {
                  return Promise.resolve()
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
              (option?.label?.toLowerCase() ?? '').includes(input.toLowerCase())
            }
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default EditProjectForm
