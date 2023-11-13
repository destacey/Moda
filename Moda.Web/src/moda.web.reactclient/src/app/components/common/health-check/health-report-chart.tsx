'use client'

import useTheme from '@/src/app/components/contexts/theme'
import { useGetHealthReport } from '@/src/services/queries/health-check-queries'
import { Spin } from 'antd'
import { useEffect, useState } from 'react'

import dynamic from 'next/dynamic'
import dayjs from 'dayjs'
import { ApexOptions } from 'apexcharts'

const Chart = dynamic(() => import('react-apexcharts'), { ssr: false })

interface HealthReportChartProps {
  objectId: string
}

const convertStatusToNumber = (status: string) => {
  switch (status) {
    case 'Healthy':
      return 2
    case 'At Risk':
      return 1
    case 'Unhealthy':
      return 0
    default:
      return 0
  }
}

// TODO: add a way to refresh the chart when a new health report is added from a parent component
const HealthReportChart = (props: HealthReportChartProps) => {
  const [seriesData, setSeriesData] = useState([])

  const { currentThemeName } = useTheme()
  const fontColor =
    currentThemeName === 'light'
      ? 'rgba(0, 0, 0, 0.45)'
      : 'rgba(255, 255, 255, 0.45)'

  const {
    data: healthReport,
    isLoading,
    isFetching,
  } = useGetHealthReport(props.objectId)

  useEffect(() => {
    if (!healthReport) return

    const chartData = healthReport
      .sort((a, b) =>
        dayjs(a.reportedOn).isAfter(dayjs(b.reportedOn)) ? 1 : -1,
      )
      .map((report) => ({
        x: dayjs(report.reportedOn),
        y: convertStatusToNumber(report.status?.name),
      }))
    console.log(chartData)
    setSeriesData([{ name: 'Health Report', data: chartData }])
  }, [healthReport])

  if (isLoading || isFetching) {
    return <Spin size="small" />
  }

  const options: ApexOptions = {
    chart: {
      fontFamily: 'inherit',
      parentHeightOffset: 0,
    },
    title: {
      text: 'Health Report',
      style: {
        fontSize: '14px',
        fontWeight: 'normal',
        color: fontColor,
      },
    },
    xaxis: {
      type: 'datetime', // removing this changes how the x-axis is displayed
      labels: {
        style: {
          colors: fontColor,
        },
        datetimeUTC: false,
        datetimeFormatter: {
          year: 'yyyy',
          month: "MMM 'yy",
          day: 'dd MMM',
          hour: 'HH:mm',
        },
      },
    },
    yaxis: {
      min: 0,
      max: 2,
      tickAmount: 2,
      labels: {
        style: {
          colors: fontColor,
        },
        formatter: (value) => {
          switch (value) {
            case 0:
              return 'Unhealthy'
            case 1:
              return 'At Risk'
            case 2:
              return 'Healthy'
            default:
              return ''
          }
        },
      },
    },
    tooltip: {
      enabled: true,
      theme: currentThemeName,
      x: {
        format: 'MMM dd, yyyy h:mm TT',
      },
      y: {
        title: {
          formatter: () => 'Health Status:',
        },
      },
    },
    markers: {
      size: 5,
    },
    stroke: {
      width: 3,
    },
    noData: {
      text: 'No Health Report Data',
    },
  }

  return (
    <div id="object-health-report-chart">
      {typeof window !== 'undefined' && (
        <Chart
          options={options}
          series={seriesData}
          type="line"
          height={200}
          width={350}
        />
      )}
    </div>
  )
}

export default HealthReportChart
