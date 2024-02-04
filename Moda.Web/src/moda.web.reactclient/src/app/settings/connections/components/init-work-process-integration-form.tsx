import useAuth from '@/src/app/components/contexts/auth'
import { InitWorkProcessIntegrationRequest } from '@/src/services/moda-api'
import { useInitAzdoBoardsConnectionWorkProcessMutation } from '@/src/services/queries/app-integration-queries'
import { Modal, Typography, message } from 'antd'
import { useEffect, useState } from 'react'

const { Text } = Typography

export interface InitWorkProcessIntegrationFormProps {
  showForm: boolean
  connectionId: string
  externalId: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface InitWorkProcessIntegrationFormValues {}

// const mapToRequestValues = (values: InitWorkProcessIntegrationFormValues) => {
//   return {
//     workspaceKey: values.workspaceKey,
//   } as InitWorkProcessIntegrationRequest
// }

const InitWorkProcessIntegrationForm = (
  props: InitWorkProcessIntegrationFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdateConnection = hasClaim(
    'Permission',
    'Permissions.Connections.Update',
  )

  const initWorkProcessMutation =
    useInitAzdoBoardsConnectionWorkProcessMutation()

  const init = async (): Promise<boolean> => {
    try {
      const request = {
        id: props.connectionId,
        externalId: props.externalId,
      } as InitWorkProcessIntegrationRequest

      await initWorkProcessMutation.mutateAsync(request)
      messageApi.success('Successfully initialized work process.')
      return true
    } catch (error) {
      messageApi.error(
        `Failed to initialize work process. Error: ${error.supportMessage}`,
      )
      console.error(error)
      // }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await init()) {
        setIsOpen(false)
        props.onFormSave()
        messageApi.success('Successfully initialized work process.')
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
  }

  useEffect(() => {
    if (canUpdateConnection) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
      messageApi.error(
        'You do not have permission to initialize work processes.',
      )
    }
  }, [canUpdateConnection, messageApi, props])

  return (
    <>
      {contextHolder}
      <Modal
        title="Initialize Work Process"
        open={isOpen}
        onOk={handleOk}
        okText="Init"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Text>
          Initializing the work process will create the necessary work item
          types, work statuses, workflows, and work process.
        </Text>
        {}
      </Modal>
    </>
  )
}

export default InitWorkProcessIntegrationForm
