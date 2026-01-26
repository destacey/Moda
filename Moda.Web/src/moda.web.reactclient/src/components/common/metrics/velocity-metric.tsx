'use client'

import { FC } from 'react'
import { MetricCard } from '.'
import useTheme from '@/src/components/contexts/theme'

export interface VelocityMetricProps {
  completed: number
  total: number
  tooltip?: string
  cardStyle?: React.CSSProperties
}

const VelocityMetric: FC<VelocityMetricProps> = ({
  completed,
  total,
  tooltip = 'Total number of story points currently in the sprint that are completed (Done or Removed). Percentage shown represents the portion of total sprint work that is complete.',
  cardStyle,
}) => {
  const { token } = useTheme()

  const velocityPercentage =
    total > 0 ? `${((completed / total) * 100).toFixed(1)}%` : '0%'

  return (
    <MetricCard
      title="Velocity"
      value={completed}
      valueStyle={{ color: token.colorSuccess }}
      secondaryValue={velocityPercentage}
      tooltip={tooltip}
      cardStyle={cardStyle}
    />
  )
}

export default VelocityMetric

