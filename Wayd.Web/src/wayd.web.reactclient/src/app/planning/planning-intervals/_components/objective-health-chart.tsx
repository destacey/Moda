'use client'

import { ChartCard } from '@/src/components/common/metrics'
import { healthCheckTagColor } from '@/src/components/common/health-check/health-check-utils'
import useTheme from '@/src/components/contexts/theme'
import { PlanningIntervalObjectiveListDto } from '@/src/services/wayd-api'
import dynamic from 'next/dynamic'
import type { PieConfig } from '@ant-design/charts'
import { theme } from 'antd'

const Pie = dynamic(
  () => import('@ant-design/charts').then((mod) => mod.Pie) as any,
  { ssr: false },
)

export interface ObjectiveHealthChartProps {
  objectivesData: PlanningIntervalObjectiveListDto[]
  embedded?: boolean
  height?: number
}

interface ObjectiveHealthChartDataItem {
  type: string
  count: number
}

const OBJECTIVE_HEALTH_ORDER = ['Healthy', 'At Risk', 'Unhealthy', 'Unknown']

const ObjectiveHealthChart = (props: ObjectiveHealthChartProps) => {
  const { antDesignChartsTheme } = useTheme()
  const { token } = theme.useToken()

  if (!props.objectivesData || props.objectivesData.length === 0) return null

  const activeObjectives = props.objectivesData.filter(
    (objective) => objective.status?.name !== 'Completed',
  )

  if (activeObjectives.length === 0) return null

  const groupedHealthData = activeObjectives.reduce(
    (acc, objective) => {
      const status = objective.status?.name
      const health =
        status === 'Canceled' || status === 'Missed'
          ? 'Unhealthy'
          : (objective.healthCheck?.status?.name ?? 'Unknown')

      acc[health] = acc[health] ? acc[health] + 1 : 1
      return acc
    },
    {} as Record<string, number>,
  )

  const data: ObjectiveHealthChartDataItem[] = Object.entries(groupedHealthData)
    .map(([health, count]) => ({ type: health, count }))
    .sort((a, b) => {
      const aIndex = OBJECTIVE_HEALTH_ORDER.indexOf(a.type)
      const bIndex = OBJECTIVE_HEALTH_ORDER.indexOf(b.type)
      const normalizedAIndex = aIndex === -1 ? Number.MAX_SAFE_INTEGER : aIndex
      const normalizedBIndex = bIndex === -1 ? Number.MAX_SAFE_INTEGER : bIndex
      return normalizedAIndex - normalizedBIndex
    })

  const mapSemanticColorToChartColor = (semanticColor: string) => {
    switch (semanticColor) {
      case 'processing':
        return token.colorInfo
      case 'success':
        return token.colorSuccess
      case 'error':
        return token.colorError
      case 'warning':
        return token.colorWarning
      case 'default':
      default:
        return token.colorTextQuaternary
    }
  }

  const healthDomain = data.map((item) => item.type)
  const healthRange = data.map((item) =>
    mapSemanticColorToChartColor(healthCheckTagColor(item.type)),
  )

  const total = data.reduce((acc, x) => acc + x.count, 0)
  const tooltipBounding =
    typeof window !== 'undefined'
      ? {
          x: 0,
          y: 0,
          width: window.innerWidth,
          height: window.innerHeight,
        }
      : undefined
  const config: PieConfig = {
    theme: antDesignChartsTheme,
    data,
    angleField: 'count',
    colorField: 'type',
    scale: {
      color: {
        domain: healthDomain,
        range: healthRange,
      },
    },
    autoFit: true,
    height: props.height ?? 280,
    padding: props.embedded ? 0 : 'auto',
    label: {
      text: (d) =>
        props.embedded
          ? `${d.count} (${Math.round((d.count / total) * 100)}%)`
          : `${d.type}\n ${d.count} (${Math.round((d.count / total) * 100)}%)`,
      transform: [
        {
          type: 'overlapDodgeY',
        },
      ],
    },
    legend: false,
    interaction: {
      tooltip: {
        mount: 'body',
        ...(props.embedded ? { position: 'top' as const } : {}),
        ...(tooltipBounding ? { bounding: tooltipBounding } : {}),
      },
    },
    tooltip: {
      title: () => 'Objectives',
      items: [
        (d: ObjectiveHealthChartDataItem) => ({
          name: d.type,
          value: `${d.count} (${Math.round((d.count / total) * 100)}%)`,
        }),
      ],
    } as any,
  }

  return (
    <ChartCard
      title="Objectives By Health"
      tooltip="Objective health for open objectives."
      embedded={props.embedded}
    >
      <Pie {...(config as any)} />
    </ChartCard>
  )
}

export default ObjectiveHealthChart
