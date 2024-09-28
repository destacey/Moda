'use client'

import { UserDetailsDto } from '@/src/services/moda-api'
import { useGetUserRoles } from '@/src/services/queries/user-management-queries'
import { InfoCircleOutlined } from '@ant-design/icons'
import { Card, Descriptions, List } from 'antd'
import Link from 'next/link'

const { Item } = Descriptions
const { Item: ListItem } = List

interface UserDetailsProps {
  user: UserDetailsDto
  canEdit: boolean
}

const UserDetails = (props: UserDetailsProps) => {
  const { user } = props

  //const [openManageUserRolesForm, setOpenManageUserRolesForm] = useState<boolean>(false)
  const { data: userRoleData } = useGetUserRoles(user?.id)

  if (!user) return null

  return (
    <>
      <Descriptions>
        <Item label="User Name">{user.userName}</Item>
        <Item label="First Name">{user.firstName}</Item>
        <Item label="Last Name">{user.lastName}</Item>
        <Item label="Email">{user.email}</Item>
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
        // extra={
        //   <>
        //     {canEdit && (
        //       <Button
        //         type="text"
        //         icon={<EditOutlined />}
        //         onClick={() => setOpenManageUserRolesForm(true)}
        //       />
        //     )}
        //   </>
        // }
      >
        <List
          size="small"
          dataSource={userRoleData}
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
    </>
  )
}

export default UserDetails
