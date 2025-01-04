'use client'

import React, { FC, useMemo } from 'react'
import useMenuToggle from '../../../components/contexts/menu-toggle'
import useTheme from '../../../components/contexts/theme'
import { Layout } from 'antd'
import AppMenu from './app-menu'

const { Sider } = Layout

interface AppSideNavProps {
  isMobile?: boolean
}

const AppSideNav: FC<AppSideNavProps> = React.memo(
  ({ isMobile = false }: AppSideNavProps) => {
    const { menuCollapsed } = useMenuToggle()
    const { currentThemeName } = useTheme()

    const siderTheme = useMemo(() => currentThemeName, [currentThemeName])

    // Return null for mobile since the dropdown is handled in AppHeader
    if (isMobile) {
      return null
    }

    return (
      <Sider
        className="app-side-nav"
        theme={siderTheme}
        width={235}
        //style={{ height: '100%' }}
        collapsedWidth={50}
        collapsed={menuCollapsed}
      >
        <AppMenu style={{ minHeight: '100%' }} />
      </Sider>
    )
  },
)

AppSideNav.displayName = 'AppSideNav'

export default AppSideNav
