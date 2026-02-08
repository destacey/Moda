'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { RoleDto } from '@/src/services/moda-api'
import { useDeleteRoleMutation } from '@/src/store/features/user-management/roles-api'
import { Modal } from 'antd'
import { useEffect, useState } from 'react'

export interface DeleteRoleFormProps {
  role: RoleDto
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
}

const DeleteRoleForm = (props: DeleteRoleFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)

  const messageApi = useMessage()

  const [deleteRoleMutation] = useDeleteRoleMutation()

  const { hasClaim } = useAuth()
  const editableRole =
    props.role && props.role.name !== 'Admin' && props.role.name !== 'Basic'
  const canDeleteRole =
    hasClaim('Permission', 'Permissions.Roles.Delete') && editableRole

  const formAction = async (role: RoleDto) => {
    try {
      const response = await deleteRoleMutation(role.id)

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error: any) {
      if (error.statusCode === 409 && error.detail) {
        messageApi.error(error.detail)
      } else {
        messageApi.error(error?.messages?.join() ?? 'Failed to delete role')
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await formAction(props.role)) {
        messageApi.success('Successfully deleted Role.')
        props.onFormComplete()
        setIsOpen(false)
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
      messageApi.error('An unexpected error occurred while deleting the role.')
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  useEffect(() => {
    if (!props.role) return
    if (canDeleteRole) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canDeleteRole, props])

  return (
    <Modal
      title="Are you sure you want to delete this Role?"
      open={isOpen}
      onOk={handleOk}
      okText="Delete"
      okType="danger"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      mask={{ blur: false }}
      maskClosable={false}
      keyboard={false}
      destroyOnHidden={true}
    >
      {props.role?.name}
    </Modal>
  )
}

export default DeleteRoleForm

