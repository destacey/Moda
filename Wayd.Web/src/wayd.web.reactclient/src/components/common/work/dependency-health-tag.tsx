'use client'

import { Tag } from 'antd'
import { DependencyHealth } from '../../types'
import { FC, memo } from 'react'
import DependencyHealthTooltip from './dependency-health-tooltip'

export interface DependencyHealthTagProps {
  name: string
  health: DependencyHealth
}

const getTagColor = (health: DependencyHealth): string => {
  switch (health) {
    case DependencyHealth.Healthy:
      return 'success'
    case DependencyHealth.AtRisk:
      return 'warning'
    case DependencyHealth.Unhealthy:
      return 'error'
    default:
      return 'default'
  }
}

const DependencyHealthTag: FC<DependencyHealthTagProps> = ({
  name,
  health,
}) => {
  const color = getTagColor(health)
  return (
    <DependencyHealthTooltip health={health}>
      <Tag color={color}>{name}</Tag>
    </DependencyHealthTooltip>
  )
}

export default memo(DependencyHealthTag)
