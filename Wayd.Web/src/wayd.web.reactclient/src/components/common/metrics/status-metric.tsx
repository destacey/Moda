'use client'

import { FC } from 'react'
import { MetricCard } from '.'

export interface StatusMetricProps {
  value: number
  total: number
  title: string
  color?: string
  tooltip?: string
  cardStyle?: React.CSSProperties
}

const StatusMetric: FC<StatusMetricProps> = ({
  value,
  total,
  title,
  color,
  tooltip,
  cardStyle,
}) => {
  const percentage =
    total > 0 ? `${((value / total) * 100).toFixed(1)}%` : '0%'

  return (
    <MetricCard
      title={title}
      value={value}
      valueStyle={color ? { color } : undefined}
      secondaryValue={percentage}
      tooltip={tooltip}
      cardStyle={cardStyle}
    />
  )
}

export default StatusMetric
