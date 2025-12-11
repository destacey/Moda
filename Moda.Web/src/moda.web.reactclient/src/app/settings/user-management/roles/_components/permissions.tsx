import { useMessage } from '@/src/components/contexts/messaging'
import useTheme from '@/src/components/contexts/theme'
import { useGetPermissionsQuery } from '@/src/store/features/user-management/permissions-api'
import { useUpdatePermissionsMutation } from '@/src/store/features/user-management/roles-api'
import {
  Row,
  Col,
  List,
  Switch,
  Badge,
  Button,
  Typography,
  Space,
  Spin,
} from 'antd'
import { useMemo, useState } from 'react'

const { Title } = Typography
const { Item } = List

interface PermissionsProps {
  roleId: string
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
  const messageApi = useMessage()
  const theme = useTheme()

  const [permissions, setPermissions] = useState<string[]>(props.permissions)
  const [activePermissionGroup, setActivePermissionGroup] =
    useState<PermissionGroup | null>(null)

  const {
    data: permissionsData,
    isLoading,
    error,
    refetch,
  } = useGetPermissionsQuery()

  const [updatePermissions, { error: updatePermissionsError }] =
    useUpdatePermissionsMutation()

  const permissionGroups = useMemo(() => {
    if (!permissionsData) return []
    const groups = permissionsData.reduce(
      (acc: PermissionGroup[], permission) => {
        const item: PermissionItem = {
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
      [],
    )

    return groups?.sort((a, b) => a.name.localeCompare(b.name)) ?? groups
  }, [permissionsData])

  // Derive active permission group: use the selected one or default to first group
  const effectiveActiveGroup =
    activePermissionGroup ??
    (permissionGroups.length > 0 ? permissionGroups[0] : null)

  const handlePermissionChange = (item: PermissionItem) => {
    let updatedPermissions: string[] = [...permissions]

    if (!hasPermission(item.name)) {
      updatedPermissions = [...permissions, item.name]
    } else {
      updatedPermissions = permissions.filter((p) => p !== item.name)
    }

    setPermissions(updatedPermissions)
  }

  const hasPermission = (permission: string) => permissions.includes(permission)

  const badgeLabel = (permissions: PermissionItem[]) => {
    const unselectedPermissions = permissions.filter(
      (p) => !hasPermission(p.name),
    )

    if (unselectedPermissions.length === 0) return 'All'

    return permissions.filter((i) => hasPermission(i.name)).length
  }

  const handleSave = async () => {
    try {
      await updatePermissions({
        roleId: props.roleId,
        permissions: permissions,
      })

      messageApi.success('Permissions saved successfully')
    } catch (error) {
      messageApi.error('Failed to save permissions')
    }
  }

  if (isLoading) return <Spin size="small" />

  function handleSelectAll(select: boolean): void {
    if (effectiveActiveGroup) {
      const updatedPermissions: string[] = [...permissions]

      effectiveActiveGroup.permissions.forEach((p) => {
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
      <Row>
        <Col span={7}>
          <Title level={5}>Permission Groups</Title>
          <List
            size="small"
            dataSource={permissionGroups}
            style={{
              height: 'calc(100vh - 400px)',
              overflowY: 'scroll',
            }}
            renderItem={(item) => (
              <Item
                onClick={() => setActivePermissionGroup(item)}
                style={{
                  cursor: 'pointer',
                  display: 'flex',
                  borderLeft: `${
                    effectiveActiveGroup?.name == item.name
                      ? '2px solid' + theme.token.colorPrimary
                      : ''
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
              </Item>
            )}
          />
        </Col>
        <Col
          span={16}
          push={1}
          style={{ display: 'flex', flexDirection: 'column' }}
        >
          <Space vertical style={{ height: '100%' }}>
            <Title level={5}>
              {effectiveActiveGroup?.name} Available Permissions
            </Title>
            {effectiveActiveGroup?.permissions?.map((permission, i) => (
              <Space key={i} style={{ paddingBottom: '15px' }}>
                <Switch
                  checked={hasPermission(permission.name)}
                  onChange={() => {
                    handlePermissionChange(permission)
                  }}
                  style={{ marginRight: '10px' }}
                />{' '}
                {permission.description}
              </Space>
            ))}

            <Space style={{ marginTop: '15px' }}>
              <a href="#" onClick={() => handleSelectAll(true)}>
                Select All{' '}
              </a>{' '}
              |{' '}
              <a href="#" onClick={() => handleSelectAll(false)}>
                Unselect All{' '}
              </a>
            </Space>
          </Space>

          <Space>
            <Button
              type="primary"
              htmlType="button"
              onClick={() => handleSave()}
            >
              Save Permissions
            </Button>
          </Space>
        </Col>
      </Row>
    </div>
  )
}

export default Permissions
