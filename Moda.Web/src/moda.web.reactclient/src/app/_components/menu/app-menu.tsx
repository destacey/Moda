'use client'

import { Menu } from 'antd'
import { CSSProperties, FC, memo, useMemo } from 'react'
import { usePathname } from 'next/navigation'
import { useAppMenuItems } from '.'
import { findMenuKeysByPathname } from './menu-helper'

interface AppMenuProps {
  style?: CSSProperties
  theme?: 'light' | 'dark'
}

const AppMenu: FC<AppMenuProps> = memo(({ style, theme: menuTheme }) => {
  const { menuItems, routeKeyMap } = useAppMenuItems()
  const pathname = usePathname()

  const { selectedKeys } = useMemo(
    () => findMenuKeysByPathname(pathname, routeKeyMap),
    [pathname, routeKeyMap],
  )

  return (
    <Menu
      mode="inline"
      theme={menuTheme}
      style={style}
      items={menuItems}
      selectedKeys={selectedKeys}
    />
  )
})

AppMenu.displayName = 'AppMenu'

export default AppMenu
