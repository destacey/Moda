import { Header } from 'antd/es/layout/layout'
import Profile from '../Profile'
import useTheme from '../contexts/theme'
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons'
import React from 'react'
import { Button, Space, Typography } from 'antd'
import useMenuToggle from '../contexts/menu-toggle'

export default function AppHeader() {
  const { menuCollapsed, setMenuCollapsed } = useMenuToggle()
  const { appBarColor } = useTheme()

  return (
    <>
      <Header
        style={{
          height: 50,
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          backgroundColor: appBarColor,
        }}
      >
        <Space>
          <Button
            type="text"
            shape="default"
            size="middle"
            icon={menuCollapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={() => setMenuCollapsed(!menuCollapsed)}
          />
          <Typography.Title
            style={{ margin: 0, fontSize: 24, fontWeight: 400 }}
          >
            Moda
          </Typography.Title>
        </Space>
        <Profile />
      </Header>
    </>
  )
}
