import { WorkItemListDto } from '@/src/services/moda-api'
import ModaEmpty from '../moda-empty'
import { List } from 'antd'
import WorkItemListItem from './work-item-list-item'

export interface WorkItemsListCardProps {
  workItems: WorkItemListDto[]
  isLoading: boolean
}

const WorkItemsListCard = ({
  workItems,
  isLoading,
}: WorkItemsListCardProps) => {
  return (
    <List
      size="small"
      loading={isLoading}
      dataSource={workItems}
      locale={{ emptyText: <ModaEmpty message="No work items" /> }}
      renderItem={(workItem) => <WorkItemListItem workItem={workItem} />}
    />
  )
}

export default WorkItemsListCard
