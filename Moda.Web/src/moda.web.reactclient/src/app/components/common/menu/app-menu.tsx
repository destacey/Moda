import {
  TeamOutlined,
  HomeOutlined,
  SettingOutlined,
  ScheduleOutlined,
  CarryOutOutlined,
} from '@ant-design/icons'
import { Layout, Menu } from 'antd'
import { useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import {
  Item,
  MenuItem,
  filterAndTransformMenuItem,
  menuItem,
} from './menu-helper'
import useMenuToggle from '../../contexts/menu-toggle'
import useTheme from '../../contexts/theme'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'

const { Sider } = Layout

const menu: (Item | MenuItem)[] = [
  menuItem('Home', 'home', '/', <HomeOutlined />),
  menuItem('Organizations', 'org', null, <TeamOutlined />, [
    menuItem('Teams', 'org.teams', '/organizations/teams'),
    menuItem('Employees', 'org.employees', '/organizations/employees'),
  ]),
  menuItem('Planning', 'plan', null, <ScheduleOutlined />, [
    menuItem(
      'Planning Intervals',
      'plan.planning-intervals',
      '/planning/planning-intervals',
    ),
    // menuItem('Increments', 'plan.increments'),
    // menuItem('Sprints', 'plan.sprints'),
  ]),
  menuItem('Work Management', 'work', null, <CarryOutOutlined />, [
    menuItem('Workspaces', 'work.workspaces', '/work/workspaces'),
  ]),
  // menuItem('Products', 'pdc', null, <DesktopOutlined />, [
  //     menuItem('Product Lines', 'pdc.product-lines'),
  //     menuItem('Product Types', 'pdc.product-types'),
  //     menuItem('Products', 'pdc.products'),
  //     { type: 'divider' },
  //     menuItem('Releases', 'pdc.releases'),
  //     menuItem('Roadmaps', 'pdc.roadmaps'),
  //     { type: 'divider' },
  //     menuItem('Requirements Management', 'pdc.requirements-management'),
  // ]),
  // menuItem('Projects', 'ppm', null, <ProjectOutlined />, [
  //     menuItem('Portfolios', 'ppm.portfolios'),
  //     menuItem('Programs', 'ppm.programs'),
  //     menuItem('Projects', 'ppm.projects'),
  // ]),
  { key: 'settings-divider', type: 'divider' },
  menuItem('Settings', 'settings', '/settings', <SettingOutlined />),
]

export default function AppMenu() {
  const { menuCollapsed } = useMenuToggle()
  const [menuItems, setMenuItems] = useState<ItemType<MenuItemType>[]>([])
  const { currentThemeName } = useTheme()
  const { hasClaim } = useAuth()

  useEffect(() => {
    // Reduce the menu items based on the user's claims and transformed into antd menu items using the getItem function
    setMenuItems(
      menu.reduce(
        (acc, item) => filterAndTransformMenuItem(acc, item, hasClaim),
        [] as ItemType<MenuItemType>[],
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
