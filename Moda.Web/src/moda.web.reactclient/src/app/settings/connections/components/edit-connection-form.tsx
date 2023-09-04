import useAuth from '@/src/app/components/contexts/auth'
import {
  AzureDevOpsBoardsConnectionDetailsDto,
  TestAzureDevOpsBoardConnectionRequest,
  UpdateAzureDevOpsBoardConnectionRequest,
} from '@/src/services/moda-api'
import {
  testAzdoBoardsConfiguration,
  useGetAzdoBoardsConnectionById,
  useUpdateAzdoBoardsConnectionMutation,
} from '@/src/services/queries/app-integration-queries'
import { toFormErrors } from '@/src/utils'
import { Button, Divider, Form, Input, Modal, Typography, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'

export interface EditConnectionFormProps {
  showForm: boolean
  id: string
  onFormUpdate: () => void
  onFormCancel: () => void
}

interface EditConnectionFormValues {
  id: string
  name: string
  description?: string | null
  organization?: string | null
  personalAccessToken?: string | null
}

const mapToRequestValues = (values: EditConnectionFormValues) => {
  return {
    id: values.id,
    name: values.name,
    description: values.description,
    organization: values.organization,
    personalAccessToken: values.personalAccessToken,
  } as UpdateAzureDevOpsBoardConnectionRequest
}

const EditConnectionForm = ({
  showForm,
  id,
  onFormUpdate,
  onFormCancel,
}: EditConnectionFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditConnectionFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()
  const [testConfigurationResult, setTestConfigurationResult] =
    useState<string>()
  const [isTestingConfiguration, setTestingConfiguration] = useState(false)

  const { data: connectionData } = useGetAzdoBoardsConnectionById(id)
  const updateConnection = useUpdateAzdoBoardsConnectionMutation()

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )

  const testConnectionConfiguration = useCallback(
    async (configuration: TestAzureDevOpsBoardConnectionRequest) => {
      setTestConfigurationResult(
        await testAzdoBoardsConfiguration(configuration),
      )
      setTestingConfiguration(false)
    },
    [],
  )

  const mapToFormValues = useCallback(
    (connection: AzureDevOpsBoardsConnectionDetailsDto) => {
      form.setFieldsValue({
        id: connection.id,
        name: connection.name,
        description: connection.description,
        organization: connection.configuration?.organization,
        personalAccessToken: connection.configuration?.personalAccessToken,
      })
    },
    [form],
  )

  const update = async (values: EditConnectionFormValues): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      await updateConnection.mutateAsync(request)
      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error('An error occurred while editing the connection.')
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
        onFormUpdate()
        form.resetFields()
        messageApi.success('Successfully updated connection.')
      }
    } catch (errorInfo) {
      console.error('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }, [onFormCancel, form])

  useEffect(() => {
    if (!connectionData) return
    if (canUpdateConnection) {
      setIsOpen(showForm)
      if (showForm) {
        mapToFormValues(connectionData)
      }
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to edit connections.')
    }
  }, [
    canUpdateConnection,
    connectionData,
    mapToFormValues,
    messageApi,
    onFormCancel,
    showForm,
  ])

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
        title="Edit Connection"
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
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="edit-connection-form"
        >
          <Form.Item name="id" hidden={true}>
            <Input />
          </Form.Item>
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

          {/* TODO: make the configuration section dynamic based on the connector  */}

          <Divider orientation="left" style={{ marginTop: '50px' }}>
            Azure DevOps Configuration
          </Divider>
          <Form.Item
            label="Organization"
            name="organization"
            rules={[{ required: true }]}
          >
            <Input showCount maxLength={128} />
          </Form.Item>
          <Form.Item
            label="Personal Access Token"
            name="personalAccessToken"
            rules={[{ required: true }]}
          >
            <Input showCount maxLength={128} />
          </Form.Item>

          <Form.Item>
            <Button
              type="primary"
              disabled={
                !form.getFieldValue('organization') ||
                !form.getFieldValue('personalAccessToken')
              }
              loading={isTestingConfiguration}
              onClick={() => {
                setTestingConfiguration(true)
                testConnectionConfiguration({
                  organization: form.getFieldValue('organization'),
                  personalAccessToken: form.getFieldValue(
                    'personalAccessToken',
                  ),
                })
              }}
            >
              Test Configuration
            </Button>
            <Typography.Text type="secondary" style={{ marginLeft: '10px' }}>
              {testConfigurationResult}
            </Typography.Text>
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditConnectionForm
