import { Spin } from 'antd'

interface LoadingAccountProps {
  message?: string
}

const LoadingAccount: React.FC<LoadingAccountProps> = (props) => {
  const message = props.message || "Loading Moda user's account..."
  return (
    <Spin tip={message} size="large">
      <div
        style={{
          minHeight: '100vh',
          background: 'linear-gradient(to right, #fff, #2196f3)',
        }}
      />
    </Spin>
  )
}

export default LoadingAccount
