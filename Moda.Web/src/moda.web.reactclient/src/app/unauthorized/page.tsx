'use client'

import { Button, Result, Typography } from 'antd'
import { useMsal } from '@azure/msal-react'
import { useCallback } from 'react'

const { Paragraph, Text } = Typography

export default function UnauthorizedPage() {
  const { instance } = useMsal()

  const handleLogout = useCallback(async () => {
    instance.setActiveAccount(null)
    await instance.logoutRedirect({
      postLogoutRedirectUri: `${window.location.origin}/login`,
    })
  }, [instance])

  const activeAccount = instance.getActiveAccount()
  const username = activeAccount?.username ?? 'Unknown user'

  return (
    <Result
      status="403"
      title="Access Denied"
      subTitle="You are not authorized to access Moda."
      extra={[
        <Button type="primary" key="logout" onClick={handleLogout}>
          Sign Out
        </Button>,
      ]}
    >
      <div className="desc">
        <Paragraph>
          <Text strong>You are signed in as: </Text>
          <Text code>{username}</Text>
        </Paragraph>
        <Paragraph>
          Your account has been authenticated, but you do not have the required
          permissions to use Moda. Please contact your administrator if you
          believe this is an error.
        </Paragraph>
      </div>
    </Result>
  )
}
