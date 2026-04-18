'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { UpdateWorkItemProjectRequest } from '@/src/services/wayd-api'
import { useGetProjectOptionsQuery } from '@/src/store/features/ppm/projects-api'
import {
  useGetWorkItemProjectInfoQuery,
  useUpdateWorkItemProjectMutation,
} from '@/src/store/features/work-management/workspace-api'
import { toFormErrors } from '@/src/utils'
import { Form, Modal, Select, Space, Typography } from 'antd'
import { useEffect } from 'react'
import { useModalForm } from '@/src/hooks'

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

  const { form, isOpen, isValid, isSaving, handleOk, handleCancel } =
    useModalForm<EditWorkItemProjectFormValues>({
      onSubmit: async (values: EditWorkItemProjectFormValues, form) => {
          try {
            const request = mapToRequestValues(values, props.workItemId)

            const response = await updateWorkItemProject({
              workspaceId: props.workspaceId,
              request: request,
              cacheKey: props.workItemKey,
            })

            if (response.error) {
              throw response.error
            }

            messageApi.success('Work item project updated successfully.')
            return true
          } catch (error) {
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
        },
      onComplete: props.onFormComplete,
      onCancel: props.onFormCancel,
      errorMessage:
        'An error occurred while updating the work item. Please try again.',
      permission: 'Permissions.Projects.ManageProjectWorkItems',
    })

  useEffect(() => {
    if (!workItemProjectInfoData || !isOpen) return
    const projectId =
      workItemProjectInfoData.project?.id ??
      workItemProjectInfoData.parentProject?.id
    form.setFieldsValue({ projectId })
  }, [workItemProjectInfoData, isOpen, form])

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
        keyboard={false}
        destroyOnHidden
      >
        <Space vertical>
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
