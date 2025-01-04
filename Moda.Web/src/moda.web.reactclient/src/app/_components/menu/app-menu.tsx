'use client'

import { Menu } from 'antd'
import { CSSProperties, FC, useMemo } from 'react'
import React from 'react'
import { useAppMenuItems } from '.'

interface AppMenuProps {
  style?: CSSProperties
}

const AppMenu: FC<AppMenuProps> = React.memo(({ style }) => {
  const menuItems = useAppMenuItems()
  return <Menu mode="inline" style={style} items={menuItems} />
})

AppMenu.displayName = 'AppMenu'

export default AppMenu
