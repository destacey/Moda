'use client'

import { Tooltip } from 'antd'
import { DependencyHealth } from '../../types'
import { FC, memo, ReactNode } from 'react'

export interface DependencyHealthTooltipProps {
  health: DependencyHealth
  children: ReactNode
}

const getHealthDescription = (health: DependencyHealth): string => {
  switch (health) {
    case DependencyHealth.Healthy:
      return 'Either the predecessor is done, or is planned to complete on or before the successor.'
    case DependencyHealth.AtRisk:
      return 'Neither the predecessor nor successor have future planned dates.'
    case DependencyHealth.Unhealthy:
      return 'Either the predecessor was removed, or is planned to complete after the successor needs it.'
    default:
      return 'The health status of this dependency has not been determined or reported.'
  }
}

const DependencyHealthTooltip: FC<DependencyHealthTooltipProps> = ({
  health,
  children,
}) => {
  const description = getHealthDescription(health)

  return <Tooltip title={description}>{children}</Tooltip>
}

export default memo(DependencyHealthTooltip)
