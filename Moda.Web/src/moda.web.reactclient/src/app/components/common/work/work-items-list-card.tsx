'use client'

import { WorkItemListDto } from '@/src/services/moda-api'

export interface WorkItemsListCardProps {
  workItems: WorkItemListDto[]
}

const WorkItemsListCard = ({ workItems }: WorkItemsListCardProps) => {
  return (
    <div>
      {workItems.map((workItem) => (
        <div key={workItem.id}>
          {workItem.key} {workItem.title}
        </div>
      ))}
    </div>
  )
}

export default WorkItemsListCard
