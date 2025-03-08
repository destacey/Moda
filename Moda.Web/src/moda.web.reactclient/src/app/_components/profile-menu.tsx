'use client'

import { Avatar, Dropdown, Space, Typography } from 'antd'
import { LogoutOutlined, UserOutlined } from '@ant-design/icons'
import { useRouter } from 'next/navigation'
import useAuth from '../../components/contexts/auth'
import { useMemo } from 'react'
import useThemeToggleMenuItem from '../../hooks/theme/use-theme-toggle-menu-item'

const { Text } = Typography

const ProfileMenu = () => {
  const { logout, user } = useAuth()
  const router = useRouter()
  const themeToggleMenuItem = useThemeToggleMenuItem()

  const handleLogout = async () => {
    try {
      await logout()
    } catch (e) {
      console.error(`logoutRedirect failed: ${e}`)
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
      <Space>
        {user?.name && <Text>Welcome, {user.name}</Text>}
        <Dropdown
          menu={{ items: menuItems, onClick: handleMenuItemClicked }}
          trigger={['click']}
        >
          <Avatar icon={<UserOutlined />} />
        </Dropdown>
      </Space>
    </>
  )
}

export default ProfileMenu
