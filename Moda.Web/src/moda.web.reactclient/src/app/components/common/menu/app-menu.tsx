import {
  TeamOutlined,
  HomeOutlined,
  SettingOutlined,
  ScheduleOutlined,
  CarryOutOutlined,
} from '@ant-design/icons'
import { Layout, Menu } from 'antd'
import { useMemo } from 'react'
import useAuth from '../../contexts/auth'
import {
  Item,
  MenuItem,
  filterAndTransformMenuItem,
  menuItem,
  restrictedPermissionMenuItem,
} from './menu-helper'
import useMenuToggle from '../../contexts/menu-toggle'
import useTheme from '../../contexts/theme'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import React from 'react'

const { Sider } = Layout

const menuIcons = {
  home: <HomeOutlined />,
  org: <TeamOutlined />,
  planning: <ScheduleOutlined />,
  work: <CarryOutOutlined />,
  settings: <SettingOutlined />,
}

const menu: (Item | MenuItem)[] = [
  menuItem('Home', 'home', '/', menuIcons.home),
  menuItem('Organizations', 'org', null, menuIcons.org, [
    menuItem('Teams', 'org.teams', '/organizations/teams'),
    menuItem('Employees', 'org.employees', '/organizations/employees'),
    { key: 'org-settings-divider-1', type: 'divider' },
    menuItem(
      'Functional Org Chart',
      'org.functional-org-chart',
      '/organizations/functional-org-chart',
    ),
  ]),
  menuItem('Planning', 'plan', null, menuIcons.planning, [
    restrictedPermissionMenuItem(
      'Permissions.PlanningIntervals.View',
      'Planning Intervals',
      'plan.planning-intervals',
      '/planning/planning-intervals',
    ),
    restrictedPermissionMenuItem(
      'Permissions.Roadmaps.View',
      'Roadmaps',
      'plan.roadmaps',
      '/planning/roadmaps',
    ),
    // menuItem('Increments', 'plan.increments'),
    // menuItem('Sprints', 'plan.sprints'),
  ]),
  menuItem('Work Management', 'work', null, menuIcons.work, [
    restrictedPermissionMenuItem(
      'Permissions.Workspaces.View',
      'Workspaces',
      'work.workspaces',
      '/work/workspaces',
    ),
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
  menuItem('Settings', 'settings', '/settings', menuIcons.settings),
]

const AppMenu = React.memo(() => {
  const { menuCollapsed } = useMenuToggle()
  const { currentThemeName } = useTheme()
  const { hasClaim } = useAuth()

  const filteredMenuItems = useMemo(
    () =>
      menu.reduce(
        (acc, item) => filterAndTransformMenuItem(acc, item, hasClaim),
        [] as ItemType<MenuItemType>[],
      ),
    [hasClaim],
  )

  const menuStyle = useMemo(() => ({ minHeight: '100%' }), [])
  const siderTheme = useMemo(() => currentThemeName, [currentThemeName])

  return (
    <Sider
      theme={siderTheme}
      width={235}
      collapsedWidth={50}
      collapsed={menuCollapsed}
    >
      <Menu mode="inline" style={menuStyle} items={filteredMenuItems} />
    </Sider>
  )
})

AppMenu.displayName = 'AppMenu'

export default AppMenu
