'use client'

import useTheme from '@/src/app/components/contexts/theme'

import dynamic from 'next/dynamic'

const Chart = dynamic(() => import('react-apexcharts'), { ssr: false })

const TeamPredictabilityRadarChart = () => {
  const { currentThemeName } = useTheme()
  const fontColor =
    currentThemeName === 'light'
      ? 'rgba(0, 0, 0, 0.45)'
      : 'rgba(255, 255, 255, 0.45)'
  const polygonSuccessColor =
    currentThemeName === 'light' ? '#87c9a7' : '#275a43'
  const polygonsNeutralColor =
    currentThemeName === 'light' ? '#ffffff' : '#141414'

  const series = [
    {
      name: 'Predictability',
      data: [80, 50, 30, 40, 100, 20],
    },
  ]
  const options = {
    chart: {
      fontFamily: 'inherit',
      parentHeightOffset: 0,
    },
    theme: {
      mode: currentThemeName,
      palette: 'palette1',
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
      categories: [
        'Core Services',
        'Data Analytics',
        'Insight',
        'Poly',
        'Team Juice',
        'Team Sauce',
      ],
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
    <div id="chart">
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
