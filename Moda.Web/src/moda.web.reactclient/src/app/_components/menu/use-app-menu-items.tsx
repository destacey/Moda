'use client'

import {
  TeamOutlined,
  HomeOutlined,
  SettingOutlined,
  ScheduleOutlined,
  CarryOutOutlined,
  ProjectOutlined,
  FundOutlined,
} from '@ant-design/icons'
import {
  filterAndTransformMenuItem,
  Item,
  menuItem,
  MenuItem,
  restrictedPermissionMenuItem,
} from './menu-helper'
import { useMemo } from 'react'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import useAuth from '../../../components/contexts/auth'

const menuIcons = {
  home: <HomeOutlined />,
  org: <TeamOutlined />,
  planning: <ScheduleOutlined />,
  ppm: <ProjectOutlined />,
  strategy: <FundOutlined />,
  work: <CarryOutOutlined />,
  settings: <SettingOutlined />,
}

const menuItems: (Item | MenuItem)[] = [
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
  menuItem('Strategic Management', 'strategy', null, menuIcons.strategy, [
    restrictedPermissionMenuItem(
      'Permissions.StrategicThemes.View',
      'Strategic Themes',
      'strategy.strategic-themes',
      '/strategic-management/strategic-themes',
    ),
  ]),
  menuItem('PPM', 'ppm', null, menuIcons.ppm, [
    restrictedPermissionMenuItem(
      'Permissions.ProjectPortfolios.View',
      'Portfolios',
      'ppm.portfolios',
      '/ppm/portfolios',
    ),
    // restrictedPermissionMenuItem('Permissions.Programs.View', 'Programs', 'ppm.programs', '/ppm/programs'),
    restrictedPermissionMenuItem(
      'Permissions.Projects.View',
      'Projects',
      'ppm.projects',
      '/ppm/projects',
    ),
  ]),
  { key: 'settings-divider', type: 'divider' },
  menuItem('Settings', 'settings', '/settings', menuIcons.settings),
]

const useAppMenuItems = () => {
  const { hasClaim } = useAuth()

  const filteredMenuItems = useMemo(
    () =>
      menuItems.reduce(
        (acc, item) => filterAndTransformMenuItem(acc, item, hasClaim),
        [] as ItemType<MenuItemType>[],
      ),
    [hasClaim],
  )

  return filteredMenuItems
}

export default useAppMenuItems
