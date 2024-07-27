'use client'

import {
  AzureDevOpsBoardsWorkProcessDto,
  AzureDevOpsBoardsWorkspaceDto,
} from '@/src/services/moda-api'
import { Alert, Space, Tabs, Typography } from 'antd'
import AzdoBoardsProcess from './azdo-boards-process'

const { Text, Title } = Typography

interface AzdoBoardsOrganizationProps {
  workProcesses: AzureDevOpsBoardsWorkProcessDto[]
  workspaces: AzureDevOpsBoardsWorkspaceDto[]
}

const AzdoBoardsOrganization = (props: AzdoBoardsOrganizationProps) => {
  if (!props.workProcesses || !props.workspaces) return null

  const getProcessWorkspaces = (process: AzureDevOpsBoardsWorkProcessDto) => {
    return props.workspaces.filter(
      (x) => x.workProcessId === process.externalId,
    )
  }

  return (
    <>
      <Space direction="vertical" size="large" style={{ display: 'flex' }}>
        <Text>
          The work processes and workspaces below represent processes and
          projects pulled from Azure DevOps.
        </Text>
        {props.workProcesses.length === 0 ? (
          <Alert
            message="No work processes were found for this connection."
            type="error"
          />
        ) : (
          <>
            <Title level={5}>Work Processes</Title>
            <Tabs
              tabPosition="left"
              items={props.workProcesses
                .slice() // array is likely in strict mode. clone the array to avoid sorting the original
                .sort((a, b) => a.name.localeCompare(b.name))
                .map((p) => {
                  return {
                    label: p.name,
                    key: p.externalId,
                    children: (
                      <AzdoBoardsProcess
                        workProcess={p}
                        workspaces={getProcessWorkspaces(p)}
                      />
                    ),
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
