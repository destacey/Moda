'use client'

import useAuth from '@/src/components/contexts/auth'
import {
  useGetUserRolesQuery,
  useManageUserRolesMutation,
} from '@/src/store/features/user-management/users-api'
import { Modal, Spin, Transfer, TransferProps } from 'antd'
import { MessageInstance } from 'antd/es/message/interface'
import { useEffect, useState } from 'react'

export interface ManageUserRolesFormProps {
  userId: string
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface RecordType {
  key: string
  title: string
}

const ManageUserRolesForm: React.FC<ManageUserRolesFormProps> = (
  props: ManageUserRolesFormProps,
) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [roles, setRoles] = useState<RecordType[]>([])
  const [targetKeys, setTargetKeys] = useState<string[]>([])
  const [selectedKeys, setSelectedKeys] = useState<TransferProps['targetKeys']>(
    [],
  )

  const {
    data: userRolesData,
    isLoading: userRolesLoading,
    error: userRolesError,
  } = useGetUserRolesQuery({
    id: props.userId,
    includeUnassigned: true,
  })

  const [manageUserRoles, { error: mutationError }] =
    useManageUserRolesMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateUserRoles = hasPermissionClaim('Permissions.UserRoles.Update')

  useEffect(() => {
    if (canUpdateUserRoles) {
      setIsOpen(props.showForm)
    } else {
      props.messageApi.error('You do not have permission to update user roles.')
      props.onFormCancel()
    }
  }, [canUpdateUserRoles, props])

  useEffect(() => {
    if (!userRolesData) return

    const transformedRoles = userRolesData
      .map((role) => ({
        key: role.roleName,
        title: role.roleName,
      }))
      .sort((a, b) => a.title.localeCompare(b.title)) as RecordType[]

    setRoles(transformedRoles)

    const activeRoleNames = userRolesData
      .filter((role) => role.enabled)
      .map((role) => role.roleName)
      .sort((a, b) => {
        const roleA =
          userRolesData.find((r) => r.roleName === a)?.roleName || ''
        const roleB =
          userRolesData.find((r) => r.roleName === b)?.roleName || ''
        return roleA.localeCompare(roleB)
      })

    setTargetKeys(activeRoleNames)
  }, [userRolesData])

  useEffect(() => {
    if (userRolesError) {
      props.messageApi.error('Failed to load user roles.')
      setIsOpen(false)
      props.onFormCancel()
    }
  }, [userRolesError, props])

  useEffect(() => {
    setSelectedKeys([])
  }, [userRolesData])

  const updateRoles = async (userId: string, roleNames: string[]) => {
    try {
      const response = await manageUserRoles({
        userId: userId,
        roleNames: roleNames,
      })
      if (response.error) {
        throw response.error
      }

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.detail ??
            'An error occurred while updating the user roles. Please try again.',
        )
      }

      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      if (await updateRoles(props.userId, targetKeys)) {
        props.messageApi.success('Successfully updated user roles.')
        setIsOpen(false)
        props.onFormComplete()
      }
    } catch (error) {
      console.error('handleOk error', error)
      props.messageApi.error(
        'An error occurred while updating user roles. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    props.onFormCancel()
  }

  const onChange = (nextTargetKeys: string[]) => {
    const sortedTargetKeys = nextTargetKeys.sort((a, b) => {
      const roleA = roles.find((r) => r.title === a)?.title || ''
      const roleB = roles.find((r) => r.title === b)?.title || ''
      return roleA.localeCompare(roleB)
    })

    setTargetKeys(sortedTargetKeys)
  }

  const onSelectChange: TransferProps['onSelectChange'] = (
    sourceSelectedKeys,
    targetSelectedKeys,
  ) => {
    setSelectedKeys([...sourceSelectedKeys, ...targetSelectedKeys])
  }

  return (
    <Modal
      title="Manage User Roles"
      open={isOpen}
      onOk={handleOk}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      maskClosable={false}
      keyboard={false} // disable esc key to close modal
      destroyOnClose={true}
    >
      <Spin spinning={userRolesLoading} size="large">
        <Transfer
          dataSource={roles || []}
          targetKeys={targetKeys}
          selectedKeys={selectedKeys}
          render={(item) => item.title}
          onChange={onChange}
          onSelectChange={onSelectChange}
          titles={['Available', 'Assigned']}
          listStyle={{ width: 300, height: 400 }}
          showSearch
        />
      </Spin>
    </Modal>
  )
}

export default ManageUserRolesForm
