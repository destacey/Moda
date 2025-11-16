'use client'

import {
  DaysCountdownMetric,
  MetricCard,
} from '@/src/components/common/metrics'
import useTheme from '@/src/components/contexts/theme'
import { IterationState, WorkStatusCategory } from '@/src/components/types'
import { SprintBacklogItemDto, SprintDetailsDto } from '@/src/services/moda-api'
import { Col, Flex, Row, Segmented, Tooltip } from 'antd'
import { FC, useMemo, useState } from 'react'

export interface SprintMetricsProps {
  sprint: SprintDetailsDto
  backlog: SprintBacklogItemDto[]
}

const SprintMetrics: FC<SprintMetricsProps> = ({ sprint, backlog }) => {
  const [useStoryPoints, setUseStoryPoints] = useState(true)

  const { token } = useTheme()

  const metrics = useMemo(() => {
    return backlog.reduce(
      (acc, item) => {
        const points = item.storyPoints ?? 0

        // Track items without story points
        if (
          item.storyPoints === null ||
          item.storyPoints === undefined ||
          item.storyPoints === 0
        ) {
          acc.missingStoryPoints += 1
        }

        switch (item.statusCategory.id) {
          case WorkStatusCategory.Done:
          case WorkStatusCategory.Removed:
            acc.completed += 1
            acc.completedStoryPoints += points
            break
          case WorkStatusCategory.Active:
            acc.inProgress += 1
            acc.inProgressStoryPoints += points
            break
          case WorkStatusCategory.Proposed:
            acc.notStarted += 1
            acc.notStartedStoryPoints += points
            break
        }

        return acc
      },
      {
        completed: 0,
        inProgress: 0,
        notStarted: 0,
        completedStoryPoints: 0,
        inProgressStoryPoints: 0,
        notStartedStoryPoints: 0,
        missingStoryPoints: 0,
      },
    )
  }, [backlog])

  const totalItems = backlog.length
  const totalStoryPoints =
    metrics.completedStoryPoints +
    metrics.inProgressStoryPoints +
    metrics.notStartedStoryPoints
  const completionRate =
    totalItems > 0 ? (metrics.completed / totalItems) * 100 : 0
  const storyPointsCompletionRate =
    totalStoryPoints > 0
      ? (metrics.completedStoryPoints / totalStoryPoints) * 100
      : 0

  const displayVelocity = useStoryPoints
    ? metrics.completedStoryPoints
    : metrics.completed
  const displayInProgress = useStoryPoints
    ? metrics.inProgressStoryPoints
    : metrics.inProgress
  const displayNotStarted = useStoryPoints
    ? metrics.notStartedStoryPoints
    : metrics.notStarted
  const displayTotal = useStoryPoints ? totalStoryPoints : totalItems
  const displayCompletionRate = useStoryPoints
    ? storyPointsCompletionRate
    : completionRate

  const velocityPercentage =
    displayTotal > 0
      ? `${((displayVelocity / displayTotal) * 100).toFixed(1)}%`
      : '0%'
  const inProgressPercentage =
    displayTotal > 0
      ? `${((displayInProgress / displayTotal) * 100).toFixed(1)}%`
      : '0%'
  const notStartedPercentage =
    displayTotal > 0
      ? `${((displayNotStarted / displayTotal) * 100).toFixed(1)}%`
      : '0%'
  const wipPercentage =
    displayTotal > 0
      ? `${((metrics.inProgress / displayTotal) * 100).toFixed(1)}%`
      : '0%'

  // Calculate average cycle time for completed items
  const averageCycleTime = useMemo(() => {
    const itemsWithCycleTime = backlog.filter(
      (item) =>
        item.cycleTime !== null &&
        item.cycleTime !== undefined &&
        item.statusCategory.id === WorkStatusCategory.Done,
    )

    if (itemsWithCycleTime.length === 0) return null

    const totalCycleTime = itemsWithCycleTime.reduce(
      (sum, item) => sum + (item.cycleTime ?? 0),
      0,
    )

    return totalCycleTime / itemsWithCycleTime.length
  }, [backlog])

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
          <MetricCard
            title="Completion Rate"
            value={displayCompletionRate}
            precision={1}
            suffix="%"
            tooltip="Percentage of story points or items currently in the sprint that are completed (Done or Removed)."
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <MetricCard
            title="Total"
            value={displayTotal}
            tooltip="Total number of story points or items currently in the sprint."
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <MetricCard
            title="Velocity"
            value={displayVelocity}
            valueStyle={{ color: token.colorSuccess }}
            secondaryValue={velocityPercentage}
            tooltip="Total number of story points or items currently in the sprint that are completed (Status Category: Done or Removed). Percentage shown represents the portion of total sprint work that is complete."
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <MetricCard
            title="In Progress"
            value={displayInProgress}
            valueStyle={{ color: token.colorInfo }}
            secondaryValue={inProgressPercentage}
            tooltip="Total number of story points or items currently in the sprint that are in progress (Status Category: Active). Percentage shown represents the portion of total sprint work that is in progress."
          />
        </Col>
        <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
          <MetricCard
            title="Not Started"
            value={displayNotStarted}
            secondaryValue={notStartedPercentage}
            tooltip="Total number of story points or items currently in the sprint that are not started (Status Category: Proposed). Percentage shown represents the portion of total sprint work that has not been started."
          />
        </Col>
        {sprint.state.id === IterationState.Active && (
          <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
            <MetricCard
              title="WIP"
              value={metrics.inProgress}
              secondaryValue={wipPercentage}
              tooltip="Work In Progress - Count of active work items (Status Category: Active). Percentage shown represents the portion of total sprint work that is currently in progress."
            />
          </Col>
        )}
        {averageCycleTime && (
          <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
            <MetricCard
              title="Avg Cycle Time"
              value={averageCycleTime}
              precision={2}
              suffix="days"
              tooltip="The average cycle time of done work items in the sprint (in days). Cycle time measures the time from when work starts (Activated) to when it's completed (Done)."
            />
          </Col>
        )}
        {useStoryPoints && (
          <Col xs={12} sm={8} md={6} lg={4} xxl={3}>
            <MetricCard
              title="Missing SPs"
              value={metrics.missingStoryPoints}
              valueStyle={{
                color:
                  metrics.missingStoryPoints === 0
                    ? token.colorSuccess
                    : token.colorError,
              }}
              tooltip="Number of work items in the sprint that don't have story points assigned."
            />
          </Col>
        )}
      </Row>
    </Flex>
  )
}

export default SprintMetrics

// missing
// pie chart by type
// burndown chart -- waiting for work item history
// burnup chart -- waiting for work item history

// removed
// scope creep

// dependencies

// sprint health based on planned vs time remaining
