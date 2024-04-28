import { WorkItemListDto } from '@/src/services/moda-api'
import { List, Space, Tag } from 'antd'

const { Item } = List
const { Meta } = Item

export interface WorkItemListItemProps {
  workItem: WorkItemListDto
}

const WorkItemListItem = ({ workItem }: WorkItemListItemProps) => {
  return (
    <Item key={workItem.id}>
      <Meta
        title={`${workItem.key} - ${workItem.title}`}
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
