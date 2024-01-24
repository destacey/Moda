'use client'

import {
  AzureDevOpsBoardsWorkProcessDto,
  AzureDevOpsBoardsWorkspaceDto,
} from '@/src/services/moda-api'
import { ExportOutlined } from '@ant-design/icons'
import { Button, Flex, List, Typography } from 'antd'
import Link from 'next/link'
import { useContext, useState } from 'react'
import InitWorkProcessIntegrationForm from '../components/init-work-process-integration-form'
import AzdoBoardsWorkspaceCard from './azdo-boards-workspace-card'
import { AzdoBoardsConnectionContext } from './azdo-boards-connection-context'

const { Title, Text } = Typography

interface AzdoBoardsProcessProps {
  workProcess: AzureDevOpsBoardsWorkProcessDto
  workspaces: AzureDevOpsBoardsWorkspaceDto[]
}

const AzdoBoardsProcess = (props: AzdoBoardsProcessProps) => {
  const [
    openInitWorkProcessIntegrationForm,
    setOpenInitWorkProcessIntegrationForm,
  ] = useState<boolean>(false)

  const azdoBoardsConnection = useContext(AzdoBoardsConnectionContext)

  const workProcessExists = false // TODO: check if work process exists

  const onInitWorkProcessIntegrationFormClosed = (wasSaved: boolean) => {
    setOpenInitWorkProcessIntegrationForm(false)
    //refetch()
  }

  const workspaceSection = (
    processWorkspaces: AzureDevOpsBoardsWorkspaceDto[],
  ) => (
    <>
      <Title level={5}>Workspaces</Title>
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
            <AzdoBoardsWorkspaceCard workspace={item} enableInit={true} />
          </List.Item>
        )}
      />
    </>
  )

  return (
    <>
      <Flex vertical>
        <Flex justify="space-between" gap="middle" wrap="wrap-reverse">
          <Flex vertical>
            <Flex gap="small" align="start">
              <Title level={5} style={{ marginTop: '0px' }}>
                {props.workProcess.name}
              </Title>
              <Link
                href={`${azdoBoardsConnection.organizationUrl}/_settings/process?process-name=${props.workProcess.name}&_a=workitemtypes`}
                target="_blank"
                title="Open in Azure DevOps"
              >
                <ExportOutlined style={{ width: '12px' }} />
              </Link>
            </Flex>
            <Text>{props.workProcess.description}</Text>
          </Flex>
          {!workProcessExists && (
            <Button
              title="Setup Work Process Integration"
              onClick={() => setOpenInitWorkProcessIntegrationForm(true)}
            >
              Initialize
            </Button>
          )}
        </Flex>
        {workspaceSection(props.workspaces)}
      </Flex>
      {openInitWorkProcessIntegrationForm && (
        <InitWorkProcessIntegrationForm
          showForm={openInitWorkProcessIntegrationForm}
          connectionId={azdoBoardsConnection.connectionId}
          externalId={props.workProcess.externalId}
          onFormSave={() => onInitWorkProcessIntegrationFormClosed(true)}
          onFormCancel={() => onInitWorkProcessIntegrationFormClosed(false)}
        />
      )}
    </>
  )
}

export default AzdoBoardsProcess
