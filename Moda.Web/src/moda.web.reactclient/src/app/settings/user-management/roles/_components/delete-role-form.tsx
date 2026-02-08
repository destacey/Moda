'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { RoleDto } from '@/src/services/moda-api'
import {
  useDeleteRoleMutation,
  useGetRoleUsersCountQuery,
} from '@/src/store/features/user-management/roles-api'
import { Alert, Flex, Modal } from 'antd'
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

  const { data: countData } = useGetRoleUsersCountQuery(props.role.id, {
    skip: !props.role?.id,
  })

  const formAction = async (role: RoleDto) => {
    try {
      const response = await deleteRoleMutation(role.id)

      if (response.error) {
        throw response.error
      }

      return true
    } catch (error: any) {
      if (error.status === 409 && error.detail) {
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
    if (!props.role && !countData) return
    if (canDeleteRole) {
      setIsOpen(props.showForm)
    } else {
      props.onFormCancel()
    }
  }, [canDeleteRole, countData, props])

  return (
    <Modal
      title="Are you sure you want to delete this Role?"
      open={isOpen}
      onOk={handleOk}
      okText="Delete"
      okType="danger"
      okButtonProps={{ disabled: countData !== undefined && countData > 0 }}
      confirmLoading={isSaving}
      onCancel={handleCancel}
      mask={{ blur: false }}
      maskClosable={false}
      keyboard={false}
      destroyOnHidden={true}
    >
      <Flex vertical gap="small">
        {props.role?.name}
        {countData && countData > 0 && (
          <Alert
            title={`This role is assigned to ${countData} user${countData !== 1 ? 's' : ''}. Roles assigned to users cannot be deleted. Please remove the role from all users before attempting to delete.`}
            type="error"
            showIcon
          />
        )}
      </Flex>
    </Modal>
  )
}

export default DeleteRoleForm

