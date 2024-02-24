import { Spin } from 'antd'
import useAuth from '../contexts/auth/use-auth'

const LoadingAccount = ({ children }: { children: React.ReactNode }) => {
  const { isLoading } = useAuth()
  return (
    <Spin spinning={isLoading} tip="Loading account..." size="large">
      {children}
    </Spin>
  )
}

export default LoadingAccount
