import { MenuProps } from 'antd'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import Link from 'next/link'
import { Key, ReactNode } from 'react'

export type MenuItem = Required<MenuProps>['items'][number]

export interface Item {
  display: string
  key: string
  route?: string
  icon?: ReactNode
  children?: (Item | MenuItem)[]
  authClaimValue?: string
  authClaimType?: string
  restrictedSection?: boolean
}

export const menuItem = (
  display: string,
  key: string,
  route?: string,
  icon?: ReactNode,
  children?: (Item | MenuItem)[],
  restrictedSection?: boolean,
) => {
  return {
    display,
    key,
    route,
    icon,
    children,
    restrictedSection,
  }
}

export const restrictedMenuSection = (
  display: string,
  key: string,
  route?: string,
  icon?: ReactNode,
  children?: (Item | MenuItem)[],
) => {
  return {
    ...menuItem(display, key, route, icon, children, true),
  }
}

export const restrictedMenuItem = (
  authClaimValue: string,
  authClaimType: string,
  display: string,
  key: string,
  route?: string,
  icon?: ReactNode,
  children?: (Item | MenuItem)[],
) => {
  return {
    ...menuItem(display, key, route, icon, children),
    authClaimValue,
    authClaimType,
  }
}

export const restrictedPermissionMenuItem = (
  authClaimValue: string,
  display: string,
  key: string,
  route?: string,
  icon?: ReactNode,
  children?: Item[],
) => {
  return {
    ...menuItem(display, key, route, icon, children),
    authClaimValue,
    authClaimType: 'Permission',
  }
}

function getItem(
  label: string,
  key: Key,
  route?: string,
  icon?: ReactNode,
  children?: MenuItem[],
  type?: 'group',
): MenuItem {
  return {
    key,
    icon,
    children,
    label: route ? <Link href={route}>{label}</Link> : label,
    type,
    title: null,
    route, // Attach route as custom property for navigation
  } as MenuItem
}

export const buildRouteKeyMap = (
  items: (Item | MenuItem)[],
): Map<string, string> => {
  const map = new Map<string, string>()
  for (const item of items) {
    if ('display' in item) {
      const menuItemItem = item as Item
      if (menuItemItem.route) {
        map.set(menuItemItem.route, menuItemItem.key)
      }
      if (menuItemItem.children) {
        const childMap = buildRouteKeyMap(menuItemItem.children)
        childMap.forEach((value, key) => map.set(key, value))
      }
    }
  }
  return map
}

export const findMenuKeysByPathname = (
  pathname: string,
  routeKeyMap: Map<string, string>,
): { selectedKeys: string[]; openKeys: string[] } => {
  let bestRoute = ''
  let bestKey = ''

  for (const [route, key] of routeKeyMap) {
    if (route === '/') {
      if (pathname === '/') {
        bestRoute = route
        bestKey = key
      }
    } else if (
      pathname === route ||
      pathname.startsWith(route + '/')
    ) {
      if (route.length > bestRoute.length) {
        bestRoute = route
        bestKey = key
      }
    }
  }

  if (!bestKey) {
    return { selectedKeys: [], openKeys: [] }
  }

  const openKeys: string[] = []
  const parts = bestKey.split('.')
  if (parts.length > 1) {
    openKeys.push(parts[0])
  }

  return { selectedKeys: [bestKey], openKeys }
}

export const filterAndTransformMenuItem = (
  acc: ItemType<MenuItemType>[],
  item: Item | MenuItem,
  claimCheck: (claimType: string, claimValue: string) => boolean,
): ItemType<MenuItemType>[] => {
  if ('type' in item) {
    // For items like dividers that already have a type.
    acc.push(item)
  } else {
    const menuItemItem = item as Item
    // Safely access children now.
    const children = menuItemItem.children
      ? menuItemItem.children.reduce(
          (childAcc, child) =>
            filterAndTransformMenuItem(childAcc, child, claimCheck),
          [] as ItemType<MenuItemType>[],
        )
      : undefined

    if ('authClaimValue' in menuItemItem && menuItemItem.authClaimType) {
      if (claimCheck(menuItemItem.authClaimType, menuItemItem.authClaimValue)) {
        if (
          menuItemItem.restrictedSection &&
          (!children || children.length === 0)
        ) {
          // Skip the top-level restricted section if no children pass the check.
        } else {
          acc.push(
            getItem(
              menuItemItem.display,
              menuItemItem.key,
              menuItemItem.route,
              menuItemItem.icon,
              children,
            ),
          )
        }
      }
    } else if ('display' in menuItemItem) {
      if (
        menuItemItem.restrictedSection &&
        (!children || children.length === 0)
      ) {
        // Skip the top-level restricted section if no children pass the check.
      } else {
        acc.push(
          getItem(
            menuItemItem.display,
            menuItemItem.key,
            menuItemItem.route,
            menuItemItem.icon,
            children,
          ),
        )
      }
    }
  }
  return acc
}
