'use client'

import { Menu } from 'antd'
import { CSSProperties, FC, memo, useState } from 'react'
import { usePathname } from 'next/navigation'
import { useAppMenuItems } from '.'
import { findMenuKeysByPathname } from './menu-helper'
import useMenuToggle from '../../../components/contexts/menu-toggle'

interface AppMenuProps {
  style?: CSSProperties
  theme?: 'light' | 'dark'
}

interface AppMenuInnerProps extends AppMenuProps {
  menuCollapsed: boolean
  selectedKeys: string[]
  derivedOpenKeys: string[]
  menuItems: ReturnType<typeof useAppMenuItems>['menuItems']
}

const AppMenuInner: FC<AppMenuInnerProps> = memo(
  ({ style, theme: menuTheme, menuCollapsed, selectedKeys, derivedOpenKeys, menuItems }) => {
    const [openKeys, setOpenKeys] = useState<string[]>(derivedOpenKeys)

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
  },
)

AppMenuInner.displayName = 'AppMenuInner'

const AppMenu: FC<AppMenuProps> = memo(({ style, theme: menuTheme }) => {
  const { menuItems, routeKeyMap } = useAppMenuItems()
  const { menuCollapsed } = useMenuToggle()
  const pathname = usePathname()

  const { selectedKeys, openKeys } = findMenuKeysByPathname(pathname, routeKeyMap)

  return (
    <AppMenuInner
      key={pathname}
      style={style}
      theme={menuTheme}
      menuCollapsed={menuCollapsed}
      selectedKeys={selectedKeys}
      derivedOpenKeys={openKeys}
      menuItems={menuItems}
    />
  )
})

AppMenu.displayName = 'AppMenu'

export default AppMenu
