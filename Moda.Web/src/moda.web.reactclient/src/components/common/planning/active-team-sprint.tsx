'use client'

import { useGetActiveSprintQuery } from '@/src/store/features/organizations/team-api'
import { useGetSprintMetricsQuery } from '@/src/store/features/planning/sprints-api'
import { Card, Col, Flex, Row, Skeleton, Typography } from 'antd'
import Link from 'next/link'
import { FC, useMemo, useState } from 'react'
import { CompletionRateMetric, VelocityMetric } from '../metrics'
import IterationDates from './iteration-dates'
import TimelineProgress from './timeline-progress'

const { Text } = Typography

export interface ActiveTeamSprintProps {
  teamId: string
}

const ActiveTeamSprint: FC<ActiveTeamSprintProps> = ({ teamId }) => {
  // TODO: update this based on team preferences (story points vs. count)
  const [useStoryPoints] = useState(true)

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
      <Text>Active Sprint</Text>
      <Link href={`/planning/sprints/${sprintData.key}`}>
        {sprintData.name}
      </Link>
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
            />
          </Col>
          <Col xs={24} sm={24} md={12} lg={12} xxl={12}>
            <VelocityMetric
              completed={displayValues.completed}
              total={displayValues.total}
            />
          </Col>
        </Row>
      </Flex>
    </Card>
  )
}

export default ActiveTeamSprint

