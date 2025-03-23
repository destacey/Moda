import { MenuProps } from 'antd'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import Link from 'next/link'

export type MenuItem = Required<MenuProps>['items'][number]

export interface Item {
  display: string
  key: string
  route?: string
  icon?: React.ReactNode
  children?: (Item | MenuItem)[]
  authClaimValue?: string
  authClaimType?: string
  restrictedSection?: boolean
}

export const menuItem = (
  display: string,
  key: string,
  route?: string,
  icon?: React.ReactNode,
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
  icon?: React.ReactNode,
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
  icon?: React.ReactNode,
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
  icon?: React.ReactNode,
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
  key: React.Key,
  route?: string,
  icon?: React.ReactNode,
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
  } as MenuItem
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
