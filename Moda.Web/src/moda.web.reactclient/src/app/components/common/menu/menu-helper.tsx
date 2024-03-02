import { MenuProps } from 'antd'
import { ItemType, MenuItemType } from 'antd/es/menu/hooks/useItems'
import Link from 'next/link'

export type MenuItem = Required<MenuProps>['items'][number]

export interface Item {
  display: string
  key: string
  route?: string
  icon?: React.ReactNode
  children?: Item[]
  authClaimValue?: string
  authClaimType?: string
}

export const menuItem = (
  display: string,
  key: string,
  route?: string,
  icon?: React.ReactNode,
  children?: Item[],
) => {
  return {
    display,
    key,
    route,
    icon,
    children,
  }
}

export const restrictedMenuItem = (
  authClaimValue: string,
  authClaimType: string,
  display: string,
  key: string,
  route?: string,
  icon?: React.ReactNode,
  children?: Item[],
) => {
  return {
    ...menuItem(display, key, route, icon, children),
    authClaimValue,
    authClaimType,
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
) => {
  if ('type' in item) {
    acc.push(item)
  } else if ('authClaimValue' in item && item.authClaimType) {
    if (claimCheck(item.authClaimType, item.authClaimValue)) {
      const children = item.children
        ? item.children.reduce(
            (acc, item) => filterAndTransformMenuItem(acc, item, claimCheck),
            [],
          )
        : undefined
      acc.push(getItem(item.display, item.key, item.route, item.icon, children))
    }
  } else if ('display' in item) {
    const children = item.children
      ? item.children.reduce(
          (acc, item) => filterAndTransformMenuItem(acc, item, claimCheck),
          [],
        )
      : undefined
    acc.push(getItem(item.display, item.key, item.route, item.icon, children))
  }
  return acc
}
