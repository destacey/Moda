'use client'

import { Menu } from 'antd'
import { CSSProperties, FC, memo } from 'react'
import { useAppMenuItems } from '.'

interface AppMenuProps {
  style?: CSSProperties
  theme?: 'light' | 'dark'
}

const AppMenu: FC<AppMenuProps> = memo(({ style, theme: menuTheme }) => {
  const menuItems = useAppMenuItems()
  return (
    <Menu mode="inline" theme={menuTheme} style={style} items={menuItems} />
  )
})

AppMenu.displayName = 'AppMenu'

export default AppMenu
