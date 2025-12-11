'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  AzureDevOpsBoardsConnectionDetailsDto,
  TestAzureDevOpsBoardConnectionRequest,
  UpdateAzureDevOpsBoardConnectionRequest,
} from '@/src/services/moda-api'
import {
  useGetAzdoConnectionQuery,
  useTestAzdoConfigurationMutation,
  useUpdateAzdoConnectionMutation,
} from '@/src/store/features/app-integration/azdo-integration-api'
import { toFormErrors } from '@/src/utils'
import { Button, Divider, Form, Input, Modal, Typography } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input
const { Text } = Typography

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
  const messageApi = useMessage()
  const [testConfigurationResult, setTestConfigurationResult] =
    useState<string>()
  const [isTestingConfiguration, setTestingConfiguration] = useState(false)

  const { data: connectionData } = useGetAzdoConnectionQuery(id)
  const [updateConnection, { error: updateConnectionError }] =
    useUpdateAzdoConnectionMutation()

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )

  const [testConfig, { error: testConfigError }] =
    useTestAzdoConfigurationMutation()

  const testConnectionConfiguration = useCallback(
    async (configuration: TestAzureDevOpsBoardConnectionRequest) => {
      const response = await testConfig(configuration)
      console.log('response', response)
      if (response.error) {
        setTestConfigurationResult('Failed to test configuration.')
      } else {
        setTestConfigurationResult('Successfully tested configuration.')
      }
      setTestingConfiguration(false)
    },
    [testConfig],
  )

  const mapToFormValues = useCallback(
    (connection: AzureDevOpsBoardsConnectionDetailsDto) => {
      form.setFieldsValue({
        id: connection.id,
        name: connection.name,
        description: connection.description || '',
        organization: connection.configuration?.organization,
        personalAccessToken: connection.configuration?.personalAccessToken,
      })
    },
    [form],
  )

  const update = async (values: EditConnectionFormValues): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      const response = await updateConnection(request)
      if (response.error) throw response.error
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
      destroyOnHidden={true}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="edit-connection-form"
      >
        <Item name="id" hidden={true}>
          <Input />
        </Item>
        <Item label="Name" name="name" rules={[{ required: true }]}>
          <TextArea
            autoSize={{ minRows: 1, maxRows: 2 }}
            showCount
            maxLength={128}
          />
        </Item>
        <Item name="description" label="Description" rules={[{ max: 1024 }]}>
          <MarkdownEditor
            value={form.getFieldValue('description')}
            onChange={(value) => form.setFieldValue('description', value || '')}
            maxLength={1024}
          />
        </Item>

        {/* TODO: make the configuration section dynamic based on the connector  */}

        <Divider titlePlacement="left" style={{ marginTop: '50px' }}>
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

export default EditConnectionForm
