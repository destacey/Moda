import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { ConnectorType, CONNECTOR_NAMES } from '@/src/types/connectors'
import { toFormErrors } from '@/src/utils'
import { Form, Modal } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import { ConnectionFormBase } from './connection-form-base'
import { ConnectorTypeSelector } from './connector-type-selector'
import { useCreateConnectionMutation } from '@/src/store/features/app-integration/connections-api'

export interface CreateConnectionFormProps {
  showForm: boolean
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateConnectionFormValues {
  name: string
  description?: string
  // Azure DevOps specific
  organization?: string
  personalAccessToken?: string
  // Azure OpenAI specific
  baseUrl?: string
  apiKey?: string
  deploymentName?: string
}

export const getDiscriminator = (connector: ConnectorType): string => {
  switch (connector) {
    case ConnectorType.AzureDevOps:
      return 'azure-devops'
    case ConnectorType.AzureOpenAI:
      return 'azure-openai'
    case ConnectorType.OpenAI:
      return 'openai'
    default:
      return 'openai'
  }
}

const CreateConnectionForm = ({
  showForm,
  onFormCreate,
  onFormCancel,
}: CreateConnectionFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [selectedConnector, setSelectedConnector] =
    useState<ConnectorType | null>(null)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateConnectionFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const [createConnection] = useCreateConnectionMutation()

  const { hasClaim } = useAuth()
  const canCreateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Create',
  )

  const create = useCallback(
    async (values: CreateConnectionFormValues): Promise<boolean> => {
      if (!selectedConnector) return false

      try {
        // Build polymorphic request with $type discriminator
        const request = {
          ...values,
          $type: getDiscriminator(selectedConnector),
        }

        const response = await createConnection(request)
        if (response.error) {
          throw response.error
        }
        return true
      } catch (error: any) {
        if (error.status === 422 && error.errors) {
          const formErrors = toFormErrors(error.errors)
          form.setFields(formErrors)
          messageApi.error('Correct the validation error(s) to continue.')
        } else {
          messageApi.error('An error occurred while creating the connection.')
        }
        return false
      }
    },
    [createConnection, form, messageApi, selectedConnector],
  )

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await create(values)) {
        setIsOpen(false)
        setSelectedConnector(null)
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
    setSelectedConnector(null)
    form.resetFields()
    onFormCancel()
  }

  const handleBack = () => {
    setSelectedConnector(null)
    form.resetFields()
  }

  useEffect(() => {
    if (canCreateConnection) {
      setIsOpen(showForm)
      if (!showForm) {
        setSelectedConnector(null)
        form.resetFields()
      }
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to create connections.')
    }
  }, [canCreateConnection, onFormCancel, showForm, messageApi, form])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <Modal
      title={
        selectedConnector === null
          ? 'Create Connection'
          : `Create ${CONNECTOR_NAMES[selectedConnector]} Connection`
      }
      open={isOpen}
      onOk={selectedConnector === null ? undefined : handleOk}
      onCancel={handleCancel}
      okButtonProps={{
        disabled: !isValid,
        hidden: selectedConnector === null,
      }}
      okText="Create"
      confirmLoading={isSaving}
      maskClosable={false}
      keyboard={false}
      destroyOnClose={true}
      width={selectedConnector === null ? 600 : 800}
      footer={
        selectedConnector === null
          ? null
          : selectedConnector !== null
            ? undefined
            : null
      }
    >
      {selectedConnector === null ? (
        <ConnectorTypeSelector onSelect={setSelectedConnector} />
      ) : (
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="create-connection-form"
        >
          <ConnectionFormBase
            connector={selectedConnector}
            mode="create"
            form={form}
          />
        </Form>
      )}
    </Modal>
  )
}

export default CreateConnectionForm
