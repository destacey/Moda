import { Result } from 'antd'

const NotAuthorized = () => {
  return (
    <Result
      status="403"
      title="403"
      subTitle="Sorry, you are not authorized to access this page."
    />
  )
}

export default NotAuthorized
