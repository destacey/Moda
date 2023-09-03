import useAuth from '@/src/app/components/contexts/auth'
import { CreateAzureDevOpsBoardConnectionRequest } from '@/src/services/moda-api'
import { useCreateAzdoBoardsConnectionMutation } from '@/src/services/queries/app-integration-queries'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal, message } from 'antd'
import { useEffect, useState } from 'react'

export interface CreateConnectionFormProps {
  showForm: boolean
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateConnectionFormValues {
  name: string
  description?: string
}

const mapToRequestValues = (values: CreateConnectionFormValues) => {
  return {
    name: values.name,
    description: values.description,
  } as CreateAzureDevOpsBoardConnectionRequest
}

const CreateConnectionForm = ({
  showForm,
  onFormCreate,
  onFormCancel,
}: CreateConnectionFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateConnectionFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const createConnection = useCreateAzdoBoardsConnectionMutation()

  const { hasClaim } = useAuth()
  const canCreateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Create',
  )

  const create = async (
    values: CreateConnectionFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      await createConnection.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error('An error occurred while creating the connection.')
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
        onFormCreate()
        messageApi.success('Successfully created connection.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }

  useEffect(() => {
    if (canCreateConnection) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to create connections.')
    }
  }, [canCreateConnection, onFormCancel, showForm, messageApi])

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
        title="Create Connection"
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
          name="create-connection-form"
        >
          <Form.Item label="Name" name="name" rules={[{ required: true }]}>
            <Input.TextArea
              autoSize={{ minRows: 1, maxRows: 4 }}
              showCount
              maxLength={256}
            />
          </Form.Item>
          <Form.Item
            name="description"
            label="Description"
            help="Markdown enabled"
          >
            <Input.TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default CreateConnectionForm
