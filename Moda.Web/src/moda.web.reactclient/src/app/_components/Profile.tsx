'use client'

import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from '@azure/msal-react'
import { Avatar, Button, Dropdown, Space, Typography } from 'antd'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { useRouter } from 'next/navigation'
import useAuth from '../../components/contexts/auth'
import { useMemo } from 'react'
import useThemeToggleMenuItem from '../../hooks/theme/use-theme-toggle-menu-item'

const { Text } = Typography

const Profile = () => {
  const { login, logout, user, isLoading } = useAuth()
  const router = useRouter()
  const themeToggleMenuItem = useThemeToggleMenuItem()

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

  const menuItems = useMemo(
    () => [
      { key: 'profile', label: 'Account', icon: <UserOutlined /> },
      themeToggleMenuItem,
      { key: 'logout', label: 'Logout', icon: <LogoutOutlined /> },
    ],
    [themeToggleMenuItem],
  )

  const menuActions: Record<string, () => void> = {
    profile: () => router.push('/account/profile'),
    logout: handleLogout,
  }

  const handleMenuItemClicked = (info: { key: string }) => {
    const action = menuActions[info.key]
    if (action) action()
  }

  return (
    <>
      <AuthenticatedTemplate>
        <Space>
          {isLoading ? (
            <Text>Loading user info...</Text>
          ) : (
            user.name?.trim() && <Text>Welcome, {user.name}</Text>
          )}
          <Dropdown menu={{ items: menuItems, onClick: handleMenuItemClicked }}>
            <Avatar icon={<UserOutlined />} />
          </Dropdown>
        </Space>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <Space>
          <div>Unauthenticated</div>
          <Button onClick={handleLogin}>Login</Button>
        </Space>
      </UnauthenticatedTemplate>
    </>
  )
}

export default Profile
