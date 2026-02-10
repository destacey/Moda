'use client'

import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  MenuOutlined,
} from '@ant-design/icons'
import React, { FC, useMemo, useState } from 'react'
import {
  Layout,
  Button,
  Typography,
  Flex,
  Drawer,
  Menu,
  MenuProps,
  Grid,
} from 'antd'
import useMenuToggle from '../../components/contexts/menu-toggle'
import { useAppMenuItems } from './menu'
import ProfileMenu from './profile-menu'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import { useRouter } from 'next/navigation'

const { Header } = Layout
const { Title } = Typography
const { useBreakpoint } = Grid

// Menu item type with custom route property
type MenuItemWithRoute = {
  route?: string
  children?: MenuItemWithRoute[]
  [key: string]: any
}

// Flatten nested menu items for mobile display
const flattenMenuItems = (
  items: ItemType<MenuItemType>[],
): ItemType<MenuItemType>[] => {
  const flattened: ItemType<MenuItemType>[] = []

  items.forEach((item) => {
    if (!item || typeof item === 'string') return

    // Skip dividers
    if ('type' in item && item.type === 'divider') return

    const menuItem = item as any

    // If item has children, add as a group with children
    if (menuItem.children && menuItem.children.length > 0) {
      // Add as menu group
      flattened.push({
        type: 'group',
        label: menuItem.label?.props?.children || menuItem.label,
        key: `${menuItem.key}-group`,
        children: menuItem.children,
      })
    } else {
      // Add regular item
      flattened.push(menuItem)
    }
  })

  return flattened
}

const AppHeader: FC = React.memo(() => {
  const { menuCollapsed, setMenuCollapsed } = useMenuToggle()
  const screens = useBreakpoint()
  const isMobile = !screens.md // Mobile/tablet for md and below (< 768px)
  const [drawerOpen, setDrawerOpen] = useState(false)
  const menuItems = useAppMenuItems()
  const router = useRouter()

  // Flatten menu for mobile
  const mobileMenuItems = useMemo(
    () => flattenMenuItems(menuItems),
    [menuItems],
  )

  // Avoid re-renders when authentication state changes
  const profileComponent = useMemo(() => <ProfileMenu />, [])

  const handleMenuClick: MenuProps['onClick'] = (e) => {
    // Find the clicked item and navigate using the custom route property
    const findItem = (items: any[], key: string): MenuItemWithRoute | null => {
      for (const item of items) {
        if (!item || typeof item === 'string') continue

        if (item.key === key) return item as MenuItemWithRoute

        if (item.children) {
          const found = findItem(item.children, key)
          if (found) return found
        }
      }
      return null
    }

    const clickedItem = findItem(mobileMenuItems, e.key)
    if (clickedItem?.route) {
      router.push(clickedItem.route)
      setDrawerOpen(false)
    }
  }

  return (
    <Header
      style={{
        height: 50,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: '0 16px',
      }}
    >
      <Flex>
        {isMobile ? (
          <>
            <Button
              type="text"
              icon={<MenuOutlined />}
              size="middle"
              onClick={() => setDrawerOpen(true)}
              aria-label="Open navigation menu"
            />
            <Drawer
              title="Menu"
              placement="left"
              onClose={() => setDrawerOpen(false)}
              open={drawerOpen}
              width={280}
              styles={{
                body: { padding: 0 },
              }}
            >
              <Menu
                mode="inline"
                items={mobileMenuItems}
                onClick={handleMenuClick}
                style={{ border: 'none' }}
              />
            </Drawer>
          </>
        ) : (
          <Button
            type="text"
            icon={menuCollapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            size="middle"
            onClick={() => setMenuCollapsed(!menuCollapsed)}
            aria-label={menuCollapsed ? 'Expand sidebar menu' : 'Collapse sidebar menu'}
          />
        )}
        <Title
          level={4}
          style={{
            margin: '0 16px',
            fontSize: 24,
            fontWeight: 400,
            whiteSpace: 'nowrap',
          }}
        >
          Moda
        </Title>
      </Flex>
      {profileComponent}
    </Header>
  )
})

AppHeader.displayName = 'AppHeader'

export default AppHeader
