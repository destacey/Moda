'use client'

import { FC } from 'react'
import { MetricCard } from '.'

export interface CycleTimeMetricProps {
  value: number
  title?: string
  tooltip?: string
  cardStyle?: React.CSSProperties
  embedded?: boolean
}

const CycleTimeMetric: FC<CycleTimeMetricProps> = ({
  value,
  title = 'Avg Cycle Time',
  tooltip = 'The time from when work starts (Activated) to when it is completed (Done).',
  cardStyle,
  embedded,
}) => {
  return (
    <MetricCard
      title={title}
      value={value}
      precision={2}
      suffix="days"
      tooltip={tooltip}
      cardStyle={cardStyle}
      embedded={embedded}
    />
  )
}

export default CycleTimeMetric
