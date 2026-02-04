'use client'

import {
  CompletionRateMetric,
  CycleTimeMetric,
  MetricCard,
  VelocityMetric,
} from '@/src/components/common/metrics'
import {
  IterationHealthIndicator,
  IterationProgressBar,
} from '@/src/components/common/planning'
import { IterationState } from '@/src/components/types'
import { SizingMethod, SprintMetricsSummary } from '@/src/services/moda-api'
import { Card, Col, Flex, Row, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { FC } from 'react'

const { Text } = Typography

interface SprintCardProps {
  sprint: SprintMetricsSummary
  sizingMethod: SizingMethod
}

const SprintCard: FC<SprintCardProps> = ({ sprint, sizingMethod }) => {
  const useStoryPoints = sizingMethod === SizingMethod.StoryPoints

  const displayTotal = useStoryPoints
    ? sprint.totalStoryPoints
    : sprint.totalWorkItems
  const displayCompleted = useStoryPoints
    ? sprint.completedStoryPoints
    : sprint.completedWorkItems
  const displayInProgress = useStoryPoints
    ? sprint.inProgressStoryPoints
    : sprint.inProgressWorkItems
  const displayNotStarted = useStoryPoints
    ? sprint.notStartedStoryPoints
    : sprint.notStartedWorkItems

  const formatDateRange = () => {
    const start = dayjs(sprint.start)
    const end = dayjs(sprint.end)
    return `${start.format('MMM D, YYYY h:mm A')} - ${end.format('MMM D, YYYY h:mm A')}`
  }

  const isFuture = sprint.state.id === IterationState.Future

  const metricCardStyle: React.CSSProperties = {
    height: '100%',
  }

  return (
    <Card
      size="small"
      hoverable
      styles={{
        body: { padding: 16 },
      }}
    >
      <Flex vertical gap="middle">
        {/* Header */}
        <Flex justify="space-between" align="flex-start">
          <Flex vertical gap={2}>
            <Link
              href={`/organizations/teams/${sprint.team.key}`}
              style={{ fontSize: 16, fontWeight: 600 }}
            >
              {sprint.team.name}
            </Link>
            <Link
              href={`/planning/sprints/${sprint.sprintKey}`}
              style={{ fontSize: 13 }}
            >
              {sprint.sprintName}
            </Link>
            <Text type="secondary" style={{ fontSize: 12 }}>
              {formatDateRange()}
            </Text>
          </Flex>
          <IterationHealthIndicator
            startDate={new Date(sprint.start)}
            endDate={new Date(sprint.end)}
            total={displayTotal}
            completed={displayCompleted}
          />
        </Flex>

        {/* Progress Bar - only show for active/completed sprints */}
        {!isFuture && (
          <IterationProgressBar
            startDate={new Date(sprint.start)}
            endDate={new Date(sprint.end)}
            total={displayTotal}
            completed={displayCompleted}
          />
        )}

        {/* Metrics Row */}
        <Row gutter={[8, 8]}>
          {isFuture ? (
            <Col xs={12} sm={8} md={6}>
              <MetricCard
                title="Total"
                value={displayTotal}
                tooltip="Total story points or items planned for this sprint"
                cardStyle={metricCardStyle}
              />
            </Col>
          ) : (
            <>
              <Col xs={12} sm={8} md={6}>
                <CompletionRateMetric
                  completed={displayCompleted}
                  total={displayTotal}
                  cardStyle={metricCardStyle}
                />
              </Col>
              <Col xs={12} sm={8} md={6}>
                <VelocityMetric
                  completed={displayCompleted}
                  total={displayTotal}
                  cardStyle={metricCardStyle}
                />
              </Col>
              <Col xs={12} sm={8} md={6}>
                <MetricCard
                  title="In Progress"
                  value={displayInProgress}
                  secondaryValue={`${displayNotStarted} not started`}
                  tooltip="Work items currently in progress"
                  cardStyle={metricCardStyle}
                />
              </Col>
              <Col xs={12} sm={8} md={6}>
                <CycleTimeMetric
                  value={sprint.averageCycleTimeDays ?? 0}
                  cardStyle={metricCardStyle}
                />
              </Col>
            </>
          )}
        </Row>
      </Flex>
    </Card>
  )
}

export default SprintCard
