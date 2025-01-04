'use client'

import Profile from '../components/Profile'
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  MenuOutlined,
} from '@ant-design/icons'
import React, { FC, useState } from 'react'
import { Layout, Button, Typography, Dropdown, Menu, Flex } from 'antd'
import useMenuToggle from '../components/contexts/menu-toggle'
import { useMediaQuery } from 'react-responsive'
import { useAppMenuItems } from './menu'

const { Header } = Layout
const { Title } = Typography

const AppHeader: FC = React.memo(() => {
  const { menuCollapsed, setMenuCollapsed } = useMenuToggle()
  const isMobile = useMediaQuery({ maxWidth: 768 }) // Define mobile breakpoint
  const [dropdownOpen, setDropdownOpen] = useState(false)

  const menuItems = useAppMenuItems()

  const handleDropdownOpen = (open: boolean) => {
    setDropdownOpen(open)
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
          <Dropdown
            menu={{ items: menuItems }}
            trigger={['click']}
            open={dropdownOpen}
            onOpenChange={handleDropdownOpen}
            placement="bottomLeft"
          >
            <Button
              type="text"
              icon={<MenuOutlined />}
              size="middle"
              onClick={() => setDropdownOpen(!dropdownOpen)}
            />
          </Dropdown>
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
          }}
        >
          Moda
        </Title>
      </Flex>
      <Profile />
    </Header>
  )
})

AppHeader.displayName = 'AppHeader'

export default AppHeader
