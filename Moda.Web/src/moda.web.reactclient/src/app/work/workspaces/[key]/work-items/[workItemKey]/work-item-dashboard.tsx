'use client'

import {
  WorkItemDetailsDto,
  WorkItemProgressDailyRollupDto,
} from '@/src/services/moda-api'
import WorkItemCycleTime from './work-item-cycle-time'
import WorkItemTimeToStart from './work-item-time-to-start'
import { Card, Space, Spin } from 'antd'
import WorkItemLeadTime from './work-item-lead-time'
import { WorkItemsCumulativeFlowChart } from '@/src/app/components/common/work'
import { useGetWorkItemMetricsQuery } from '@/src/store/features/work-management/workspace-api'
import { useEffect } from 'react'
import WorkItemProgressPieChart from './work-item-progress-pie-chart'

export interface WorkItemDashboardProps {
  workItem: WorkItemDetailsDto
}

const WorkItemDashboard = ({ workItem }: WorkItemDashboardProps) => {
  const {
    data: metricsData,
    isLoading,
    isError,
  } = useGetWorkItemMetricsQuery(
    {
      idOrKey: workItem.workspace.key,
      workItemKey: workItem.key,
    },
    { skip: !workItem || workItem?.tier !== 'Portfolio' },
  )

  useEffect(() => {
    if (isError) {
      console.error('Error fetching work item metrics', isError)
    }
  }, [isError])

  if (!workItem) return null

  // get the last item from the CFD data
  const progress: WorkItemProgressDailyRollupDto =
    metricsData && metricsData.length > 0
      ? metricsData[metricsData.length - 1]
      : null

  return (
    <>
      <Space align="start" wrap style={{ marginBottom: 10 }}>
        <WorkItemTimeToStart workItem={workItem} />
        <WorkItemCycleTime workItem={workItem} />
        <WorkItemLeadTime workItem={workItem} />
        {workItem?.tier === 'Portfolio' && progress && (
          <WorkItemProgressPieChart progress={progress} isLoading={isLoading} />
        )}
      </Space>
      {workItem?.tier === 'Portfolio' && metricsData && (
        <Card size="small">
          <WorkItemsCumulativeFlowChart
            workItems={metricsData}
            isLoading={isLoading}
          />
        </Card>
      )}
    </>
  )
}

export default WorkItemDashboard
