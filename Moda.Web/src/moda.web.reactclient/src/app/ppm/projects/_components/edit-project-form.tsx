'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import { EmployeeSelect } from '@/src/components/common/organizations'
import useAuth from '@/src/components/contexts/auth'
import {
  ProjectDetailsDto,
  UpdateProjectRequest,
} from '@/src/services/moda-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useGetExpenditureCategoryOptionsQuery } from '@/src/store/features/ppm/expenditure-categories-api'
import {
  useGetProjectQuery,
  useUpdateProjectMutation,
} from '@/src/store/features/ppm/projects-api'
import { useGetStrategicThemeOptionsQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { DatePicker, Form, Modal, Select } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'
import dayjs from 'dayjs'
import { toFormErrors } from '@/src/utils'

const { Item } = Form

export interface EditProjectFormProps {
  projectKey: number
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
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
    strategicThemeIds: values.strategicThemeIds,
  } as UpdateProjectRequest
}

const EditProjectForm = (props: EditProjectFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditProjectFormValues>()
  const formValues = Form.useWatch([], form)

  const [updateProject, { error: mutationError }] = useUpdateProjectMutation()

  const {
    data: projectData,
    isLoading,
    error,
  } = useGetProjectQuery(props.projectKey)

  const {
    data: expenditureData,
    isLoading: expenditureOptionsIsLoading,
    error: expenditureOptionsError,
  } = useGetExpenditureCategoryOptionsQuery(false)

  const {
    data: employeeData,
    isLoading: employeeOptionsIsLoading,
    error: employeeOptionsError,
  } = useGetEmployeeOptionsQuery(true)

  const {
    data: strategicThemeData,
    isLoading: strategicThemeOptionsIsLoading,
    error: strategicThemeOptionsError,
  } = useGetStrategicThemeOptionsQuery(false)

  const { hasPermissionClaim } = useAuth()
  const canUpdateProject = hasPermissionClaim('Permissions.Projects.Update')

  const mapToFormValues = useCallback(
    (project: ProjectDetailsDto) => {
      if (!project) {
        throw new Error('Project not found')
      }
      form.setFieldsValue({
        name: project.name,
        description: project.description,
        expenditureCategoryId: project.expenditureCategory.id,
        start: project.start ? dayjs(project.start) : undefined,
        end: project.end ? dayjs(project.end) : undefined,
        sponsorIds: project.projectSponsors.map((s) => s.id),
        ownerIds: project.projectOwners.map((o) => o.id),
        managerIds: project.projectManagers.map((m) => m.id),
        strategicThemeIds: project.strategicThemes.map((t) => t.id),
      })
    },
    [form],
  )

  const update = async (
    values: EditProjectFormValues,
    project: ProjectDetailsDto,
  ) => {
    try {
      const request = mapToRequestValues(values, project.id)
      const response = await updateProject({
        request,
        cacheKey: project.key,
      })
      if (response.error) {
        throw response.error
      }
      props.messageApi.success(`Project updated successfully.`)
      return true
    } catch (error) {
      console.error('update error', error)
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.detail ??
            'An error occurred while updating the project. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values, projectData)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      props.messageApi.error(
        'An error occurred while updating the project. Please try again.',
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
    if (!projectData) return
    if (canUpdateProject) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(projectData)
      }
    } else {
      props.onFormCancel()
      props.messageApi.error('You do not have permission to update projects.')
    }
  }, [canUpdateProject, mapToFormValues, projectData, props])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

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
      props.messageApi.error(
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
    props.messageApi,
    strategicThemeOptionsError,
  ])

  return (
    <>
      <Modal
        title="Edit Project"
        open={isOpen}
        width={'60vw'}
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
          name="edit-project-form"
        >
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
                  if (
                    (status === 'Active' || status === 'Completed') &&
                    !value
                  ) {
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
                  if (
                    (status === 'Active' || status === 'Completed') &&
                    !value
                  ) {
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

export default EditProjectForm
