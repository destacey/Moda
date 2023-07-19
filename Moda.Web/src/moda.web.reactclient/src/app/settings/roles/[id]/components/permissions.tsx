import useTheme from '@/src/app/components/contexts/theme'
import { getPermissionsClient, getRolesClient } from '@/src/services/clients'
import { ApplicationPermission, PermissionsClient, RoleDto } from '@/src/services/moda-api'
import { useGetPermissions, useUpdatePermissionsMutation } from '@/src/services/query'
import { Row, Col, List, Switch, Anchor, Badge, Button, message } from 'antd'
import React, { useEffect, useState } from 'react'
import { useQueryClient } from 'react-query'

interface PermissionsProps {
  roleId: string,
  permissions: string[]
}

interface PermissionGroup {
  name: string
  permissions: PermissionItem[]
}

interface PermissionItem {
  name: string
  description: string
}

const Permissions = (props: PermissionsProps) => {
  const availablePermissions = useGetPermissions()
  const [messageApi, contextHolder] = message.useMessage()
  const theme = useTheme()

  const [permissions, setPermissions] = useState<string[]>(props.permissions)
  const [permissionGroups, setPermissionGroups] = useState<PermissionGroup[]>()
  const [activePermissionGroup, setActivePermissionGroup] =
    useState<PermissionGroup>(null)

  const queryClient = useQueryClient();
  const updatePermissions = useUpdatePermissionsMutation(queryClient);

  useEffect(() => setPermissions(props.permissions), [props.permissions])

  useEffect(() => {
    const groups = availablePermissions.data?.reduce(
      (acc: PermissionGroup[], permission) => {
        const item = {
          name: permission.name,
          description: permission.description,
        }
        const group = acc.find((g) => g.name === permission.resource)
        if (group) {
          group.permissions.push(item)
        } else {
          acc.push({
            name: permission.resource,
            permissions: [item],
          })
        }
        return acc
      },
      []
    )

    setPermissionGroups(groups)
    setActivePermissionGroup(groups?.[0])
  }, [availablePermissions.data])

  const handlePermissionChange = (item: PermissionItem) => {
    let updatedPermissions: string[] = [...permissions]

    if (!hasPermission(item.name)) {
      updatedPermissions = [...permissions, item.name]
    } else {
      updatedPermissions = permissions.filter((p) => p !== item.name)
    }

    setPermissions(updatedPermissions);
  }

  const hasPermission = (permission: string) => permissions.includes(permission)

  const badgeLabel = (permissions: PermissionItem[]) => {
    const unselectedPermissions = permissions.filter(
      (p) => !hasPermission(p.name)
    )

    if (unselectedPermissions.length === 0) return 'All'

    return permissions.filter((i) => hasPermission(i.name)).length
  }

  const handleSave = async () => {
    try {
      await updatePermissions.mutateAsync({
          roleId: props.roleId,
          permissions: permissions
        }
      );

      messageApi.success('Permissions saved successfully')
    } catch (error) {
      messageApi.error('Failed to save permissions')
    }
  }

  if (availablePermissions.isLoading) 
    return <div>Loading...</div>

  function handleSelectAll(select: boolean): void {
    if (activePermissionGroup) {
      const updatedPermissions: string[] = [...permissions]

      activePermissionGroup.permissions.forEach((p) => {
        if (select) {
          if (!hasPermission(p.name)) {
            updatedPermissions.push(p.name)
          }
        } else {
          if (hasPermission(p.name)) {
            updatedPermissions.splice(updatedPermissions.indexOf(p.name), 1)
          }
        }
      })

      setPermissions(updatedPermissions)
    }
  }

  return (
    <div>
      {contextHolder}
      <Row>
        <Col span={4}>
          <p>Permission Groups</p>
          <List
            size="small"
            dataSource={permissionGroups}
            style={{
              height: 'calc(100vh - 320px)',
              overflowY: 'scroll',
            }}
            renderItem={(item) => (
              <List.Item
                onClick={() => setActivePermissionGroup(item)}
                style={{
                  cursor: 'pointer',
                  display: 'flex',
                  borderLeft: `2px solid ${
                    activePermissionGroup.name == item.name
                      ? theme.token.colorPrimary
                      : theme.appBarColor
                  }`,
                }}
              >
                {item.name}

                <Badge
                  count={badgeLabel(item.permissions)}
                  style={{
                    color: theme.token.colorWhite,
                    backgroundColor: theme.token.colorPrimary,
                  }}
                />
              </List.Item>
            )}
          />
        </Col>
        <Col
          span={19}
          push={1}
          style={{ display: 'flex', flexDirection: 'column' }}
        >
          <div style={{height: '100%'}}>
            <div>
              <p>{activePermissionGroup?.name} Available Permissions</p>
            </div>
            {activePermissionGroup?.permissions?.map((permission, i) => (
              <div key={i} style={{ paddingBottom: '15px' }}>
                <Switch
                  checked={hasPermission(permission.name)}
                  onChange={() => {
                    handlePermissionChange(permission)
                  }}
                  style={{ marginRight: '10px' }}
                />{' '}
                {permission.description}
              </div>
            ))}

            <div style={{marginTop: '15px'}}>
              <a href="#" onClick={() => handleSelectAll(true)}>Select All </a> | <a href="#" onClick={() => handleSelectAll(false)}>Unselect All </a>
            </div>
          </div>

          <div>
            <Button type="primary" htmlType="button" onClick={() => handleSave()}>
              Save Permissions
            </Button>
          </div>
        </Col>
      </Row>
    </div>
  )
}

export default Permissions
