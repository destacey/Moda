'use client'

import { Menu } from 'antd'
import { usePathname, useRouter } from 'next/navigation'
import { useMemo } from 'react'
import type { DocNavItem } from '@/src/services/docs'
import type { MenuProps } from 'antd'

type MenuItem = Required<MenuProps>['items'][number]

interface DocsSidebarProps {
  navigation: DocNavItem[]
}

function buildMenuItems(items: DocNavItem[]): MenuItem[] {
  return items.map((item) => {
    if (item.children && item.children.length > 0) {
      return {
        key: `/docs/${item.slug}`,
        label: item.title,
        children: buildMenuItems(item.children),
      }
    }
    return {
      key: `/docs/${item.slug}`,
      label: item.title,
    }
  })
}

function findOpenKeys(
  items: DocNavItem[],
  pathname: string,
  parentKeys: string[] = [],
): string[] {
  for (const item of items) {
    const key = `/docs/${item.slug}`
    if (pathname.startsWith(key)) {
      if (item.children) {
        return [
          ...parentKeys,
          key,
          ...findOpenKeys(item.children, pathname, [...parentKeys, key]),
        ]
      }
      return parentKeys
    }
  }
  return []
}

export default function DocsSidebar({ navigation }: DocsSidebarProps) {
  const pathname = usePathname()
  const router = useRouter()

  const menuItems = useMemo(() => buildMenuItems(navigation), [navigation])

  const defaultOpenKeys = useMemo(
    () => findOpenKeys(navigation, pathname),
    [navigation, pathname],
  )

  const handleClick: MenuProps['onClick'] = ({ key }) => {
    router.push(key)
  }

  return (
    <Menu
      mode="inline"
      selectedKeys={[pathname]}
      defaultOpenKeys={defaultOpenKeys}
      items={menuItems}
      onClick={handleClick}
      style={{ height: '100%', borderRight: 0 }}
    />
  )
}
