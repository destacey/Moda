'use client'

import { FC, memo } from 'react'
import useMenuToggle from '../../../components/contexts/menu-toggle'
import useTheme from '../../../components/contexts/theme'
import { Layout } from 'antd'
import AppMenu from './app-menu'

const { Sider } = Layout

interface AppSideNavProps {
  isMobile?: boolean
}

const AppSideNav: FC<AppSideNavProps> = memo(
  ({ isMobile = false }: AppSideNavProps) => {
    const { menuCollapsed } = useMenuToggle()
    const { currentThemeName } = useTheme()

    // Return null for mobile since the dropdown is handled in AppHeader
    if (isMobile) {
      return null
    }

    return (
      <Sider
        className="app-side-nav"
        theme={currentThemeName === 'light' ? 'light' : 'dark'}
        width={235}
        collapsedWidth={50}
        collapsed={menuCollapsed}
      >
        <AppMenu
          theme={currentThemeName === 'light' ? 'light' : 'dark'}
          style={{ minHeight: '100%' }}
        />
      </Sider>
    )
  },
)

AppSideNav.displayName = 'AppSideNav'

export default AppSideNav
