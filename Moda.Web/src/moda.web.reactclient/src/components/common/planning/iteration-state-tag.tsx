import { Tag } from 'antd'
import { IterationState } from '../../types'
import { FC } from 'react'

export interface IterationStateTagProps {
  state: IterationState
}

const getTagNameAndColor = (state: IterationState) => {
  switch (state) {
    case IterationState.Completed:
      return { name: 'Completed', color: 'success' }
    case IterationState.Active:
      return { name: 'Active', color: 'processing' }
    default:
      return { name: 'Future', color: 'default' }
  }
}

const IterationStateTag: FC<IterationStateTagProps> = ({ state }) => {
  const { name, color: tagColor } = getTagNameAndColor(state)
  return <Tag color={tagColor}>{name}</Tag>
}

export default IterationStateTag
