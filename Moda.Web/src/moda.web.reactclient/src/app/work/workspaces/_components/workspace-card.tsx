import { WorkspaceListDto } from '@/src/services/moda-api'
import { Card, Descriptions, Space, Tag, Typography } from 'antd'
import { useRouter } from 'next/navigation'

const { Item: DiscriptionItem } = Descriptions
const { Text } = Typography

interface WorkspaceCardProps {
  workspace: WorkspaceListDto
}

const WorkspaceCard: React.FC<WorkspaceCardProps> = (
  props: WorkspaceCardProps,
) => {
  const router = useRouter()

  return (
    <Card
      title={props.workspace.name}
      size="small"
      hoverable
      onClick={() => router.push(`/work/workspaces/${props.workspace.key}`)}
    >
      <Space direction="vertical">
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
        <div>
          {!props.workspace.isActive && <Tag>Inactive</Tag>}
          {props.workspace.ownership.name === 'Managed' && (
            <Tag title="This workspace is owned by an external system.">
              Managed
            </Tag>
          )}
        </div>
      </Space>
    </Card>
  )
}

export default WorkspaceCard
