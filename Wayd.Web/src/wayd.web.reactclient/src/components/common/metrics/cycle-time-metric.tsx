'use client'

import { FC } from 'react'
import { MetricCard } from '.'

export interface CycleTimeMetricProps {
  value: number
  title?: string
  tooltip?: string
  cardStyle?: React.CSSProperties
}

const CycleTimeMetric: FC<CycleTimeMetricProps> = ({
  value,
  title = 'Avg Cycle Time',
  tooltip = 'The time from when work starts (Activated) to when it is completed (Done).',
  cardStyle,
}) => {
  return (
    <MetricCard
      title={title}
      value={value}
      precision={2}
      suffix="days"
      tooltip={tooltip}
      cardStyle={cardStyle}
    />
  )
}

export default CycleTimeMetric
