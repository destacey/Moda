'use client'

import { Button, Result, Typography } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'

const { Paragraph } = Typography

interface ServiceUnavailablePageProps {
  onRetry?: () => void
}

export default function ServiceUnavailablePage({
  onRetry,
}: ServiceUnavailablePageProps) {
  const handleRetry = () => {
    if (onRetry) {
      onRetry()
    } else {
      // Default behavior: reload the page
      window.location.reload()
    }
  }

  return (
    <Result
      status="error"
      title="Service Unavailable"
      subTitle="Moda is unable to connect to the server. Please try again later."
      extra={[
        <Button
          type="primary"
          key="retry"
          icon={<ReloadOutlined />}
          onClick={handleRetry}
        >
          Retry
        </Button>,
      ]}
    >
      <div className="desc">
        <Paragraph>
          The application is currently unable to reach the server. This could be
          due to:
        </Paragraph>
        <ul>
          <li>The server is temporarily unavailable</li>
          <li>Network connectivity issues</li>
          <li>The server is undergoing maintenance</li>
        </ul>
        <Paragraph>
          If this issue persists, please contact your administrator.
        </Paragraph>
      </div>
    </Result>
  )
}
