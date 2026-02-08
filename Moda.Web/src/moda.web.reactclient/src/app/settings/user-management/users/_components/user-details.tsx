'use client'

import useAuth from '@/src/components/contexts/auth'
import { UserDetailsDto } from '@/src/services/moda-api'
import { EditOutlined, InfoCircleOutlined } from '@ant-design/icons'
import { Button, Card, Descriptions, Flex, List } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { useCallback, useState } from 'react'
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

  const { hasPermissionClaim } = useAuth()
  const canUpdateUserRoles = hasPermissionClaim('Permissions.UserRoles.Update')

  const onOpenManageUserRolesFormClosed = useCallback((wasSaved: boolean) => {
    setOpenManageUserRolesForm(false)
  }, [])

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
          <Item label="Last Activity">
            {user.lastActivityAt
              ? dayjs(user.lastActivityAt).format('MMM D, YYYY h:mm A')
              : null}
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
            dataSource={user.roles}
            renderItem={(item) => (
              <ListItem>
                <Link href={`/settings/user-management/roles/${item.id}`}>
                  {item.name}
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

