import { AzureDevOpsBoardsWorkspaceDto } from '@/src/services/moda-api'
import { ExportOutlined } from '@ant-design/icons'
import { Card, List, Space, Typography } from 'antd'
import Link from 'next/link'

interface AzdoBoardsWorkspacesProps {
  workspaces: AzureDevOpsBoardsWorkspaceDto[]
  organizationUrl: string
}

const AzdoBoardsWorkspaces = ({
  workspaces,
  organizationUrl,
}: AzdoBoardsWorkspacesProps) => {
  if (!workspaces) return null
  if (workspaces.length === 0)
    return (
      <>
        <Typography.Text>
          The workspaces below represent projects within Azure DevOps.
        </Typography.Text>
      </>
    )

  return (
    <>
      <Space direction="vertical" size="large" style={{ display: 'flex' }}>
        <Typography.Text>
          The workspaces below represent projects within Azure DevOps.
        </Typography.Text>
        <List
          grid={{
            gutter: 16,
            xs: 1,
            sm: 1,
            md: 2,
            lg: 3,
            xl: 4,
            xxl: 6,
          }}
          dataSource={workspaces}
          renderItem={(item) => (
            <List.Item>
              <Card
                type=""
                title={
                  <>
                    {item.name}{' '}
                    <Link
                      href={`${organizationUrl}/${item.name}`}
                      target="_blank"
                      title="Open in Azure DevOps"
                    >
                      <ExportOutlined style={{ width: '12px' }} />
                    </Link>
                  </>
                }
              >
                {/* adds a non-breaking space if the description is null */}
                {item.description ?? '\u00A0'}
              </Card>
            </List.Item>
          )}
        />
      </Space>
    </>
  )
}

export default AzdoBoardsWorkspaces
