import { useMessage } from '@/src/components/contexts/messaging'
import { InitWorkProcessIntegrationRequest } from '@/src/services/moda-api'
import { useInitAzdoConnectionWorkProcessMutation } from '@/src/store/features/app-integration/azdo-integration-api'
import { Modal, Typography } from 'antd'
import { useCallback } from 'react'
import { useConfirmModal } from '@/src/hooks'

const { Text } = Typography

export interface InitWorkProcessIntegrationFormProps {
  connectionId: string
  externalId: string
  onFormSave: () => void
  onFormCancel: () => void
}

const InitWorkProcessIntegrationForm = ({
  connectionId,
  externalId,
  onFormSave,
  onFormCancel,
}: InitWorkProcessIntegrationFormProps) => {
  const messageApi = useMessage()

  const [initAzdoConnectionWorkProcess] =
    useInitAzdoConnectionWorkProcessMutation()

  const { isOpen, isSaving, handleOk, handleCancel } = useConfirmModal({
    onSubmit: useCallback(async () => {
      try {
        const request = {
          id: connectionId,
          externalId: externalId,
        } as InitWorkProcessIntegrationRequest

        const response = await initAzdoConnectionWorkProcess(request)
        if (response.error) {
          throw response.error
        }
        messageApi.success('Successfully initialized work process.')
        return true
      } catch (error) {
        messageApi.error(
          `Failed to initialize work process. Error: ${error.detail}`,
        )
        console.error(error)
        return false
      }
    }, [initAzdoConnectionWorkProcess, connectionId, externalId, messageApi]),
    onComplete: onFormSave,
    onCancel: onFormCancel,
    errorMessage: 'Failed to initialize work process.',
    permission: 'Permissions.Connections.Update',
  })

  return (
    <Modal
      title="Initialize Work Process"
      open={isOpen}
      onOk={handleOk}
      okText="Init"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      <Text>
        Initializing the work process will create the necessary work item types,
        work statuses, workflows, and work process.
      </Text>
    </Modal>
  )
}

export default InitWorkProcessIntegrationForm
