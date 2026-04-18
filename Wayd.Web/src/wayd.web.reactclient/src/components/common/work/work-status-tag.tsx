'use client'

import { Tag } from 'antd'
import { WorkStatusCategory } from '../../types'
import { FC, memo } from 'react'

export interface WorkStatusTagProps {
  status: string
  category: WorkStatusCategory
}

const getTagColor = (category: WorkStatusCategory): string => {
  switch (category) {
    case WorkStatusCategory.Proposed:
      return 'default'
    case WorkStatusCategory.Active:
      return 'processing'
    case WorkStatusCategory.Done:
      return 'success'
    case WorkStatusCategory.Removed:
      return 'warning'
    default:
      return 'default'
  }
}

const WorkStatusTag: FC<WorkStatusTagProps> = ({ status, category }) => {
  const color = getTagColor(category)
  return <Tag color={color}>{status}</Tag>
}

export default memo(WorkStatusTag)
