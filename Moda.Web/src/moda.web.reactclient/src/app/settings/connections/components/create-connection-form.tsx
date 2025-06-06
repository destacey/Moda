import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  CreateAzureDevOpsBoardConnectionRequest,
  TestAzureDevOpsBoardConnectionRequest,
} from '@/src/services/moda-api'
import {
  useCreateAzdoConnectionMutation,
  useTestAzdoConfigurationMutation,
} from '@/src/store/features/app-integration/azdo-integration-api'
import { toFormErrors } from '@/src/utils'
import { Button, Divider, Form, Input, Modal, Typography } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input
const { Text } = Typography

export interface CreateConnectionFormProps {
  showForm: boolean
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateConnectionFormValues {
  name: string
  description?: string
  organization?: string | null
  personalAccessToken?: string | null
}

const mapToRequestValues = (values: CreateConnectionFormValues) => {
  return {
    name: values.name,
    description: values.description,
    organization: values.organization,
    personalAccessToken: values.personalAccessToken,
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
  const messageApi = useMessage()
  const [testConfigurationResult, setTestConfigurationResult] =
    useState<string>()
  const [isTestingConfiguration, setTestingConfiguration] = useState(false)

  const [createConnection, { error: createConnectionError }] =
    useCreateAzdoConnectionMutation()

  const { hasClaim } = useAuth()
  const canCreateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Create',
  )

  const [testConfig, { error: testConfigError }] =
    useTestAzdoConfigurationMutation()

  const testConnectionConfiguration = useCallback(
    async (configuration: TestAzureDevOpsBoardConnectionRequest) => {
      const response = await testConfig(configuration)
      if (response.error) {
        setTestConfigurationResult('Failed to test configuration.')
      } else {
        setTestConfigurationResult('Successfully tested configuration.')
      }
      setTestingConfiguration(false)
    },
    [testConfig],
  )

  const create = async (
    values: CreateConnectionFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      const response = await createConnection(request)
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
      destroyOnHidden={true}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="create-connection-form"
      >
        <Item label="Name" name="name" rules={[{ required: true }]}>
          <TextArea
            autoSize={{ minRows: 1, maxRows: 2 }}
            showCount
            maxLength={128}
          />
        </Item>
        <Item
          name="description"
          label="Description"
          initialValue=""
          rules={[{ max: 1024 }]}
        >
          <MarkdownEditor
            value={form.getFieldValue('description')}
            onChange={(value) => form.setFieldValue('description', value || '')}
            maxLength={1024}
          />
        </Item>

        {/* TODO: make the configuration section dynamic based on the connector  */}

        <Divider orientation="left" style={{ marginTop: '50px' }}>
          Azure DevOps Configuration
        </Divider>
        <Item
          label="Organization"
          name="organization"
          rules={[{ required: true }]}
        >
          <Input showCount maxLength={128} />
        </Item>
        <Item
          label="Personal Access Token"
          name="personalAccessToken"
          rules={[{ required: true }]}
        >
          <Input showCount maxLength={128} />
        </Item>

        <Item>
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
                personalAccessToken: form.getFieldValue('personalAccessToken'),
              })
            }}
          >
            Test Configuration
          </Button>
          <Text type="secondary" style={{ marginLeft: '10px' }}>
            {testConfigurationResult}
          </Text>
        </Item>
      </Form>
    </Modal>
  )
}

export default CreateConnectionForm
