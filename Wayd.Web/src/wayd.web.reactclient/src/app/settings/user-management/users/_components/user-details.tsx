'use client'

import { UserDetailsDto } from '@/src/services/wayd-api'
import { InfoCircleOutlined } from '@ant-design/icons'
import { Card, Descriptions, Flex, List, Tag } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'

const { Item } = Descriptions
const { Item: ListItem } = List

interface UserDetailsProps {
  user: UserDetailsDto
}

const UserDetails = (props: UserDetailsProps) => {
  const { user } = props

  if (!user) return null

  return (
    <Flex vertical gap="middle">
      <Descriptions column={2} size="small">
        <Item label="User Name">{user.userName}</Item>
        <Item label="Email">{user.email}</Item>
        <Item label="First Name">{user.firstName}</Item>
        <Item label="Last Name">{user.lastName}</Item>
        <Item label="Phone Number">{user.phoneNumber}</Item>
        <Item label="Login Provider">
          {user.loginProvider === 'MicrosoftEntraId'
            ? 'Microsoft Entra ID'
            : user.loginProvider}
        </Item>
        <Item label="Employee">
          {user.employee && (
            <Link href={`/organizations/employees/${user.employee?.key}`}>
              {user.employee?.name}
            </Link>
          )}
        </Item>
        <Item label="Last Activity">
          {user.lastActivityAt
            ? dayjs(user.lastActivityAt).format('MMM D, YYYY h:mm A')
            : null}
        </Item>
        <Item label="Is Active?">{user.isActive?.toString()}</Item>
        {user.lockoutEnd && new Date(user.lockoutEnd) > new Date() && (
          <Item label="Account Status">
            <Tag color="error">
              Locked until {dayjs(user.lockoutEnd).format('MMM D, YYYY h:mm A')}
            </Tag>
          </Item>
        )}
      </Descriptions>
      <Card size="small" title="Roles" style={{ width: 300 }}>
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
  )
}

export default UserDetails
