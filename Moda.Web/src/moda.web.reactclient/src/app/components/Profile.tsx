'use client'

import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from '@azure/msal-react'
import { Avatar, Button, Dropdown, Space, Typography } from 'antd'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { createElement } from 'react'
import { useRouter } from 'next/navigation'
import useAuth from './contexts/auth'
import ThemeToggle from './common/theme/theme-toggle'

const { Text } = Typography

export default function Profile() {
  const { login, logout, user, isLoading } = useAuth()
  const router = useRouter()

  const handleLogout = () => {
    logout().catch((e) => {
      console.error(`logoutRedirect failed: ${e}`)
    })
  }

  const handleLogin = async () => {
    if (!user.isAuthenticated) {
      await login().catch((e) => {
        console.error(`loginRedirect failed: ${e}`)
      })
    }
  }

  function WelcomeUser() {
    if (isLoading) {
      return <Text>Loading user info...</Text>
    }
    return user.name && user.name.trim() ? (
      <Text>Welcome, {user.name}</Text>
    ) : null
  }

  const menuItems = [
    { key: 'profile', label: 'Account', icon: createElement(UserOutlined) },
    //{ key: 'theme', label: 'Theme', icon: themeIcon },
    ThemeToggle({}),
    { key: 'logout', label: 'Logout', icon: createElement(LogoutOutlined) },
  ]

  const handleMenuItemClicked = (info: any) => {
    switch (info.key) {
      case 'profile':
        router.push('/account/profile')
        break
      case 'logout':
        handleLogout()
    }
  }

  const authTemplate = () => {
    return (
      <AuthenticatedTemplate>
        <Space>
          <WelcomeUser />
          <Dropdown menu={{ items: menuItems, onClick: handleMenuItemClicked }}>
            <Avatar icon={<UserOutlined />} />
          </Dropdown>
        </Space>
      </AuthenticatedTemplate>
    )
  }

  const noAuthTemplate = () => {
    return (
      <UnauthenticatedTemplate>
        <Space>
          <div>Unauthenticated</div>
          <Button onClick={handleLogin}>Login</Button>
        </Space>
      </UnauthenticatedTemplate>
    )
  }

  return (
    <>
      {authTemplate()}
      {noAuthTemplate()}
    </>
  )
}
