'use client'

import useTheme from '@/src/components/contexts/theme'
import { Spin } from 'antd'
import { useEffect, useMemo, useState } from 'react'

import dynamic from 'next/dynamic'
import dayjs from 'dayjs'
import { ApexOptions } from 'apexcharts'
import { useGetHealthReportQuery } from '@/src/store/features/common/health-checks-api'

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
    data: healthReportData,
    isLoading,
    isFetching,
    error,
    refetch,
  } = useGetHealthReportQuery(props.objectId, { skip: !props.objectId })

  const chartData = useMemo(() => {
    if (!healthReportData) return []
    return healthReportData
      .slice()
      .sort((a, b) =>
        dayjs(a.reportedOn).isAfter(dayjs(b.reportedOn)) ? 1 : -1,
      )
      .map((report) => ({
        x: dayjs(report.reportedOn),
        y: convertStatusToNumber(report.status?.name),
      }))
  }, [healthReportData])

  useEffect(() => {
    setSeriesData([{ name: 'Health Report', data: chartData }])
  }, [chartData])

  const options: ApexOptions = useMemo(
    () => ({
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
        type: 'datetime',
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
    }),
    [currentThemeName, fontColor],
  )

  if (isLoading || isFetching) {
    return <Spin size="small" />
  }

  const isClient = typeof window !== 'undefined'

  return (
    <div id="object-health-report-chart">
      {isClient && (
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
