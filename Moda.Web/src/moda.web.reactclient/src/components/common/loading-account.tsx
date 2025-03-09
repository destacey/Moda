import { Spin } from 'antd'

const LoadingAccount = () => {
  return (
    <Spin tip="Loading Moda user's account..." size="large">
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
