'use client'

import dynamic from 'next/dynamic'
import React, { useEffect, useMemo, useState } from 'react'
import useTheme from '../../contexts/theme'
import { useGetHealthReportQuery } from '@/src/store/features/common/health-checks-api'
import { Card } from 'antd'
import dayjs from 'dayjs'

const Line = dynamic(
  () => import('@ant-design/charts').then((mod) => mod.Line) as any,
  { ssr: false },
)

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

const statusMap = {
  0: 'Unhealthy',
  1: 'At Risk',
  2: 'Healthy',
}

// TODO: copying the image in light mode works, but in dark mode it doesn't
// TODO: add empty state

const HealthReportChart: React.FC<HealthReportChartProps> = (
  props: HealthReportChartProps,
) => {
  const { currentThemeName, antDesignChartsTheme } = useTheme()

  const {
    data: healthReportData,
    isLoading,
    isFetching,
    error,
    refetch,
  } = useGetHealthReportQuery(props.objectId, { skip: !props.objectId })

  // Derive series data from health report data
  const seriesData = useMemo(() => {
    if (!healthReportData) return []

    return healthReportData
      .slice()
      .sort((a, b) =>
        dayjs(a.reportedOn).isAfter(dayjs(b.reportedOn)) ? 1 : -1,
      )
      .map((report) => ({
        date: dayjs(report.reportedOn).toDate(),
        status: convertStatusToNumber(report.status?.name),
      }))
  }, [healthReportData])

  // https://ant-design-charts.antgroup.com/en/options/plots/component/axis
  const config = useMemo(() => {
    const fontColor =
      currentThemeName === 'light'
        ? 'rgba(0, 0, 0, 0.45)'
        : 'rgba(255, 255, 255, 0.45)'

    return {
      title: {
        title: 'Health Report',
        style: {
          titleFontSize: 14,
          titleFontWeight: 'normal',
          titleFill: fontColor,
        },
      },
      theme: antDesignChartsTheme,
      height: 200,
      width: 350,
      data: seriesData,
      xField: 'date',
      yField: 'status',
      point: {
        size: 5,
        shape: 'circle',
      },
      tooltip: {
        title: (datum) => `${dayjs(datum.date).format('MMM D')}`, // Show full Date & Time
        items: [
          {
            channel: 'y',
            valueFormatter: (value) => statusMap[value],
            name: 'Status',
          },
          {
            channel: 'x',
            valueFormatter: (value) => dayjs(value).format('h:mm A'),
            name: 'Time',
          },
        ],
      },
      axis: {
        x: {
          gridStrokeOpacity: 0.3,
          labelFormatter: (value) => dayjs(value).format('MMM D'),
        },
        y: {
          labelFormatter: (value) => statusMap[value],
          gridStrokeOpacity: 0.3,
        },
      },
      scale: {
        y: {
          type: 'linear',
          domain: [0, 1, 2],
          tickMethod: () => [0, 1, 2],
        },
      },
    } as any
  }, [antDesignChartsTheme, currentThemeName, seriesData])

  return (
    <Card
      size="small"
      loading={isLoading}
      style={{ minHeight: 200, minWidth: 350 }}
    >
      <Line {...config} />
    </Card>
  )
}

export default HealthReportChart
