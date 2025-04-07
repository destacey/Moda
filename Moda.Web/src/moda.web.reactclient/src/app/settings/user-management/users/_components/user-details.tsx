'use client'

import useAuth from '@/src/components/contexts/auth'
import { UserDetailsDto } from '@/src/services/moda-api'
import { useGetUserRolesQuery } from '@/src/store/features/user-management/users-api'
import { EditOutlined, InfoCircleOutlined } from '@ant-design/icons'
import { Button, Card, Descriptions, Flex, List, Space } from 'antd'
import Link from 'next/link'
import { useCallback, useEffect, useState } from 'react'
import { ManageUserRolesForm } from '.'

const { Item } = Descriptions
const { Item: ListItem } = List

interface UserDetailsProps {
  user: UserDetailsDto
}

const UserDetails = (props: UserDetailsProps) => {
  const { user } = props
  const [openManageUserRolesForm, setOpenManageUserRolesForm] =
    useState<boolean>(false)

  const {
    data: userRoleData,
    isLoading,
    error,
    refetch,
  } = useGetUserRolesQuery({ id: user?.id })

  const { hasPermissionClaim } = useAuth()
  const canUpdateUserRoles = hasPermissionClaim('Permissions.UserRoles.Update')

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const onOpenManageUserRolesFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenManageUserRolesForm(false)
      if (wasSaved) {
        refetch()
      }
    },
    [refetch],
  )

  if (!user) return null

  return (
    <>
      <Flex vertical gap="middle">
        <Descriptions column={2} size="small">
          <Item label="User Name">{user.userName}</Item>
          <Item label="Email">{user.email}</Item>
          <Item label="First Name">{user.firstName}</Item>
          <Item label="Last Name">{user.lastName}</Item>
          <Item label="Phone Number">{user.phoneNumber}</Item>
          <Item label="Employee">
            <Link href={`/organizations/employees/${user.employee?.key}`}>
              {user.employee?.name}
            </Link>
          </Item>
          <Item label="Is Active?">{user.isActive?.toString()}</Item>
        </Descriptions>
        <Card
          size="small"
          title="Roles"
          style={{ width: 300 }}
          extra={
            <>
              {canUpdateUserRoles && (
                <Button
                  type="text"
                  icon={<EditOutlined />}
                  onClick={() => setOpenManageUserRolesForm(true)}
                />
              )}
            </>
          }
        >
          <List
            size="small"
            dataSource={userRoleData}
            loading={isLoading}
            renderItem={(item) => (
              <ListItem>
                <Link href={`/settings/user-management/roles/${item.roleId}`}>
                  {item.roleName}
                </Link>
                {item.description && (
                  <InfoCircleOutlined title={item.description} />
                )}
              </ListItem>
            )}
          />
        </Card>
      </Flex>
      {openManageUserRolesForm && (
        <ManageUserRolesForm
          userId={user.id}
          showForm={openManageUserRolesForm}
          onFormComplete={() => onOpenManageUserRolesFormClosed(true)}
          onFormCancel={() => setOpenManageUserRolesForm(false)}
        />
      )}
    </>
  )
}

export default UserDetails
