'use client'

import { FC } from 'react'
import { MetricCard } from '.'
import useTheme from '@/src/components/contexts/theme'

export interface CompletionRateMetricProps {
  completed: number
  total: number
  target?: number
  title?: string
  tooltip?: string
  cardStyle?: React.CSSProperties
}

const CompletionRateMetric: FC<CompletionRateMetricProps> = ({
  completed,
  total,
  target = 80,
  title = 'Completion Rate',
  tooltip = 'Percentage of story points or items that are completed (Done or Removed).',
  cardStyle,
}) => {
  const { token } = useTheme()

  const completionRate =
    total > 0 ? Number(((completed / total) * 100).toFixed(1)) : 0

  return (
    <MetricCard
      title={title}
      value={completionRate}
      precision={1}
      suffix="%"
      valueStyle={{
        color: completionRate >= target ? token.colorSuccess : undefined,
      }}
      tooltip={tooltip}
      cardStyle={cardStyle}
    />
  )
}

export default CompletionRateMetric

