import { Spin } from 'antd'
import { AuthenticatedTemplate } from '@azure/msal-react'
import useAuth from '../contexts/auth/use-auth'

const LoadingAccount = ({ children }: { children: React.ReactNode }) => {
  const { isLoading } = useAuth()

  return (
    <Spin spinning={isLoading} tip="Loading account..." size="large">
      <AuthenticatedTemplate>{children}</AuthenticatedTemplate>
    </Spin>
  )
}

export default LoadingAccount
