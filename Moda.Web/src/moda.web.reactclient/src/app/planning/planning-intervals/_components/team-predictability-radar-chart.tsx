'use client'

import useTheme from '@/src/components/contexts/theme'
import { PlanningIntervalTeamPredictabilityDto } from '@/src/services/moda-api'
import { RadarConfig } from '@ant-design/charts'
import { Card } from 'antd'
import dynamic from 'next/dynamic'
import { useMemo } from 'react'

const Radar = dynamic(
  () => import('@ant-design/charts').then((mod) => mod.Radar) as any,
  { ssr: false },
)

interface TeamPredictabilityRadarChartProps {
  teamPredictabilities?: PlanningIntervalTeamPredictabilityDto[]
  isLoading: boolean
}

const TeamPredictabilityRadarChart: React.FC<
  TeamPredictabilityRadarChartProps
> = ({
  teamPredictabilities,
  isLoading,
}: TeamPredictabilityRadarChartProps) => {
  const { currentThemeName, antDesignChartsTheme } = useTheme()

  const config = useMemo(() => {
    const fontColor =
      currentThemeName === 'light'
        ? 'rgba(0, 0, 0, 0.45)'
        : 'rgba(255, 255, 255, 0.45)'

    return {
      title: {
        title: 'Team Predictability',
        style: {
          titleFontSize: 14,
          titleFontWeight: 'normal',
          titleFill: fontColor,
        },
      },
      theme: antDesignChartsTheme,
      height: 350,
      width: 400,
      data: teamPredictabilities?.map((x) => ({
        team: x.team.name,
        Predictability: x.predictability ?? 0,
      })),
      xField: 'team',
      yField: 'Predictability',
      tooltip: {
        items: [
          { channel: 'y', valueFormatter: (value) => `${value.toFixed(0)}%` },
        ],
      },
      area: {
        style: {
          fillOpacity: 0.2,
        },
      },
      scale: {
        x: {
          padding: 0.5,
          align: 0,
        },
        y: {
          domainMin: 0,
          domainMax: 100,
          tickInterval: 20,
        },
      },
      axis: {
        x: {
          title: false,
          grid: true,
        },
        y: {
          gridAreaFill: 'rgba(0, 0, 0, 0.1)',
          label: true,
          title: false,
        },
      },
    } as any // this is a hack to fix typescript error. Should be as RadarConfig
  }, [antDesignChartsTheme, currentThemeName, teamPredictabilities])

  return (
    <Card
      size="small"
      loading={isLoading}
      style={{ minHeight: 350, minWidth: 400 }}
    >
      <Radar {...config} />
    </Card>
  )
}

export default TeamPredictabilityRadarChart
