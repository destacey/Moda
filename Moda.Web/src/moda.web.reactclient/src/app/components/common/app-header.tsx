import Profile from '../Profile'
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons'
import React from 'react'
import { Layout, Button, Space, Typography } from 'antd'
import useMenuToggle from '../contexts/menu-toggle'

const { Header } = Layout

export default function AppHeader() {
  const { menuCollapsed, setMenuCollapsed } = useMenuToggle()

  return (
    <>
      <Header
        style={{
          height: 50,
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
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
