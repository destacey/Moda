import {
  AzureDevOpsBoardsWorkProcessDto,
  AzureDevOpsBoardsWorkspaceDto,
} from '@/src/services/moda-api'
import { ExportOutlined } from '@ant-design/icons'
import { Alert, Card, Descriptions, List, Space, Typography } from 'antd'
import Link from 'next/link'

interface AzdoBoardsWorkspacesProps {
  workspaces: AzureDevOpsBoardsWorkspaceDto[]
  workProcesses: AzureDevOpsBoardsWorkProcessDto[]
  organizationUrl: string
}

const AzdoBoardsWorkspaces = ({
  workspaces,
  workProcesses,
  organizationUrl,
}: AzdoBoardsWorkspacesProps) => {
  if (!workspaces) return null

  return (
    <>
      <Space direction="vertical" size="large" style={{ display: 'flex' }}>
        <Typography.Text>
          The workspaces below represent projects within Azure DevOps.
        </Typography.Text>
        {workspaces.length === 0 ? (
          <Alert
            message="No workspaces were found for this connection."
            type="error"
          />
        ) : (
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
                  <Descriptions column={1} size="small">
                    <Descriptions.Item>{item.description}</Descriptions.Item>
                    <Descriptions.Item label="Work Process">
                      {workProcesses.find(
                        (x) => x.externalId === item.workProcessId,
                      )?.name ?? 'Not Found'}
                    </Descriptions.Item>
                  </Descriptions>
                </Card>
              </List.Item>
            )}
          />
        )}
      </Space>
    </>
  )
}

export default AzdoBoardsWorkspaces
