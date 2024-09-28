'use client'

import { AzureDevOpsBoardsWorkspaceDto } from '@/src/services/moda-api'
import { AppstoreAddOutlined, ExportOutlined } from '@ant-design/icons'
import { Button, Card, Descriptions, message, Typography } from 'antd'
import Link from 'next/link'
import { useContext, useState } from 'react'
import InitWorkspaceIntegrationForm from '../components/init-workspace-integration-form'
import { AzdoBoardsConnectionContext } from './azdo-boards-connection-context'
import MapAzdoWorkspaceTeamsForm from '../components/map-azdo-workspace-teams-form'

const { Item } = Descriptions
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
  const [openMapAzdoWorkspaceTeamsForm, setOpenMapAzdoWorkspaceTeamsForm] =
    useState<boolean>(false)
  const [messageApi, contextHolder] = message.useMessage()

  const azdoBoardsConnection = useContext(AzdoBoardsConnectionContext)

  return (
    <>
      {contextHolder}
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
          <Item>
            {props.workspace.description ?? (
              <Text type="secondary" italic>
                No description provided.
              </Text>
            )}
          </Item>
        </Descriptions>
        {!props.enableInit && (
          <Button
            size="small"
            onClick={() => setOpenMapAzdoWorkspaceTeamsForm(true)}
          >
            Team Mappings
          </Button>
        )}
      </Card>
      {openInitWorkspaceIntegrationForm && (
        <InitWorkspaceIntegrationForm
          showForm={openInitWorkspaceIntegrationForm}
          connectionId={azdoBoardsConnection.connectionId}
          externalId={props.workspace.externalId}
          workspaceName={props.workspace.name}
          onFormSave={() => setOpenInitWorkspaceIntegrationForm(false)}
          onFormCancel={() => setOpenInitWorkspaceIntegrationForm(false)}
        />
      )}
      {openMapAzdoWorkspaceTeamsForm && (
        <MapAzdoWorkspaceTeamsForm
          showForm={openMapAzdoWorkspaceTeamsForm}
          connectionId={azdoBoardsConnection.connectionId}
          workspaceId={props.workspace.externalId}
          workspaceName={props.workspace.name}
          onFormSave={() => setOpenMapAzdoWorkspaceTeamsForm(false)}
          onFormCancel={() => setOpenMapAzdoWorkspaceTeamsForm(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

export default AzdoBoardsWorkspaceCard
