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
  Flex,
  List,
  Space,
  Tabs,
  Typography,
} from 'antd'
import Link from 'next/link'

interface AzdoBoardsOrganizationProps {
  workProcesses: AzureDevOpsBoardsWorkProcessDto[]
  workspaces: AzureDevOpsBoardsWorkspaceDto[]
  organizationUrl: string
  initWorkspace: (workspaceId: string) => void
}

const AzdoBoardsOrganization = (props: AzdoBoardsOrganizationProps) => {
  if (!props.workProcesses || !props.workspaces) return null

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

  const workspaceSection = (
    processWorkspaces: AzureDevOpsBoardsWorkspaceDto[],
  ) => (
    <>
      <Typography.Title level={5}>Workspaces</Typography.Title>
      <List
        grid={{
          gutter: 16,
          xs: 1,
          sm: 1,
          md: 1,
          lg: 2,
          xl: 3,
          xxl: 4,
        }}
        locale={{ emptyText: 'No workspaces found.' }}
        dataSource={processWorkspaces.sort((a, b) =>
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
    </>
  )

  const processSection = (process: AzureDevOpsBoardsWorkProcessDto) => {
    const processWorkspaces = props.workspaces.filter(
      (x) => x.workProcessId === process.externalId,
    )

    return (
      <>
        <Flex vertical>
          <Typography.Title level={5} style={{ marginTop: '0px' }}>
            {process.name}
          </Typography.Title>
          <Typography.Text>{process.description}</Typography.Text>
          {workspaceSection(processWorkspaces)}
        </Flex>
      </>
    )
  }

  return (
    <>
      <Space direction="vertical" size="large" style={{ display: 'flex' }}>
        <Typography.Text>
          The work processes and workspaces below represent processes and
          projects pulled from Azure DevOps.
        </Typography.Text>
        {props.workProcesses.length === 0 ? (
          <Alert
            message="No work processes were found for this connection."
            type="error"
          />
        ) : (
          <>
            <Typography.Title level={5}>Work Processes</Typography.Title>
            <Tabs
              tabPosition="left"
              items={props.workProcesses
                .sort((a, b) => a.name.localeCompare(b.name))
                .map((p) => {
                  return {
                    label: p.name,
                    key: p.externalId,
                    children: processSection(p),
                  }
                })}
            />
          </>
        )}
      </Space>
    </>
  )
}

export default AzdoBoardsOrganization
