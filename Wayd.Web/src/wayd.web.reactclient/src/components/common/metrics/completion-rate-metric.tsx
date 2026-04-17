'use client'

import { CSSProperties, FC } from 'react'
import { MetricCard } from '.'
import useTheme from '@/src/components/contexts/theme'
import { SizingMethod } from '@/src/services/moda-api'

const getTooltipText = (sizingMethod: SizingMethod): string => {
  const unit =
    sizingMethod === SizingMethod.StoryPoints ? 'story points' : 'work items'
  return `Percentage of ${unit} that are completed (Done or Removed).`
}

export interface CompletionRateMetricProps {
  completed: number
  total: number
  target?: number
  title?: string
  tooltip?: string | SizingMethod
  cardStyle?: CSSProperties
}

const CompletionRateMetric: FC<CompletionRateMetricProps> = ({
  completed,
  total,
  target = 80,
  title = 'Completion Rate',
  tooltip = SizingMethod.StoryPoints,
  cardStyle,
}) => {
  const { token } = useTheme()

  const completionRate =
    total > 0 ? Number(((completed / total) * 100).toFixed(1)) : 0

  const resolvedTooltip =
    tooltip === SizingMethod.StoryPoints || tooltip === SizingMethod.Count
      ? getTooltipText(tooltip)
      : tooltip

  return (
    <MetricCard
      title={title}
      value={completionRate}
      precision={1}
      suffix="%"
      valueStyle={{
        color: completionRate >= target ? token.colorSuccess : undefined,
      }}
      tooltip={resolvedTooltip}
      cardStyle={cardStyle}
    />
  )
}

export default CompletionRateMetric
