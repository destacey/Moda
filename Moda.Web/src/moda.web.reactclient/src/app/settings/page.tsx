'use client'

import { Alert, Card, Divider, Layout, Menu, MenuProps, Typography } from 'antd'
import { useEffect, useState } from 'react'
import { PageTitle } from '../components/common'
import useAuth from '../components/contexts/auth'
import {
  ItemType,
  MenuItemGroupType,
  MenuItemType,
  SubMenuType,
} from 'antd/es/menu/hooks/useItems'

const { Content, Sider } = Layout
const { Title, Text } = Typography

enum SettingsTab {
  Default = 'default',

  // user-management
  Users = 'users',
  Roles = 'roles',

  // work-management
  WorkTypes = 'work-types',
  WorkStatuses = 'work-statuses',
  WorkProcesses = 'work-processes',

  // integrations
  Connections = 'connections',

  // other
  BackgroundJobs = 'background-jobs',
}

type RestrictedMenuItem = SubMenuType & {
  claimValue: string
  claimType: string
}

interface SectionMenuItem extends MenuItemGroupType {}

const authorizeMenuItems = (
  acc: ItemType<MenuItemType>[],
  item: SectionMenuItem | RestrictedMenuItem | SubMenuType,
  claimCheck: (claimValue: string) => boolean,
) => {
  if ('type' in item) {
    const children = item.children
      ? item.children.reduce(
          (acc, item: RestrictedMenuItem | SubMenuType) =>
            authorizeMenuItems(acc, item, claimCheck),
          [],
        )
      : undefined
    if (item.type === 'group' && children && children.length > 0) {
      acc.push({ ...item, children })
    }
  } else if ('claimValue' in item) {
    // TODO: fix dev error - Warning: React does not recognize the `claimValue` prop on a DOM element. If you intentionally want it to appear in the DOM as a custom attribute, spell it as lowercase `claimvalue` instead. If you accidentally passed it from a parent component, remove it from the DOM element.
    if (claimCheck(item.claimValue)) {
      acc.push(item)
    }
  } else {
    acc.push(item)
  }
  return acc
}

const getSectionMenuItem = (
  label: React.ReactNode,
  key: React.Key,
  children?: SubMenuType[],
): SectionMenuItem => {
  return {
    label,
    key,
    children,
    type: 'group',
  } as SectionMenuItem
}

const getRestrictedMenuItem = (
  claimValue: string,
  label: string,
  key: string,
  children?: SubMenuType[],
): RestrictedMenuItem => {
  return {
    label,
    key,
    children,
    claimValue,
  } as RestrictedMenuItem
}

const getRegularMenuItem = (
  label: React.ReactNode,
  key: string,
  children?: SubMenuType[],
): SubMenuType => {
  return {
    label,
    key,
    children,
  } as SubMenuType
}

const settingsMenuItems: ItemType[] = [
  getSectionMenuItem('User Management', 'user-management', [
    getRestrictedMenuItem('Permissions.Users.View', 'Users', SettingsTab.Users),
    getRestrictedMenuItem('Permissions.Roles.View', 'Roles', SettingsTab.Roles),
  ]),

  getSectionMenuItem('Work Management', 'work-management', [
    getRegularMenuItem('Work Types', SettingsTab.WorkTypes),
    getRegularMenuItem('Work Statuses', SettingsTab.WorkStatuses),
    getRegularMenuItem('Work Processes', SettingsTab.WorkProcesses),
  ]),

  getSectionMenuItem('Integrations', 'integration-management', [
    getRestrictedMenuItem(
      'Permissions.Connections.View',
      'Connections',
      SettingsTab.Connections,
    ),
  ]),

  getSectionMenuItem('Other', 'other', [
    getRestrictedMenuItem(
      'Permissions.BackgroundJobs.View',
      'Background Jobs',
      SettingsTab.BackgroundJobs,
    ),
  ]),
]

const SettingsPage: React.FC = () => {
  const [menuItems, setMenuItems] = useState<ItemType<MenuItemType>[]>([])
  const [activeSetting, setActiveSetting] = useState(SettingsTab.Default)
  const { hasPermissionClaim } = useAuth()

  useEffect(() => {
    // Reduce the menu items based on the user's claims and transformed into antd menu items using the getItem function
    var filteredMenu = settingsMenuItems.reduce(
      (acc: ItemType<MenuItemType>[], item: SectionMenuItem) =>
        authorizeMenuItems(acc, item, hasPermissionClaim),
      [],
    )

    if (filteredMenu.length === 0) return

    setMenuItems(filteredMenu)
  }, [hasPermissionClaim])

  useEffect(() => {
    console.log('activeSetting', activeSetting)
  }, [activeSetting])

  if (menuItems.length === 0) {
    return (
      <Alert
        message="You do not have access to any settings"
        description="Please contact your administrator to request access."
        type="error"
        showIcon
      />
    )
  }

  const onClick: MenuProps['onClick'] = (menuItem: MenuItemType) => {
    if (menuItem.key === activeSetting) return
    setActiveSetting(menuItem.key as SettingsTab)
  }

  return (
    <>
      <br />
      <PageTitle title="Settings" />
      <Divider />
      <Layout>
        <Card size="small">
          <Sider>
            <Menu
              mode="inline"
              style={{
                borderRight: 0,
              }}
              onClick={onClick}
              items={menuItems}
            />
          </Sider>
        </Card>

        <Content style={{ paddingLeft: '12px' }}>
          <Card size="small">
            <Title level={4}>Settings</Title>
          </Card>
        </Content>
      </Layout>
    </>
  )
}

export default SettingsPage
