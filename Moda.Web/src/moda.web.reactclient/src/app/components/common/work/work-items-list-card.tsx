import { WorkItemListDto } from '@/src/services/moda-api'
import ModaEmpty from '../moda-empty'
import { List } from 'antd'
import WorkItemListItem from './work-item-list-item'

export interface WorkItemsListCardProps {
  workItems: WorkItemListDto[]
}

const WorkItemsListCard = ({ workItems }: WorkItemsListCardProps) => {
  if (!workItems || workItems.length === 0)
    return <ModaEmpty message="No work items" />

  console.log(workItems)

  return (
    <List
      size="small"
      dataSource={workItems}
      renderItem={(workItem) => <WorkItemListItem workItem={workItem} />}
    />
  )
}

export default WorkItemsListCard
