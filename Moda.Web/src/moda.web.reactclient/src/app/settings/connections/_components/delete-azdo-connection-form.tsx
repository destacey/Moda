'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { AzureDevOpsConnectionDetailsDto } from '@/src/services/moda-api'
import { useDeleteConnectionMutation } from '@/src/store/features/app-integration/connections-api'
import { Descriptions, Modal } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Descriptions

interface DeleteAzdoConnectionFormProps {
  showForm: boolean
  connection: AzureDevOpsConnectionDetailsDto
  onFormSave: () => void
  onFormCancel: () => void
}

const DeleteAzdoConnectionForm = (props: DeleteAzdoConnectionFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const messageApi = useMessage()

  const [deleteConnectionMutation, { error: deleteConnectionError }] =
    useDeleteConnectionMutation()

  const { hasClaim } = useAuth()
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Connections.Delete',
  )

  const deleteConnection = async (
    connection: AzureDevOpsConnectionDetailsDto,
  ) => {
    try {
      const response = await deleteConnectionMutation(connection.id)
      if (response.error) {
        throw response.error
      }
      return true
    } catch (error) {
      messageApi.error(
        'An unexpected error occurred while deleting the Azure DevOps connection.',
      )
      console.log(error)
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await deleteConnection(props.connection)) {
        setIsOpen(false)
        props.onFormSave()
        messageApi.success('Successfully deleted Azure DevOps connection.')
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
  }, [props])

  useEffect(() => {
    if (!props.connection) return
    if (canManageTeamMemberships) {
      setIsOpen(props.showForm)
    } else {
      handleCancel()
      messageApi.error(
        'You do not have permission to deleted Azure DevOps connections.',
      )
    }
  }, [
    canManageTeamMemberships,
    handleCancel,
    messageApi,
    props.connection,
    props.showForm,
  ])

  return (
    <Modal
      title="Are you sure you want to delete this Azure DevOps connection?"
      open={isOpen}
      onOk={handleOk}
      okText="Delete"
      okType="danger"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <Descriptions size="small" column={1}>
        <Item label="Name">{props.connection?.name}</Item>
      </Descriptions>
    </Modal>
  )
}

export default DeleteAzdoConnectionForm
