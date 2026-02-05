'use client'

import { useEffect, useState } from 'react'
import { Button, Result, Space, Typography } from 'antd'
import { LogoutOutlined, ReloadOutlined } from '@ant-design/icons'

const { Paragraph } = Typography

interface ServiceUnavailablePageProps {
  onRetry?: () => void
  onLogout?: () => void
}

export default function ServiceUnavailablePage({
  onRetry,
  onLogout,
}: ServiceUnavailablePageProps) {
  const [apiReachable, setApiReachable] = useState<boolean | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    const checkApi = async () => {
      try {
        const apiUrl = process.env.NEXT_PUBLIC_API_BASE_URL
        if (!apiUrl) return

        // Use the unauthenticated /alive endpoint to check API availability.
        // Any HTTP response (even 404) proves the server is reachable.
        await fetch(`${apiUrl}/alive`, {
          method: 'GET',
          signal: controller.signal,
        })
        setApiReachable(true)
      } catch {
        // Network error or timeout - API is truly unreachable
        setApiReachable(false)
      }
    }

    checkApi()

    return () => controller.abort()
  }, [])

  const handleRetry = () => {
    if (onRetry) {
      onRetry()
    } else {
      window.location.reload()
    }
  }

  const handleLogout = () => {
    if (onLogout) {
      onLogout()
    } else {
      window.location.href = '/logout'
    }
  }

  const isAuthIssue = apiReachable === true

  return (
    <Result
      status={isAuthIssue ? 'warning' : 'error'}
      title={isAuthIssue ? 'Session Error' : 'Service Unavailable'}
      subTitle={
        isAuthIssue
          ? 'The server is running, but your session could not be verified. Signing out and back in typically resolves this.'
          : 'Moda is unable to connect to the server. Please try again later.'
      }
      extra={
        <Space>
          <Button
            type="primary"
            key="retry"
            icon={<ReloadOutlined />}
            onClick={handleRetry}
          >
            Retry
          </Button>
          <Button
            key="logout"
            icon={<LogoutOutlined />}
            onClick={handleLogout}
          >
            Sign Out
          </Button>
        </Space>
      }
    >
      <div className="desc">
        {isAuthIssue ? (
          <>
            <Paragraph>
              This can happen when your authentication session expires or
              becomes invalid. Common causes include:
            </Paragraph>
            <ul>
              <li>Your session expired while the app was open in multiple tabs</li>
              <li>Your account permissions were recently updated</li>
              <li>A browser extension or network issue interrupted authentication</li>
            </ul>
            <Paragraph>
              Click <strong>Sign Out</strong> and then sign back in. If this
              issue persists, please contact your administrator.
            </Paragraph>
          </>
        ) : (
          <>
            <Paragraph>
              The application is currently unable to reach the server. This
              could be due to:
            </Paragraph>
            <ul>
              <li>The server is temporarily unavailable</li>
              <li>Network connectivity issues</li>
              <li>The server is undergoing maintenance</li>
            </ul>
            <Paragraph>
              If this issue persists, please contact your administrator.
            </Paragraph>
          </>
        )}
      </div>
    </Result>
  )
}
