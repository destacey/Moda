'use client'

import {
  SetExternalUrlTemplatesRequest,
  WorkspaceDto,
} from '@/src/services/moda-api'
import {
  SetWorkspaceExternalUrlTemplatesRequest,
  useGetWorkspaceQuery,
  useSetWorkspaceExternalUrlTemplatesMutation,
} from '@/src/store/features/work-management/workspace-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input

export interface SetWorkspaceExternalUrlTemplatesFormProps {
  showForm: boolean
  workspaceId: string
  onFormUpdate: () => void
  onFormCancel: () => void
}

interface SetWorkspaceExternalUrlTemplatesFormValues {
  externalViewWorkItemUrlTemplate?: string | undefined
}

const mapToRequestValues = (
  workspaceId: string,
  values: SetWorkspaceExternalUrlTemplatesFormValues,
): SetWorkspaceExternalUrlTemplatesRequest => {
  return {
    workspaceId: workspaceId,
    externalUrlTemplatesRequest: {
      externalViewWorkItemUrlTemplate: values.externalViewWorkItemUrlTemplate,
    } as SetExternalUrlTemplatesRequest,
  }
}

const SetWorkspaceExternalUrlTemplatesForm = (
  props: SetWorkspaceExternalUrlTemplatesFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<SetWorkspaceExternalUrlTemplatesFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const {
    data: workspaceData,
    isLoading,
    refetch,
  } = useGetWorkspaceQuery(props.workspaceId)
  const [setWorkspaceExternalUrlTemplatesMutation] =
    useSetWorkspaceExternalUrlTemplatesMutation()

  const mapToFormValues = useCallback(
    (workspace: WorkspaceDto) => {
      form.setFieldsValue({
        externalViewWorkItemUrlTemplate:
          workspace.externalViewWorkItemUrlTemplate,
      })
    },
    [form],
  )

  const update = async (
    values: SetWorkspaceExternalUrlTemplatesFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(props.workspaceId, values)
      const response = await setWorkspaceExternalUrlTemplatesMutation(request)
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
        messageApi.error(
          'An unexpected error occurred while updating external URL templates.',
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
      if (await update(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormUpdate()
        messageApi.success('Successfully set external URL templates')
        refetch() // this makes sure the workspace data is updated if the form is opened again
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
  }, [props, form])

  const loadData = useCallback(async () => {
    try {
      mapToFormValues(workspaceData)
      setIsValid(true)
    } catch (error) {
      handleCancel()
      messageApi.error('An unexpected error occurred while loading form data.')
      console.error(error)
    }
  }, [handleCancel, mapToFormValues, messageApi, workspaceData])

  useEffect(() => {
    if (!workspaceData) return
    setIsOpen(props.showForm)
    if (props.showForm) {
      loadData()
    }
  }, [isLoading, loadData, props.showForm, workspaceData])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      {contextHolder}
      <Modal
        title="Set External URL Templates"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form form={form} layout="vertical">
          <Item
            label="View Work Item URL"
            name="externalViewWorkItemUrlTemplate"
            extra={
              <span>
                <br />
                This template plus the work item external id will create a url
                to view the work item in the external system.
              </span>
            }
            rules={[
              {
                max: 256,
                message: 'The URL must be equal to or less than 256 characters',
              },
            ]}
          >
            <TextArea
              autoSize={{ minRows: 1, maxRows: 4 }}
              showCount
              maxLength={256}
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default SetWorkspaceExternalUrlTemplatesForm
