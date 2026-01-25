'use client'

import {
  CompletionRateMetric,
  CycleTimeMetric,
  HealthMetric,
  MetricCard,
  StatusMetric,
  VelocityMetric,
} from '@/src/components/common/metrics'
import TimelineProgress from '@/src/components/common/planning/timeline-progress'
import useTheme from '@/src/components/contexts/theme'
import { PlanningIntervalIterationDetailsDto } from '@/src/services/moda-api'
import { useGetPlanningIntervalIterationMetricsQuery } from '@/src/store/features/planning/planning-interval-api'
import {
  Col,
  Divider,
  Flex,
  Row,
  Segmented,
  Skeleton,
  Tooltip,
  Typography,
} from 'antd'
import { FC, ReactNode, useEffect, useMemo, useState } from 'react'
import { SprintCard } from '.'
import { IterationHealthIndicator } from '@/src/components/common/planning'

const { Title } = Typography

export interface PlanningIntervalIterationSummaryProps {
  iteration: PlanningIntervalIterationDetailsDto
  onHealthIndicatorReady?: (indicator: ReactNode) => void
}

const PlanningIntervalIterationSummary: FC<
  PlanningIntervalIterationSummaryProps
> = ({ iteration, onHealthIndicatorReady }) => {
  const [useStoryPoints, setUseStoryPoints] = useState(true)
  const { token } = useTheme()

  const { data: metrics, isLoading } =
    useGetPlanningIntervalIterationMetricsQuery({
      planningIntervalKey: iteration.planningInterval.key,
      iterationKey: iteration.key,
    })

  const sortedSprints = useMemo(() => {
    if (!metrics) return []
    return [...metrics.sprintMetrics].sort((a, b) =>
      a.team.name.localeCompare(b.team.name),
    )
  }, [metrics])

  const displayValues = useMemo(() => {
    if (!metrics) {
      return {
        total: 0,
        completed: 0,
        inProgress: 0,
        notStarted: 0,
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

    return {
      total,
      completed,
      inProgress,
      notStarted,
    }
  }, [metrics, useStoryPoints])

  // Notify parent when health indicator is ready
  useEffect(() => {
    if (!isLoading && metrics && onHealthIndicatorReady) {
      onHealthIndicatorReady(
        <IterationHealthIndicator
          startDate={new Date(iteration.start)}
          endDate={new Date(iteration.end)}
          total={displayValues.total}
          completed={displayValues.completed}
        />,
      )
    }
  }, [
    displayValues.completed,
    displayValues.total,
    isLoading,
    metrics,
    onHealthIndicatorReady,
    iteration.end,
    iteration.start,
  ])

  if (isLoading || !metrics) {
    return <Skeleton active />
  }

  const isFuture = iteration.state === 'Future'

  return (
    <Flex vertical gap="middle">
      <TimelineProgress
        start={iteration.start}
        end={iteration.end}
        dateFormat="MMM D"
      />
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
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <MetricCard
            title="Team Sprints"
            value={metrics.teamCount}
            tooltip="Number of teams with sprints mapped to this iteration."
          />
        </Col>
        {!isFuture && (
          <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
            <CompletionRateMetric
              completed={displayValues.completed}
              total={displayValues.total}
              tooltip="Percentage of story points or items across all team sprints that are completed (Done or Removed)."
            />
          </Col>
        )}
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <MetricCard
            title="Total"
            value={displayValues.total}
            tooltip="Total number of story points or items across all team sprints in this iteration."
          />
        </Col>
        {!isFuture && (
          <>
            <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
              <VelocityMetric
                completed={displayValues.completed}
                total={displayValues.total}
                tooltip="Total completed story points or items across all team sprints (Done or Removed)."
              />
            </Col>
            <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
              <StatusMetric
                title="In Progress"
                value={displayValues.inProgress}
                total={displayValues.total}
                color={token.colorInfo}
                tooltip="Total in-progress story points or items across all team sprints."
              />
            </Col>
            <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
              <StatusMetric
                title="Not Started"
                value={displayValues.notStarted}
                total={displayValues.total}
                tooltip="Total not-started story points or items across all team sprints."
              />
            </Col>
            {metrics.averageCycleTimeDays !== undefined &&
              metrics.averageCycleTimeDays !== null && (
                <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
                  <CycleTimeMetric
                    value={metrics.averageCycleTimeDays}
                    tooltip="Average cycle time of completed (Done) work items across all team sprints."
                  />
                </Col>
              )}
            {useStoryPoints && (
              <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
                <HealthMetric
                  title="Missing SPs"
                  value={metrics.missingStoryPointsCount}
                  tooltip="Number of work items across all team sprints that don't have story points assigned."
                />
              </Col>
            )}
          </>
        )}
      </Row>

      <Divider />

      {sortedSprints.length > 0 && (
        <Flex vertical gap="small">
          <Title level={5} style={{ margin: 0 }}>
            Team Sprints
          </Title>
          <Flex vertical gap={8}>
            {sortedSprints.map((sprint) => (
              <SprintCard
                key={sprint.sprintId}
                sprint={sprint}
                useStoryPoints={useStoryPoints}
              />
            ))}
          </Flex>
        </Flex>
      )}
    </Flex>
  )
}

export default PlanningIntervalIterationSummary
