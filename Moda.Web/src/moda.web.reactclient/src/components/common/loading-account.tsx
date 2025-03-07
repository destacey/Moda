'use client'

import { Spin } from 'antd'
import useAuth from '../contexts/auth/use-auth'

const LoadingAccount = ({ children }: { children: React.ReactNode }) => {
  const { isLoading, user } = useAuth()

  return isLoading || !user?.isAuthenticated ? (
    <Spin spinning={true} tip="Loading account..." size="large">
      <div style={{ minHeight: '100vh' }} /> {/* Prevents layout collapse */}
    </Spin>
  ) : (
    <>{children}</>
  )
}

export default LoadingAccount
