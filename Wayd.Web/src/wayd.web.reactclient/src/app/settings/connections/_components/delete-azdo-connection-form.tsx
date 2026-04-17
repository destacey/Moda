'use client'

import { useMessage } from '@/src/components/contexts/messaging'
import { AzureDevOpsConnectionDetailsDto } from '@/src/services/moda-api'
import { useDeleteConnectionMutation } from '@/src/store/features/app-integration/connections-api'
import { Descriptions, Modal } from 'antd'
import { useConfirmModal } from '@/src/hooks'

const { Item } = Descriptions

interface DeleteAzdoConnectionFormProps {
  connection: AzureDevOpsConnectionDetailsDto
  onFormSave: () => void
  onFormCancel: () => void
}

const DeleteAzdoConnectionForm = ({
  connection,
  onFormSave,
  onFormCancel,
}: DeleteAzdoConnectionFormProps) => {
  const messageApi = useMessage()

  const [deleteConnectionMutation] =
    useDeleteConnectionMutation()

  const { isOpen, isSaving, handleOk, handleCancel } = useConfirmModal({
    onSubmit: async () => {
      try {
        const response = await deleteConnectionMutation(connection.id)
        if (response.error) {
          throw response.error
        }
        messageApi.success('Successfully deleted Azure DevOps connection.')
        return true
      } catch (error) {
        messageApi.error(
          'An unexpected error occurred while deleting the Azure DevOps connection.',
        )
        console.log(error)
        return false
      }
    },
    onComplete: onFormSave,
    onCancel: onFormCancel,
    errorMessage:
      'An unexpected error occurred while deleting the Azure DevOps connection.',
    permission: 'Permissions.Connections.Delete',
  })

  return (
    <Modal
      title="Are you sure you want to delete this Azure DevOps connection?"
      open={isOpen}
      onOk={handleOk}
      okText="Delete"
      okType="danger"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden
    >
      <Descriptions size="small" column={1}>
        <Item label="Name">{connection?.name}</Item>
      </Descriptions>
    </Modal>
  )
}

export default DeleteAzdoConnectionForm
