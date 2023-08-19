import {
  TeamOutlined,
  HomeOutlined,
  SettingOutlined,
  ScheduleOutlined,
} from '@ant-design/icons'
import { Layout, Menu } from 'antd'
import { useLayoutEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import {
  ItemType,
  MenuItemGroupType,
  MenuItemType,
} from 'antd/es/menu/hooks/useItems'
import {
  Item,
  MenuItem,
  filterAndTransformMenuItem,
  menuItem,
  restrictedMenuItem,
} from './menu-helper'
import useMenuToggle from '../../contexts/menu-toggle'
import useTheme from '../../contexts/theme'

const { Sider } = Layout

const menu: (Item | MenuItem)[] = [
  menuItem('Home', 'home', '/', <HomeOutlined />),
  menuItem('Organizations', 'org', null, <TeamOutlined />, [
    menuItem('Teams', 'org.teams', '/organizations/teams'),
    menuItem('Employees', 'org.employees', '/organizations/employees'),
  ]),
  menuItem('Planning', 'plan', null, <ScheduleOutlined />, [
    menuItem(
      'Program Increments',
      'plan.program-increments',
      '/planning/program-increments',
    ),
    // menuItem('Iterations', 'plan.iterations'),
    // menuItem('Sprints', 'plan.sprints'),
  ]),
  // menuItem('Products', 'pdc', null, <DesktopOutlined />, [
  //     menuItem('Product Lines', 'pdc.product-lines'),
  //     menuItem('Product Types', 'pdc.product-types'),
  //     menuItem('Products', 'pdc.products'),
  //     { type: 'divider' },
  //     menuItem('Releases', 'pdc.releases'),
  //     menuItem('Roadmaps', 'pdc.roadmaps'),
  //     { type: 'divider' },
  //     menuItem('Requirement Management', 'pdc.requirement-management'),
  // ]),
  // menuItem('Projects', 'ppm', null, <ProjectOutlined />, [
  //     menuItem('Portfolios', 'ppm.portfolios'),
  //     menuItem('Programs', 'ppm.programs'),
  //     menuItem('Projects', 'ppm.projects'),
  // ]),
  { key: 'settings-divider', type: 'divider' },
  menuItem('Settings', 'settings', null, <SettingOutlined />, [
    restrictedMenuItem(
      'Permissions.Users.View',
      'Permission',
      'Users',
      'settings.users',
      '/settings/users',
    ),
    restrictedMenuItem(
      'Permissions.Roles.View',
      'Permission',
      'Roles',
      'settings.roles',
      '/settings/roles',
    ),
    restrictedMenuItem(
      'Permissions.BackgroundJobs.View',
      'Permission',
      'Background Jobs',
      'settings.background-jobs',
      '/settings/background-jobs',
    ),
  ]),
]

export default function AppMenu() {
  const { menuCollapsed } = useMenuToggle()
  const [menuItems, setMenuItems] = useState<ItemType<MenuItemType>[]>([])
  const { currentThemeName } = useTheme()
  const { hasClaim } = useAuth()

  useLayoutEffect(() => {
    // Reduce the menu items based on the user's claims and transformed into antd menu items using the getItem function
    const filteredMenu = menu.reduce(
      (acc, item) => filterAndTransformMenuItem(acc, item, hasClaim),
      [] as ItemType<MenuItemType>[],
    )

    const settingsMenuItem = filteredMenu.find(
      (item) => item.key === 'settings',
    ) as MenuItemGroupType

    setMenuItems(
      settingsMenuItem.children?.length > 0
        ? filteredMenu
        : filteredMenu.filter(
            (item) =>
              item.key !== 'settings' && item.key !== 'settings-divider',
          ),
    )
  }, [hasClaim])

  return (
    <Sider
      theme={currentThemeName} // without this the menu displays weird faint lines on the left side below the menu items
      width={235}
      collapsedWidth={50}
      collapsed={menuCollapsed}
    >
      <Menu mode="inline" style={{ minHeight: '100%' }} items={menuItems} />
    </Sider>
  )
}
