'use client'

import { Avatar, Dropdown, MenuProps, Space, Typography } from 'antd'
import {
  FileTextOutlined,
  LogoutOutlined,
  UserOutlined,
} from '@ant-design/icons'
import { useRouter } from 'next/navigation'
import useAuth from '../../components/contexts/auth'
import { useCallback, useMemo } from 'react'
import useThemeToggleMenuItem from '../../hooks/theme/use-theme-toggle-menu-item'

const { Text } = Typography

const ProfileMenu = () => {
  const { logout, user } = useAuth()
  const router = useRouter()
  const themeToggleMenuItem = useThemeToggleMenuItem()

  const handleLogout = useCallback(async () => {
    try {
      await logout()
    } catch (e) {
      console.error(`logoutRedirect failed: ${e}`)
    }
  }, [logout])

  const menuItems: MenuProps['items'] = useMemo(
    () => [
      {
        key: 'profile',
        label: 'Account',
        icon: <UserOutlined />,
        onClick: () => router.push('/account/profile'),
      },
      themeToggleMenuItem,
      {
        key: 'divider',
        type: 'divider',
      },
      ...(process.env.NEXT_PUBLIC_API_BASE_URL
        ? [
            {
              key: 'api-spec',
              label: 'API Specification',
              icon: <FileTextOutlined />,
              onClick: () =>
                window.open(
                  `${process.env.NEXT_PUBLIC_API_BASE_URL}/swagger`,
                  '_blank',
                  'noopener,noreferrer',
                ),
            },
          ]
        : []),
      {
        key: 'logout',
        label: 'Logout',
        icon: <LogoutOutlined />,
        onClick: handleLogout,
      },
    ],
    [themeToggleMenuItem, router, handleLogout],
  )

  return (
    <>
      <Space>
        {user?.name && <Text>Welcome, {user.name}</Text>}
        <Dropdown menu={{ items: menuItems }} trigger={['click']}>
          <Avatar icon={<UserOutlined />} />
        </Dropdown>
      </Space>
    </>
  )
}

export default ProfileMenu
