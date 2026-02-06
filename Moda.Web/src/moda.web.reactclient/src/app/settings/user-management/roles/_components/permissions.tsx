import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import useTheme from '@/src/components/contexts/theme'
import { useGetPermissionsQuery } from '@/src/store/features/user-management/permissions-api'
import { useUpdatePermissionsMutation } from '@/src/store/features/user-management/roles-api'
import { Button, Collapse, Divider, Flex, Input, Space, Spin, Switch, Tag } from 'antd'
import { SearchOutlined } from '@ant-design/icons'
import { useCallback, useEffect, useMemo, useState } from 'react'

interface PermissionsProps {
  roleId: string
  permissions: string[]
  onDirtyChange?: (isDirty: boolean) => void
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
  const { hasPermissionClaim } = useAuth()
  const messageApi = useMessage()
  const theme = useTheme()

  const canUpdate = hasPermissionClaim('Permissions.Roles.Update')

  const [permissions, setPermissions] = useState<string[]>(props.permissions)
  const [searchText, setSearchText] = useState('')
  const [expandedKeys, setExpandedKeys] = useState<string[]>([])

  const isDirty = useMemo(() => {
    if (permissions.length !== props.permissions.length) return true
    const sorted = [...permissions].sort()
    const sortedProps = [...props.permissions].sort()
    return sorted.some((p, i) => p !== sortedProps[i])
  }, [permissions, props.permissions])

  const { onDirtyChange } = props
  useEffect(() => {
    onDirtyChange?.(isDirty)
  }, [isDirty, onDirtyChange])

  useEffect(() => {
    if (!isDirty) return

    const message =
      'You have unsaved permission changes. Are you sure you want to leave?'

    // Browser navigation (refresh, close tab, external URL)
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      e.preventDefault()
      e.returnValue = message
    }

    // Client-side navigation (capture phase to intercept before Next.js)
    const handleClick = (e: MouseEvent) => {
      const anchor = (e.target as HTMLElement).closest('a')
      if (!anchor) return
      const href = anchor.getAttribute('href')
      if (!href || href === '#') return
      if (
        anchor.target === '_blank' ||
        anchor.hasAttribute('download') ||
        anchor.href === window.location.href
      )
        return
      if (!window.confirm(message)) {
        e.preventDefault()
        e.stopPropagation()
      }
    }

    window.addEventListener('beforeunload', handleBeforeUnload)
    document.addEventListener('click', handleClick, true)
    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload)
      document.removeEventListener('click', handleClick, true)
    }
  }, [isDirty])

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

  const filteredGroups = useMemo(() => {
    if (!searchText) return permissionGroups
    const lower = searchText.toLowerCase()
    return permissionGroups
      .map((group) => {
        const groupNameMatch = group.name.toLowerCase().includes(lower)
        if (groupNameMatch) return group
        const filteredPermissions = group.permissions.filter(
          (p) =>
            p.name.toLowerCase().includes(lower) ||
            p.description.toLowerCase().includes(lower),
        )
        if (filteredPermissions.length === 0) return null
        return { ...group, permissions: filteredPermissions }
      })
      .filter(Boolean) as PermissionGroup[]
  }, [permissionGroups, searchText])

  const hasPermission = useCallback(
    (permission: string) => permissions.includes(permission),
    [permissions],
  )

  const handlePermissionChange = (item: PermissionItem) => {
    if (!hasPermission(item.name)) {
      setPermissions([...permissions, item.name])
    } else {
      setPermissions(permissions.filter((p) => p !== item.name))
    }
  }

  const handleToggleGroup = (group: PermissionGroup, checked: boolean) => {
    const updated = [...permissions]
    group.permissions.forEach((p) => {
      const idx = updated.indexOf(p.name)
      if (checked && idx === -1) {
        updated.push(p.name)
      } else if (!checked && idx !== -1) {
        updated.splice(idx, 1)
      }
    })
    setPermissions(updated)
  }

  const handleSelectAll = (select: boolean) => {
    if (select) {
      const allNames = permissionGroups.flatMap((g) =>
        g.permissions.map((p) => p.name),
      )
      setPermissions([...new Set([...permissions, ...allNames])])
    } else {
      const allNames = new Set(
        permissionGroups.flatMap((g) => g.permissions.map((p) => p.name)),
      )
      setPermissions(permissions.filter((p) => !allNames.has(p)))
    }
  }

  const handleExpandAll = () => {
    setExpandedKeys(filteredGroups.map((g) => g.name))
  }

  const handleCollapseAll = () => {
    setExpandedKeys([])
  }

  const isAllExpanded =
    filteredGroups.length > 0 &&
    expandedKeys.length === filteredGroups.length

  const getGroupSelectedCount = (group: PermissionGroup) =>
    group.permissions.filter((p) => hasPermission(p.name)).length

  const isGroupAllSelected = (group: PermissionGroup) =>
    group.permissions.every((p) => hasPermission(p.name))

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

  const collapseItems = filteredGroups.map((group) => {
    const selectedCount = getGroupSelectedCount(group)
    const totalCount = group.permissions.length
    const allSelected = selectedCount === totalCount

    return {
      key: group.name,
      label: (
        <Flex align="center">
          <span style={{ fontWeight: 600 }}>{group.name}</span>
          <Tag
            color={selectedCount > 0 ? 'blue' : undefined}
            style={{ marginLeft: 8 }}
          >
            {selectedCount} of {totalCount}
          </Tag>
        </Flex>
      ),
      extra: (
        <Flex align="center" gap={8} onClick={(e) => e.stopPropagation()}>
          <span style={{ fontSize: 13, color: theme.token.colorTextSecondary }}>
            Toggle All
          </span>
          <Switch
            size="small"
            checked={allSelected}
            disabled={!canUpdate}
            onChange={(checked) => handleToggleGroup(group, checked)}
          />
        </Flex>
      ),
      children: (
        <div>
          {group.permissions.map((permission, idx) => (
            <div key={permission.name}>
              {idx > 0 && <Divider style={{ margin: '8px 0' }} />}
              <Flex
                justify="space-between"
                align="center"
                style={{ padding: '4px 0' }}
              >
                <div>
                  <div style={{ fontWeight: 600 }}>
                    {permission.description}
                  </div>
                  <div
                    style={{
                      fontSize: 13,
                      color: theme.token.colorTextSecondary,
                    }}
                  >
                    {permission.name}
                  </div>
                </div>
                <Switch
                  checked={hasPermission(permission.name)}
                  disabled={!canUpdate}
                  onChange={() => handlePermissionChange(permission)}
                />
              </Flex>
            </div>
          ))}
        </div>
      ),
    }
  })

  return (
    <div>
      <Flex justify="space-between" align="center" style={{ marginBottom: 16 }}>
        <Input
          placeholder="Search permissions..."
          prefix={<SearchOutlined />}
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
          allowClear
          style={{ maxWidth: 300 }}
        />
        <Space>
          {canUpdate && (
            <>
              <a onClick={() => handleSelectAll(true)}>Select All</a>
              <a onClick={() => handleSelectAll(false)}>Unselect All</a>
            </>
          )}
          <a onClick={isAllExpanded ? handleCollapseAll : handleExpandAll}>
            {isAllExpanded ? 'Collapse All' : 'Expand All'}
          </a>
          {canUpdate && (
            <Button type="primary" onClick={handleSave} disabled={!isDirty}>
              Save Permissions
            </Button>
          )}
        </Space>
      </Flex>

      <Collapse
        activeKey={expandedKeys}
        onChange={(keys) => setExpandedKeys(keys as string[])}
        items={collapseItems}
      />
    </div>
  )
}

export default Permissions
