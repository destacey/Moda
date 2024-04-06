'use client'

import { WorkspaceListDto } from '@/src/services/moda-api'
import { Card, Descriptions, Flex, List, Tag, Typography } from 'antd'
import { useRouter } from 'next/navigation'
import { ReactElement } from 'react'

const { Item: ListItem } = List
const { Item: DiscriptionItem } = Descriptions
const { Text } = Typography

export interface WorkspacesCardGridProps {
  workspaces: WorkspaceListDto[]
  viewSelector: ReactElement
  isLoading: boolean
}

const WorkspacesCardGrid = (props: WorkspacesCardGridProps) => {
  return (
    <>
      <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
        {props.viewSelector}
      </Flex>
      <List
        grid={{
          gutter: 16,
          xs: 1,
          sm: 2,
          md: 3,
          lg: 3,
          xl: 4,
          xxl: 4,
        }}
        loading={{
          spinning: props.isLoading,
          tip: 'Loading workspaces...',
          size: 'large',
        }}
        locale={{ emptyText: 'No workspaces found.' }}
        dataSource={props.workspaces?.sort((a, b) =>
          a.name.localeCompare(b.name),
        )}
        renderItem={(item) => (
          <ListItem>
            <WorkspaceCard workspace={item} />
          </ListItem>
        )}
      />
    </>
  )
}

interface WorkspaceCardProps {
  workspace: WorkspaceListDto
}

const WorkspaceCard = (props: WorkspaceCardProps) => {
  const router = useRouter()

  return (
    <Card
      title={props.workspace.name}
      size="small"
      hoverable
      onClick={() => router.push(`/work/workspaces/${props.workspace.key}`)}
    >
      <Descriptions column={1} size="small">
        <DiscriptionItem label="Key">{props.workspace.key}</DiscriptionItem>
        <DiscriptionItem>
          {props.workspace.description ?? (
            <Text type="secondary" italic>
              No description provided.
            </Text>
          )}
        </DiscriptionItem>
      </Descriptions>
      {!props.workspace.isActive && <Tag>Inactive</Tag>}
      {props.workspace.ownership.name === 'Managed' && (
        <Tag title="This workspace is owned by an external system.">
          Managed
        </Tag>
      )}
    </Card>
  )
}

export default WorkspacesCardGrid
