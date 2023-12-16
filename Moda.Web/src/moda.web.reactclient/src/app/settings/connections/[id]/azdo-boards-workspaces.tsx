import {
  AzureDevOpsBoardsWorkProcessDto,
  AzureDevOpsBoardsWorkspaceDto,
} from '@/src/services/moda-api'
import { AppstoreAddOutlined, ExportOutlined } from '@ant-design/icons'
import {
  Alert,
  Button,
  Card,
  Descriptions,
  List,
  Space,
  Typography,
} from 'antd'
import Link from 'next/link'

interface AzdoBoardsWorkspacesProps {
  workspaces: AzureDevOpsBoardsWorkspaceDto[]
  workProcesses: AzureDevOpsBoardsWorkProcessDto[]
  organizationUrl: string
  initWorkspace: (workspaceId: string) => void
}

const AzdoBoardsWorkspaces = (props: AzdoBoardsWorkspacesProps) => {
  if (!props.workspaces) return null

  const getWorkProcessName = (workProcessId?: string) => {
    if (!workProcessId) return null

    const workProcess = props.workProcesses.find(
      (x) => x.externalId === workProcessId,
    )

    return workProcess?.name
  }

  // const allowIntegrationSetup = (workProcessId?: string) => {
  //   if (!workProcessId) return false

  //   // and the workprocess isn't already integrated
  //   return !!getWorkProcessName(workProcessId)
  // }

  return (
    <>
      <Space direction="vertical" size="large" style={{ display: 'flex' }}>
        <Typography.Text>
          The workspaces below represent projects within Azure DevOps.
        </Typography.Text>
        {props.workspaces.length === 0 ? (
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
            dataSource={props.workspaces.sort((a, b) =>
              a.name.localeCompare(b.name),
            )}
            renderItem={(item) => (
              <List.Item>
                <Card
                  title={
                    <>
                      {item.name}{' '}
                      <Link
                        href={`${props.organizationUrl}/${item.name}`}
                        target="_blank"
                        title="Open in Azure DevOps"
                      >
                        <ExportOutlined style={{ width: '12px' }} />
                      </Link>
                    </>
                  }
                  // extra={
                  //   allowIntegrationSetup(item.workProcessId) && (
                  //     <Button
                  //       type="text"
                  //       title="Setup Workspace Integration"
                  //       icon={<AppstoreAddOutlined />}
                  //       onClick={() => props.initWorkspace(item.externalId)}
                  //     />
                  //   )
                  // }
                >
                  <Descriptions column={1} size="small">
                    <Descriptions.Item>{item.description}</Descriptions.Item>
                    <Descriptions.Item label="Work Process">
                      {getWorkProcessName(item.workProcessId) ?? 'Not Found'}
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
