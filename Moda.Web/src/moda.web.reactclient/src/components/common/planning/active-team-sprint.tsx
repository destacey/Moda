'use client'

import { useGetActiveSprintQuery } from '@/src/store/features/organizations/team-api'
import { useGetSprintBacklogQuery } from '@/src/store/features/planning/sprints-api'
import { Card, Col, Flex, Row, Segmented, Tooltip, Typography } from 'antd'
import { useRouter } from 'next/navigation'
import { FC, useMemo, useState } from 'react'
import { SprintMetricsData, WorkStatusCategory } from '../../types'
import { MetricCard } from '../metrics'
import useTheme from '../../contexts/theme'
import Link from 'next/link'
import IterationDates from './iteration-dates'

const { Text } = Typography

export interface ActiveTeamSprintProps {
  teamId: string
}

const ActiveTeamSprint: FC<ActiveTeamSprintProps> = ({ teamId }) => {
  // TODO: update this based on team preferences (story points vs. count)
  const [useStoryPoints, setUseStoryPoints] = useState(true)

  const { token } = useTheme()
  const router = useRouter()

  const { data: sprintData, isLoading } = useGetActiveSprintQuery(teamId)

  const {
    data: workItemsData,
    isLoading: workItemsDataIsLoading,
    refetch: refetchWorkItemsData,
  } = useGetSprintBacklogQuery(sprintData?.key, {
    skip: !sprintData?.key,
  })

  const metrics = useMemo((): SprintMetricsData => {
    if (!workItemsData)
      return {
        completed: 0,
        inProgress: 0,
        notStarted: 0,
        completedStoryPoints: 0,
        inProgressStoryPoints: 0,
        notStartedStoryPoints: 0,
        missingStoryPoints: 0,
      }

    // TODO: move to a reusable function
    return workItemsData.reduce(
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
  }, [workItemsData])

  if (isLoading) {
    return null
  }

  if (!sprintData) {
    return null
  }

  const totalItems = workItemsData?.length || 0
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
  const displayTotal = useStoryPoints ? totalStoryPoints : totalItems
  const displayCompletionRate = useStoryPoints
    ? storyPointsCompletionRate
    : completionRate

  const velocityPercentage =
    displayTotal > 0
      ? `${((displayVelocity / displayTotal) * 100).toFixed(1)}%`
      : '0%'

  const title = (
    <Flex justify="space-between">
      <Text>Active Sprint</Text>
      <Link href={`/planning/sprints/${sprintData.key}`}>
        {sprintData.name}
      </Link>
    </Flex>
  )

  return (
    <Card title={title} size="small" style={{ maxWidth: '450px' }}>
      <Flex vertical gap="small">
        <IterationDates
          start={sprintData.start}
          end={sprintData.end}
          showDurationDays={false}
          style={{ width: '100%' }}
        />
        <Row gutter={[8, 8]}>
          <Col xs={24} sm={24} md={12} lg={12} xxl={12}>
            <MetricCard
              title="Completion Rate"
              value={displayCompletionRate}
              precision={1}
              suffix="%"
              tooltip="Percentage of story points currently in the sprint that are completed (Done or Removed)."
            />
          </Col>
          <Col xs={24} sm={24} md={12} lg={12} xxl={12}>
            <MetricCard
              title="Velocity"
              value={displayVelocity}
              valueStyle={{ color: token.colorSuccess }}
              secondaryValue={velocityPercentage}
              tooltip="Total number of story points currently in the sprint that are completed (Status Category: Done or Removed). Percentage shown represents the portion of total sprint work that is complete."
            />
          </Col>
        </Row>
      </Flex>
    </Card>
  )
}

export default ActiveTeamSprint
