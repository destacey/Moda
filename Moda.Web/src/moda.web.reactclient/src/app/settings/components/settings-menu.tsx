'use client'

import {
  ItemType,
  MenuItemGroupType,
  MenuItemType,
  SubMenuType,
} from 'antd/es/menu/hooks/useItems'
import useAuth from '../../components/contexts/auth'
import { useEffect, useState } from 'react'
import { Menu } from 'antd'
import Link from 'next/link'

enum SettingsTab {
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
  route?: string,
  children?: SubMenuType[],
): RestrictedMenuItem => {
  return {
    label: route ? <Link href={route}>{label}</Link> : label,
    key,
    route,
    children,
    claimValue,
  } as RestrictedMenuItem
}

const getRegularMenuItem = (
  label: React.ReactNode,
  key: string,
  route?: string,
  children?: SubMenuType[],
): SubMenuType => {
  return {
    label: route ? <Link href={route}>{label}</Link> : label,
    key,
    route,
    children,
  } as SubMenuType
}

const settingsMenuItems: ItemType[] = [
  getSectionMenuItem('User Management', 'user-management', [
    getRestrictedMenuItem(
      'Permissions.Users.View',
      'Users',
      SettingsTab.Users,
      '/settings/user-management/users',
    ),
    getRestrictedMenuItem(
      'Permissions.Roles.View',
      'Roles',
      SettingsTab.Roles,
      '/settings/user-management/roles',
    ),
  ]),

  getSectionMenuItem('Work Management', 'work-management', [
    getRegularMenuItem(
      'Work Types',
      SettingsTab.WorkTypes,
      '/settings/work-management/work-types',
    ),
    getRegularMenuItem(
      'Work Statuses',
      SettingsTab.WorkStatuses,
      '/settings/work-management/work-statuses',
    ),
    getRegularMenuItem(
      'Work Processes',
      SettingsTab.WorkProcesses,
      '/settings/work-management/work-processes',
    ),
  ]),

  getSectionMenuItem('Integrations', 'integration-management', [
    getRestrictedMenuItem(
      'Permissions.Connections.View',
      'Connections',
      SettingsTab.Connections,
      '/settings/connections',
    ),
  ]),

  getSectionMenuItem('Other', 'other', [
    getRestrictedMenuItem(
      'Permissions.BackgroundJobs.View',
      'Background Jobs',
      SettingsTab.BackgroundJobs,
      '/settings/background-jobs',
    ),
  ]),
]

// TODO: improve style and layout for smaller screens
export default function SettingsMenu() {
  const [menuItems, setMenuItems] = useState<ItemType<MenuItemType>[]>([])
  const [activeSetting, setActiveSetting] = useState()
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

  return (
    <Menu
      mode="inline"
      style={{
        borderRight: 0,
      }}
      items={menuItems}
    />
  )
}
