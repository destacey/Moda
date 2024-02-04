'use client'

import { AzureDevOpsBoardsWorkspaceDto } from '@/src/services/moda-api'
import { AppstoreAddOutlined, ExportOutlined } from '@ant-design/icons'
import { Button, Card, Descriptions, Typography } from 'antd'
import Link from 'next/link'
import { useContext, useState } from 'react'
import InitWorkspaceIntegrationForm from '../components/init-workspace-integration-form'
import { AzdoBoardsConnectionContext } from './azdo-boards-connection-context'

const { Text } = Typography

interface AzdoBoardsWorkspaceCardProps {
  workspace: AzureDevOpsBoardsWorkspaceDto
  enableInit: boolean
}

const AzdoBoardsWorkspaceCard = (props: AzdoBoardsWorkspaceCardProps) => {
  const [
    openInitWorkspaceIntegrationForm,
    setOpenInitWorkspaceIntegrationForm,
  ] = useState<boolean>(false)

  const onInitWorkspaceFormClosed = (wasSaved: boolean) => {
    setOpenInitWorkspaceIntegrationForm(false)
  }

  const azdoBoardsConnection = useContext(AzdoBoardsConnectionContext)

  const noDescription = (
    <Text type="secondary" italic>
      No description provided.
    </Text>
  )

  return (
    <>
      <Card
        data-testid={props.workspace.externalId}
        size="small"
        title={
          <>
            {props.workspace.name}{' '}
            <Link
              href={`${azdoBoardsConnection.organizationUrl}/${props.workspace.name}`}
              target="_blank"
              title="Open in Azure DevOps"
            >
              <ExportOutlined style={{ width: '12px' }} />
            </Link>
          </>
        }
        extra={
          props.enableInit && (
            <Button
              type="text"
              title="Setup Workspace Integration"
              icon={<AppstoreAddOutlined />}
              onClick={() => setOpenInitWorkspaceIntegrationForm(true)}
            />
          )
        }
      >
        <Descriptions column={1} size="small">
          <Descriptions.Item>
            {props.workspace.description ?? noDescription}
          </Descriptions.Item>
        </Descriptions>
      </Card>
      {openInitWorkspaceIntegrationForm && (
        <InitWorkspaceIntegrationForm
          showForm={openInitWorkspaceIntegrationForm}
          connectionId={azdoBoardsConnection.connectionId}
          externalId={props.workspace.externalId}
          onFormSave={() => onInitWorkspaceFormClosed(false)}
          onFormCancel={() => onInitWorkspaceFormClosed(false)}
        />
      )}
    </>
  )
}

export default AzdoBoardsWorkspaceCard
