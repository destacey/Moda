import { WorkItemListDto } from '@/src/services/moda-api'
import { List, Space, Tag } from 'antd'
import Link from 'next/link'
import ExternalIconLink from '../external-icon-link'
import { getWorkStatusCategoryColor } from '@/src/utils'

const { Item } = List
const { Meta } = Item

export interface WorkItemListItemProps {
  workItem: WorkItemListDto
}

const WorkItemListItem = ({ workItem }: WorkItemListItemProps) => {
  const workItemTitle = (
    <Link
      href={`/work/workspaces/${workItem.workspace.key}/work-items/${workItem.key}`}
    >
      {workItem.key} - {workItem.title}&nbsp;
    </Link>
  )
  return (
    <Item key={workItem.id}>
      <Meta
        title={
          <ExternalIconLink
            content={workItemTitle}
            url={workItem.externalViewWorkItemUrl}
            tooltip="Open in external system"
          />
        }
        description={<WorkItemListItemDescription workItem={workItem} />}
      />
    </Item>
  )
}

export default WorkItemListItem

const WorkItemListItemDescription = ({ workItem }: WorkItemListItemProps) => {
  return (
    <Space wrap>
      <Tag title="Work Item Type">{workItem.type}</Tag>
      <Tag
        title="Work Item Status"
        color={getWorkStatusCategoryColor(workItem.statusCategory.name)}
      >
        {workItem.status}
      </Tag>
      {workItem.team && <Tag title="Work Item Team">{workItem.team.name}</Tag>}
    </Space>
  )
}
