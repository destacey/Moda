'use client'

import {
  CompletionRateMetric,
  CycleTimeMetric,
  DaysCountdownMetric,
  HealthMetric,
  MetricCard,
  StatusMetric,
  VelocityMetric,
} from '@/src/components/common/metrics'
import useTheme from '@/src/components/contexts/theme'
import { IterationState } from '@/src/components/types'
import { SprintDetailsDto } from '@/src/services/moda-api'
import { useGetSprintMetricsQuery } from '@/src/store/features/planning/sprints-api'
import { Col, Flex, Row, Segmented, Skeleton, Tooltip } from 'antd'
import { FC, useMemo, useState } from 'react'

export interface SprintMetricsProps {
  sprint: SprintDetailsDto
}

const SprintMetrics: FC<SprintMetricsProps> = ({ sprint }) => {
  const [useStoryPoints, setUseStoryPoints] = useState(true)
  const { token } = useTheme()

  const { data: metrics, isLoading } = useGetSprintMetricsQuery(sprint.key)

  const displayValues = useMemo(() => {
    if (!metrics) {
      return {
        total: 0,
        completed: 0,
        inProgress: 0,
        notStarted: 0,
        completionRate: 0,
      }
    }

    const total = useStoryPoints
      ? metrics.totalStoryPoints
      : metrics.totalWorkItems
    const completed = useStoryPoints
      ? metrics.completedStoryPoints
      : metrics.completedWorkItems
    const inProgress = useStoryPoints
      ? metrics.inProgressStoryPoints
      : metrics.inProgressWorkItems
    const notStarted = useStoryPoints
      ? metrics.notStartedStoryPoints
      : metrics.notStartedWorkItems
    const completionRate =
      total > 0 ? Number(((completed / total) * 100).toFixed(1)) : 0

    return {
      total,
      completed,
      inProgress,
      notStarted,
      completionRate,
    }
  }, [metrics, useStoryPoints])

  if (isLoading) {
    return <Skeleton active paragraph={{ rows: 2 }} />
  }

  return (
    <Flex vertical gap="small">
      <Flex gap="small" justify="flex-end">
        <Tooltip title="Switch between summing story points and counting work items for metrics">
          <Segmented<string>
            options={['Story Points', 'Count']}
            value={useStoryPoints ? 'Story Points' : 'Count'}
            onChange={(value) =>
              setUseStoryPoints(value === 'Story Points' ? true : false)
            }
          />
        </Tooltip>
      </Flex>
      <Row gutter={[8, 8]}>
        {sprint.state.id !== IterationState.Completed && (
          <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
            <DaysCountdownMetric
              state={sprint.state.id as IterationState}
              startDate={sprint.start}
              endDate={sprint.end}
              style={{ height: '100%' }}
            />
          </Col>
        )}
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <CompletionRateMetric
            completed={displayValues.completed}
            total={displayValues.total}
            tooltip="Percentage of story points or items currently in the sprint that are completed (Done or Removed)."
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <MetricCard
            title="Total"
            value={displayValues.total}
            tooltip="Total number of story points or items currently in the sprint."
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <VelocityMetric
            completed={displayValues.completed}
            total={displayValues.total}
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <StatusMetric
            title="In Progress"
            value={displayValues.inProgress}
            total={displayValues.total}
            color={token.colorInfo}
            tooltip="Total number of story points or items currently in the sprint that are in progress (Status Category: Active). Percentage shown represents the portion of total sprint work that is in progress."
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <StatusMetric
            title="Not Started"
            value={displayValues.notStarted}
            total={displayValues.total}
            tooltip="Total number of story points or items currently in the sprint that are not started (Status Category: Proposed). Percentage shown represents the portion of total sprint work that has not been started."
          />
        </Col>
        {sprint.state.id === IterationState.Active && metrics && (
          <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
            <StatusMetric
              title="WIP"
              value={metrics.inProgressWorkItems}
              total={displayValues.total}
              tooltip="Work In Progress - Count of active work items (Status Category: Active). Percentage shown represents the portion of total sprint work that is currently in progress."
            />
          </Col>
        )}
        {metrics?.averageCycleTimeDays !== undefined &&
          metrics.averageCycleTimeDays !== null && (
            <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
              <CycleTimeMetric
                value={metrics.averageCycleTimeDays}
                tooltip="The average cycle time of done work items in the sprint (in days). Cycle time measures the time from when work starts (Activated) to when it's completed (Done)."
              />
            </Col>
          )}
        {useStoryPoints && metrics && (
          <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
            <HealthMetric
              title="Missing SPs"
              value={metrics.missingStoryPointsCount}
              tooltip="Number of work items in the sprint that don't have story points assigned."
            />
          </Col>
        )}
      </Row>
    </Flex>
  )
}

export default SprintMetrics
