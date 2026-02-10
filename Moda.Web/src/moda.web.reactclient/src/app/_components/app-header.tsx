'use client'

import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  MenuOutlined,
} from '@ant-design/icons'
import React, { FC, useMemo, useState } from 'react'
import { Layout, Button, Typography, Flex, Drawer, Menu } from 'antd'
import useMenuToggle from '../../components/contexts/menu-toggle'
import { useMediaQuery } from 'react-responsive'
import { useAppMenuItems } from './menu'
import ProfileMenu from './profile-menu'
import { ItemType, MenuItemType } from 'antd/es/menu/interface'
import { useRouter } from 'next/navigation'

const { Header } = Layout
const { Title } = Typography

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
  const isMobile = useMediaQuery({ maxWidth: 768 })
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

  const handleMenuClick = (e: any) => {
    // Find the clicked item and navigate if it has a route
    const findItem = (items: any[], key: string): any => {
      for (const item of items) {
        if (item?.key === key) return item
        if (item?.children) {
          const found = findItem(item.children, key)
          if (found) return found
        }
      }
      return null
    }

    const clickedItem = findItem(mobileMenuItems, e.key)
    if (clickedItem?.label?.props?.href) {
      router.push(clickedItem.label.props.href)
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
