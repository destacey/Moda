'use client'

import { Menu } from 'antd'
import { CSSProperties, FC, memo, useState, useEffect, useMemo } from 'react'
import { usePathname } from 'next/navigation'
import { useAppMenuItems } from '.'
import { findMenuKeysByPathname } from './menu-helper'
import useMenuToggle from '../../../components/contexts/menu-toggle'

interface AppMenuProps {
  style?: CSSProperties
  theme?: 'light' | 'dark'
}

const AppMenu: FC<AppMenuProps> = memo(({ style, theme: menuTheme }) => {
  const { menuItems, routeKeyMap } = useAppMenuItems()
  const { menuCollapsed } = useMenuToggle()
  const pathname = usePathname()

  const { selectedKeys, openKeys: derivedOpenKeys } = useMemo(
    () => findMenuKeysByPathname(pathname, routeKeyMap),
    [pathname, routeKeyMap],
  )

  const [openKeys, setOpenKeys] = useState<string[]>(derivedOpenKeys)

  // When pathname changes, update openKeys only if menu is expanded
  useEffect(() => {
    if (!menuCollapsed) {
      setOpenKeys(derivedOpenKeys)
    }
  }, [derivedOpenKeys, menuCollapsed])

  const openKeyProps = menuCollapsed
    ? {}
    : { openKeys, onOpenChange: setOpenKeys }

  return (
    <Menu
      mode="inline"
      theme={menuTheme}
      style={style}
      items={menuItems}
      selectedKeys={selectedKeys}
      {...openKeyProps}
    />
  )
})

AppMenu.displayName = 'AppMenu'

export default AppMenu
