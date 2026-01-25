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
import {
  PlanningIntervalIterationDetailsDto,
  SprintMetricsSummary,
} from '@/src/services/moda-api'
import { useGetPlanningIntervalIterationMetricsQuery } from '@/src/store/features/planning/planning-interval-api'
import {
  Card,
  Col,
  Flex,
  Row,
  Segmented,
  Skeleton,
  Tooltip,
  Typography,
} from 'antd'
import { FC, useMemo, useState } from 'react'

const { Text, Title } = Typography

export interface PlanningIntervalIterationSummaryProps {
  iteration: PlanningIntervalIterationDetailsDto
}

interface SprintCardProps {
  sprint: SprintMetricsSummary
  useStoryPoints: boolean
}

const SprintCard: FC<SprintCardProps> = ({ sprint, useStoryPoints }) => {
  const { token } = useTheme()

  const displayTotal = useStoryPoints
    ? sprint.totalStoryPoints
    : sprint.totalWorkItems
  const displayCompleted = useStoryPoints
    ? sprint.completedStoryPoints
    : sprint.completedWorkItems
  const completionRate =
    displayTotal > 0
      ? Number(((displayCompleted / displayTotal) * 100).toFixed(1))
      : 0

  return (
    <Card size="small" hoverable>
      <Flex vertical gap="small">
        <Flex justify="space-between" align="center">
          <Text strong>{sprint.team.name}</Text>
          <Text type="secondary">{sprint.sprintName}</Text>
        </Flex>
        <Flex justify="space-between" wrap="wrap" gap="small">
          <Tooltip title="Completion Rate">
            <Text>
              Completion:{' '}
              <Text
                strong
                style={{
                  color: completionRate >= 80 ? token.colorSuccess : undefined,
                }}
              >
                {completionRate.toFixed(1)}%
              </Text>
            </Text>
          </Tooltip>
          <Tooltip title="Velocity (completed)">
            <Text>
              Velocity:{' '}
              <Text strong style={{ color: token.colorSuccess }}>
                {displayCompleted}
              </Text>
              <Text type="secondary"> / {displayTotal}</Text>
            </Text>
          </Tooltip>
          {sprint.averageCycleTimeDays !== undefined &&
            sprint.averageCycleTimeDays !== null && (
              <Tooltip title="Average Cycle Time">
                <Text>
                  Cycle Time:{' '}
                  <Text strong>{sprint.averageCycleTimeDays.toFixed(1)}d</Text>
                </Text>
              </Tooltip>
            )}
        </Flex>
      </Flex>
    </Card>
  )
}

const PlanningIntervalIterationSummary: FC<
  PlanningIntervalIterationSummaryProps
> = ({ iteration }) => {
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

  if (isLoading || !metrics) {
    return <Skeleton active />
  }

  const displayTotal = useStoryPoints
    ? metrics.totalStoryPoints
    : metrics.totalWorkItems
  const displayCompleted = useStoryPoints
    ? metrics.completedStoryPoints
    : metrics.completedWorkItems
  const displayInProgress = useStoryPoints
    ? metrics.inProgressStoryPoints
    : metrics.inProgressWorkItems
  const displayNotStarted = useStoryPoints
    ? metrics.notStartedStoryPoints
    : metrics.notStartedWorkItems

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
              completed={displayCompleted}
              total={displayTotal}
              tooltip="Percentage of story points or items across all team sprints that are completed (Done or Removed)."
            />
          </Col>
        )}
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <MetricCard
            title="Total"
            value={displayTotal}
            tooltip="Total number of story points or items across all team sprints in this iteration."
          />
        </Col>
        {!isFuture && (
          <>
            <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
              <VelocityMetric
                completed={displayCompleted}
                total={displayTotal}
                tooltip="Total completed story points or items across all team sprints (Done or Removed)."
              />
            </Col>
            <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
              <StatusMetric
                title="In Progress"
                value={displayInProgress}
                total={displayTotal}
                color={token.colorInfo}
                tooltip="Total in-progress story points or items across all team sprints."
              />
            </Col>
            <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
              <StatusMetric
                title="Not Started"
                value={displayNotStarted}
                total={displayTotal}
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

      {sortedSprints.length > 0 && (
        <Flex vertical gap="small">
          <Title level={5} style={{ margin: 0 }}>
            Team Sprints
          </Title>
          <Row gutter={[8, 8]}>
            {sortedSprints.map((sprint) => (
              <Col key={sprint.sprintId} xs={24} sm={12} md={8} lg={6} xxl={4}>
                <SprintCard sprint={sprint} useStoryPoints={useStoryPoints} />
              </Col>
            ))}
          </Row>
        </Flex>
      )}
    </Flex>
  )
}

export default PlanningIntervalIterationSummary
