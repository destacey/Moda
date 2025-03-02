'use client'

import {
  AuthenticatedTemplate,
  UnauthenticatedTemplate,
} from '@azure/msal-react'
import { Avatar, Button, Dropdown, Space, Typography } from 'antd'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { useRouter } from 'next/navigation'
import useAuth from '../../components/contexts/auth'
import { useMemo, useEffect, useState } from 'react'
import useThemeToggleMenuItem from '../../hooks/theme/use-theme-toggle-menu-item'

const { Text } = Typography

const Profile = () => {
  const { login, logout, user, isLoading } = useAuth()
  const router = useRouter()
  const themeToggleMenuItem = useThemeToggleMenuItem()

  // Ensure UI updates when authentication state changes (multi-tab sync)
  const [userState, setUserState] = useState(user)

  useEffect(() => {
    setUserState(user)
  }, [user]) // Reactively update profile when authentication state changes

  const handleLogout = async () => {
    try {
      await logout()
    } catch (e) {
      console.error(`logoutRedirect failed: ${e}`)
    }
  }

  const handleLogin = async () => {
    if (!user?.isAuthenticated) {
      try {
        await login()
      } catch (e) {
        console.error(`loginRedirect failed: ${e}`)
      }
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
    if (action) {
      action()
    } else {
      console.warn(`No action found for menu key: ${info.key}`)
    }
  }

  return (
    <>
      <AuthenticatedTemplate>
        <Space>
          {isLoading ? (
            <Text>Loading user info...</Text>
          ) : (
            userState?.name && <Text>Welcome, {userState.name}</Text>
          )}
          <Dropdown
            menu={{ items: menuItems, onClick: handleMenuItemClicked }}
            trigger={['click']}
          >
            <Avatar icon={<UserOutlined />} />
          </Dropdown>
        </Space>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <Space>
          <Text>Unauthenticated</Text>
          <Button onClick={handleLogin}>Login</Button>
        </Space>
      </UnauthenticatedTemplate>
    </>
  )
}

export default Profile
