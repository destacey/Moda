'use client'

import useTheme from '@/src/app/components/contexts/theme'
import { PlanningIntervalPredictabilityDto } from '@/src/services/moda-api'

import dynamic from 'next/dynamic'

const Chart = dynamic(() => import('react-apexcharts'), { ssr: false })

interface TeamPredictabilityRadarChartProps {
  predictability: PlanningIntervalPredictabilityDto
}

const TeamPredictabilityRadarChart = ({
  predictability,
}: TeamPredictabilityRadarChartProps) => {
  const { currentThemeName } = useTheme()
  const fontColor =
    currentThemeName === 'light'
      ? 'rgba(0, 0, 0, 0.45)'
      : 'rgba(255, 255, 255, 0.45)'
  const polygonSuccessColor =
    currentThemeName === 'light' ? '#87c9a7' : '#275a43'
  const polygonsNeutralColor =
    currentThemeName === 'light' ? '#ffffff' : '#141414'

  if (!predictability) return null

  const teams = predictability.teamPredictabilities.map((x) => x.team.name)
  const data = predictability.teamPredictabilities.map((x) => x.predictability)

  const series = [
    {
      name: 'Predictability',
      data: data,
    },
  ]
  const options = {
    chart: {
      fontFamily: 'inherit',
      parentHeightOffset: 0,
    },
    title: {
      text: 'Team Predictability',
      style: {
        fontSize: '14px',
        fontWeight: 'normal',
        color: fontColor,
      },
    },
    dataLabels: {
      enabled: true,
    },
    tooltip: {
      enabled: true,
      theme: currentThemeName,
    },
    plotOptions: {
      radar: {
        polygons: {
          fill: {
            colors: [
              polygonSuccessColor,
              polygonSuccessColor,
              polygonSuccessColor,
              polygonsNeutralColor,
              polygonsNeutralColor,
              polygonsNeutralColor,
              polygonsNeutralColor,
              polygonsNeutralColor,
              polygonsNeutralColor,
              polygonsNeutralColor,
            ],
          },
        },
      },
    },
    xaxis: {
      categories: teams,
    },
    yaxis: {
      min: 0,
      max: 100,
      tickAmount: 10,
      labels: {
        style: {
          fontSize: '12px',
          fontWeight: 'normal',
          colors: fontColor,
        },
      },
    },
  }

  return (
    <div id="team-predictability-radar-chart">
      {typeof window !== 'undefined' && (
        <Chart
          options={options}
          series={series}
          type="radar"
          height={350}
          width={400}
        />
      )}
    </div>
  )
}

export default TeamPredictabilityRadarChart
