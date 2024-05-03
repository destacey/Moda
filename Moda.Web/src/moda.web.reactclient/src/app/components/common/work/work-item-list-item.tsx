import { WorkItemListDto } from '@/src/services/moda-api'
import { List, Space, Tag } from 'antd'
import Link from 'next/link'
import ExternalIconLink from '../external-icon-link'

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
      <Tag>{workItem.type}</Tag>
      <Tag>{workItem.status}</Tag>
    </Space>
  )
}
