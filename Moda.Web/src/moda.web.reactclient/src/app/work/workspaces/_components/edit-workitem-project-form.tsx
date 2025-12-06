'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { UpdateWorkItemProjectRequest } from '@/src/services/moda-api'
import { useGetProjectOptionsQuery } from '@/src/store/features/ppm/projects-api'
import {
  useGetWorkItemProjectInfoQuery,
  useUpdateWorkItemProjectMutation,
} from '@/src/store/features/work-management/workspace-api'
import { toFormErrors } from '@/src/utils'
import { Form, Modal, Select, Space, Typography } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { Text } = Typography

export interface EditWorkItemProjectFormProps {
  workItemId: string
  workItemKey: string
  workspaceId: string
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditWorkItemProjectFormValues {
  projectId: string
}

const mapToRequestValues = (
  values: EditWorkItemProjectFormValues,
  workItemId: string,
): UpdateWorkItemProjectRequest => {
  return {
    workItemId: workItemId,
    projectId: values.projectId,
  }
}

const EditWorkItemProjectForm = (props: EditWorkItemProjectFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditWorkItemProjectFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const [updateWorkItemProject] = useUpdateWorkItemProjectMutation()

  const {
    data: workItemProjectInfoData,
    isLoading,
    error,
  } = useGetWorkItemProjectInfoQuery({
    idOrKey: props.workspaceId,
    workItemKey: props.workItemKey,
  })

  const {
    data: projectOptions,
    isLoading: projectOptionsIsLoading,
    error: projectOptionsError,
  } = useGetProjectOptionsQuery()

  const { hasPermissionClaim } = useAuth()
  const canManageProjectWorkItems = hasPermissionClaim(
    'Permissions.Projects.ManageProjectWorkItems',
  )

  const mapToFormValues = useCallback(
    (projectId: string | null) => {
      form.setFieldsValue({
        projectId: projectId,
      })
    },
    [form],
  )

  const update = async (
    values: EditWorkItemProjectFormValues,
    workItemId: string,
    workItemKey: string,
    workspaceId: string,
  ) => {
    try {
      const request = mapToRequestValues(values, workItemId)

      const response = await updateWorkItemProject({
        workspaceId: workspaceId,
        request: request,
        cacheKey: workItemKey,
      })

      if (response.error) {
        throw response.error
      }

      messageApi.success('Work item project updated successfully.')
      return true
    } catch (error) {
      console.error('update error', error)
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the work item. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (
        await update(
          values,
          props.workItemId,
          props.workItemKey,
          props.workspaceId,
        )
      ) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while updating the work item. Please try again.',
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
    if (!workItemProjectInfoData) return
    if (canManageProjectWorkItems) {
      setIsOpen(true)
      const projectId =
        workItemProjectInfoData.project?.id ??
        workItemProjectInfoData.parentProject?.id
      mapToFormValues(projectId)
    } else {
      props.onFormCancel()
      messageApi.error(
        'You do not have permission to update the project on work items.',
      )
    }
  }, [
    canManageProjectWorkItems,
    mapToFormValues,
    messageApi,
    props,
    workItemProjectInfoData,
  ])

  useEffect(() => {
    if (error || projectOptionsError) {
      console.error(error ?? projectOptionsError)
      messageApi.error(
        error.detail ??
          projectOptionsError.detail ??
          'An error occurred while loading form data. Please try again.',
      )
    }
  }, [error, messageApi, projectOptionsError])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const projectSourceText =
    workItemProjectInfoData &&
    (workItemProjectInfoData.project
      ? 'The work item is currently overriding the Parent Project.  Clear and save to inherit the Parent Project.'
      : workItemProjectInfoData.parentProject
        ? 'The work item is currently is inheriting the Project from its Parent. Select a Project and save to override.'
        : 'The work item is currently not associated with any project.')

  return (
    <>
      <Modal
        title="Edit Project"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <Space direction="vertical">
          {projectSourceText && <Text italic>{projectSourceText}</Text>}
          <Form
            form={form}
            size="small"
            layout="vertical"
            name="edit-workitem-project-form"
          >
            <Item
              name="projectId"
              label="Project"
              rules={[
                {
                  required: !workItemProjectInfoData?.project,
                  message: 'Project is required',
                },
              ]}
            >
              <Select
                allowClear
                options={projectOptions ?? []}
                placeholder="Select Project"
                showSearch
                optionFilterProp="label"
                filterOption={(input, option) =>
                  (option?.label?.toLowerCase() ?? '').includes(
                    input.toLowerCase(),
                  )
                }
              />
            </Item>
          </Form>
        </Space>
      </Modal>
    </>
  )
}

export default EditWorkItemProjectForm
