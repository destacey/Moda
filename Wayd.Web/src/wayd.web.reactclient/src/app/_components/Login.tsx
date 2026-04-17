'use client'

import useAuth from '@/src/components/contexts/auth'
import { Button, Card, Flex, Typography } from 'antd'
import Image from 'next/image'

const { Title } = Typography

const Login: React.FC = () => {
  const { login, user, isLoading } = useAuth()

  const handleLogin = async () => {
    if (!user?.isAuthenticated) {
      try {
        await login()
      } catch (e) {
        console.error(`loginRedirect failed: ${e}`)
      }
    }
  }

  // Prevents login from rendering while still checking authentication state
  if (isLoading) {
    return null
  }

  return (
    <Flex
      vertical
      align="center"
      justify="center"
      style={{
        height: '100vh',
        background: 'linear-gradient(to right, #fff, #2196f3)',
      }}
    >
      <Card>
        <Flex vertical align="center" justify="center">
          <Flex align="center" justify="center">
            <Image
              src={'/moda-icon.png'}
              alt="Moda Icon"
              style={{ width: '40px', height: '40px', marginRight: '10px' }}
            />
            <Title level={2} style={{ margin: 0 }}>
              Moda
            </Title>
          </Flex>
          <br />
          <Button loading={isLoading} onClick={handleLogin}>
            Continue with Microsoft Entra
          </Button>
        </Flex>
      </Card>
    </Flex>
  )
}

export default Login
