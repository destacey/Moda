'use client'

import { useGetActiveSprintQuery } from '@/src/store/features/organizations/team-api'
import { useGetSprintMetricsQuery } from '@/src/store/features/planning/sprints-api'
import { SizingMethod } from '@/src/services/moda-api'
import { Card, Col, Flex, Row, Skeleton, Typography } from 'antd'
import Link from 'next/link'
import { FC, useMemo } from 'react'
import { CompletionRateMetric, VelocityMetric } from '../metrics'
import TimelineProgress from './timeline-progress'
import IterationHealthIndicator from './iteration-health-indicator'

const { Text } = Typography

export interface ActiveTeamSprintProps {
  teamId: string
  sizingMethod: SizingMethod
}

const ActiveTeamSprint: FC<ActiveTeamSprintProps> = ({
  teamId,
  sizingMethod,
}) => {
  const useStoryPoints = sizingMethod === SizingMethod.StoryPoints

  const { data: sprintData, isLoading: sprintIsLoading } =
    useGetActiveSprintQuery(teamId)

  const { data: metrics, isLoading: metricsIsLoading } =
    useGetSprintMetricsQuery(sprintData?.key, {
      skip: !sprintData?.key,
    })

  const displayValues = useMemo(() => {
    if (!metrics) {
      return {
        total: 0,
        completed: 0,
      }
    }

    const displayTotal = useStoryPoints
      ? metrics.totalStoryPoints
      : metrics.totalWorkItems
    const displayCompleted = useStoryPoints
      ? metrics.completedStoryPoints
      : metrics.completedWorkItems

    return {
      total: displayTotal,
      completed: displayCompleted,
    }
  }, [metrics, useStoryPoints])

  if (sprintIsLoading) {
    return <Skeleton active paragraph={{ rows: 3 }} />
  }

  if (!sprintData) {
    return null
  }

  const title = (
    <Flex justify="space-between">
      <div>
        <Text>Active Sprint: </Text>
        <Link href={`/planning/sprints/${sprintData.key}`}>
          {sprintData.name}
        </Link>
      </div>
      <IterationHealthIndicator
        startDate={new Date(sprintData.start)}
        endDate={new Date(sprintData.end)}
        total={displayValues.total}
        completed={displayValues.completed}
      />
    </Flex>
  )

  return (
    <Card
      title={title}
      size="small"
      loading={metricsIsLoading}
      style={{ maxWidth: '450px' }}
    >
      <Flex vertical gap="small">
        <TimelineProgress
          start={sprintData.start}
          end={sprintData.end}
          dateFormat="MMM D - h:mm A"
          style={{ width: '100%' }}
        />
        <Row gutter={[8, 8]}>
          <Col xs={24} sm={24} md={12} lg={12} xxl={12}>
            <CompletionRateMetric
              completed={displayValues.completed}
              total={displayValues.total}
              tooltip={sizingMethod}
            />
          </Col>
          <Col xs={24} sm={24} md={12} lg={12} xxl={12}>
            <VelocityMetric
              completed={displayValues.completed}
              total={displayValues.total}
              tooltip={sizingMethod}
            />
          </Col>
        </Row>
      </Flex>
    </Card>
  )
}

export default ActiveTeamSprint

