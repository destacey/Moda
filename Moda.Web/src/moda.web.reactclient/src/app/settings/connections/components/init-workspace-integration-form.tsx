import useAuth from '@/src/app/components/contexts/auth'
import { InitWorkspaceIntegrationRequest } from '@/src/services/moda-api'
import { useInitAzdoBoardsConnectionWorkspaceMutation } from '@/src/services/queries/app-integration-queries'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, message } from 'antd'
import { useEffect, useState } from 'react'

const { Item } = Form

export interface InitWorkspaceIntegrationFormProps {
  showForm: boolean
  connectionId: string
  externalId: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface InitWorkspaceIntegrationFormValues {
  workspaceKey: string
}

const mapToRequestValues = (values: InitWorkspaceIntegrationFormValues) => {
  return {
    workspaceKey: values.workspaceKey,
  } as InitWorkspaceIntegrationRequest
}

const InitWorkspaceIntegrationForm = (
  props: InitWorkspaceIntegrationFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<InitWorkspaceIntegrationFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )

  const initWorkspaceMutation = useInitAzdoBoardsConnectionWorkspaceMutation()
  const init = async (
    values: InitWorkspaceIntegrationFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      request.id = props.connectionId
      request.externalId = props.externalId

      await initWorkspaceMutation.mutateAsync(request)
      messageApi.success('Successfully initialized workspace.')
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          `Failed to initialize workspace. Error: ${error.supportMessage}`,
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
      if (await init(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormSave()
        messageApi.success('Successfully initialized workspace.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
    form.resetFields()
  }

  useEffect(() => {
    if (canUpdateConnection) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to initialize workspaces.')
    }
  }, [canUpdateConnection, messageApi, props])

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
        title="Initialize Workspace"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Init"
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
          name="init-workspace-form"
        >
          <Item
            name="workspaceKey"
            label="Workspace Key"
            extra="The workspace key is a unique identifier for the workspace. Workspace key's must be uppercase letters and numbers only, start with an uppercase letter, and be between 2-20 characters."
            rules={[
              {
                required: true,
                message: 'The Workspace Key field is required.',
              },
              {
                min: 2,
                max: 20,
                message:
                  'The Workspace Key field must be between 2-20 characters.',
              },
              {
                pattern: /^([A-Z][A-Z0-9]{1,19})$/,
                message:
                  "The Workspace Key field is invalid. Workspace key's must be uppercase letters and numbers only and start with an uppercase letter.",
              },
            ]}
          >
            <Input
              showCount
              maxLength={20}
              onInput={(e) =>
                ((e.target as HTMLInputElement).value = (
                  e.target as HTMLInputElement
                ).value.toUpperCase())
              }
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default InitWorkspaceIntegrationForm
