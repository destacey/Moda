import Profile from '../Profile'
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons'
import React from 'react'
import { Layout, Button, Typography, Flex } from 'antd'
import useMenuToggle from '../contexts/menu-toggle'
import { useMediaQuery } from 'react-responsive'

const { Header } = Layout
const { Title } = Typography

export default function AppHeader() {
  const { menuCollapsed, setMenuCollapsed } = useMenuToggle()

  const isXXS = useMediaQuery({ maxWidth: 480 })

  return (
    <>
      <Header
        style={{
          height: isXXS ? 100 : 50, // TODO: still isn't great
          display: 'flex',
          //justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        <Flex
          align="center"
          justify="space-between"
          wrap
          style={{ width: '100%' }}
        >
          <Flex>
            <Button
              type="text"
              shape="default"
              size="middle"
              icon={
                menuCollapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />
              }
              onClick={() => setMenuCollapsed(!menuCollapsed)}
            />
            <Title
              style={{
                margin: 0,
                fontSize: 24,
                fontWeight: 400,
              }}
            >
              Moda
            </Title>
          </Flex>
          <Profile />
        </Flex>
      </Header>
    </>
  )
}
