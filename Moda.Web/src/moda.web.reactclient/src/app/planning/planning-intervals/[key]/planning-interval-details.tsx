'use client'

import LinksCard from '@/src/app/components/common/links/links-card'
import { PlanningIntervalDetailsDto } from '@/src/services/moda-api'
import { Card, Col, Descriptions, Divider, Row, Space, Statistic } from 'antd'
import dayjs from 'dayjs'
import { daysRemaining } from '@/src/utils'
import TeamPredictabilityRadarChart from './team-predictability-radar-chart'
import { useGetPlanningIntervalPredictability } from '@/src/services/queries/planning-queries'
import { useMemo } from 'react'

import type { DescriptionsProps } from 'antd'
import ModaMarkdownDescription from '@/src/app/components/common/moda-markdown-description'
import PlanningIntervalIterationsList from './planning-interval-iterations-list'
import ObjectiveStatusChart, {
  ObjectiveStatusChartDataItem,
} from './objective-status-chart'
import { useGetPlanningIntervalObjectivesQuery } from '@/src/store/features/planning/planning-interval-api'
import ObjectiveHealthChart, {
  ObjectiveHealthChartDataItem,
} from './objective-health-chart'

const { Item } = Descriptions

interface PlanningIntervalDetailsProps {
  planningInterval: PlanningIntervalDetailsDto
}

const PlanningIntervalDetails = ({
  planningInterval,
}: PlanningIntervalDetailsProps) => {
  const { data: piPredictabilityData } = useGetPlanningIntervalPredictability(
    planningInterval?.id,
  )

  const {
    data: objectivesData,
    isLoading: isLoadingObjectives,
    refetch: refectchObjectives,
  } = useGetPlanningIntervalObjectivesQuery(
    {
      planningIntervalId: planningInterval?.id,
      teamId: null,
    },
    { skip: !planningInterval?.id || planningInterval?.state === 'Future' },
  )

  const detailsItems: DescriptionsProps['items'] = [
    {
      key: 'start',
      label: 'Start',
      children: dayjs(planningInterval.start).format('MMM D, YYYY'),
    },
    {
      key: 'end',
      label: 'End',
      children: dayjs(planningInterval.end).format('MMM D, YYYY'),
    },
    {
      key: 'state',
      label: 'State',
      children: planningInterval.state,
    },
    {
      key: 'objectivesLocked',
      label: 'Objectives Locked?',
      children: planningInterval.objectivesLocked ? 'Yes' : 'No',
    },
  ]

  const daysCountdownMetric = useMemo(() => {
    if (!planningInterval) return null

    switch (planningInterval.state) {
      case 'Future':
        return (
          <Card>
            <Statistic
              title="Days Until Start"
              value={daysRemaining(planningInterval.start)}
              suffix="days"
            />
          </Card>
        )
      case 'Active':
        return (
          <Card>
            <Statistic
              title="Days Remaining"
              value={daysRemaining(planningInterval.end)}
              suffix="days"
            />
          </Card>
        )
      default:
        return null
    }
  }, [planningInterval])

  const planningIntervalPredictability = useMemo(() => {
    if (!planningInterval || planningInterval.predictability == null)
      return null
    return (
      <Card>
        <Statistic
          title="PI Predictability"
          value={planningInterval.predictability}
          suffix="%"
        />
      </Card>
    )
  }, [planningInterval])

  const teamPredictabilityChart = useMemo(() => {
    if (
      !planningInterval ||
      planningInterval.state === 'Future' ||
      !piPredictabilityData ||
      piPredictabilityData.predictability === null
    )
      return null
    return (
      <Card size="small">
        <TeamPredictabilityRadarChart predictability={piPredictabilityData} />
      </Card>
    )
  }, [planningInterval, piPredictabilityData])

  const { objectiveStatusData, objectiveHealthData } = useMemo(() => {
    if (!objectivesData)
      return { objectiveStatusData: [], objectiveHealthData: [] }
    const objectives = objectivesData.map((o) => ({
      status: o.status.name,
      health:
        o.status.name === 'Completed'
          ? 'Healthy'
          : o.status.name === 'Canceled' || o.status.name === 'Missed'
            ? 'Unhealthy'
            : (o.healthCheck?.status.name ?? 'Unknown'),
    }))

    // group by status with count
    const statusData = objectives.reduce(
      (acc, obj) => {
        acc[obj.status] = acc[obj.status] ? acc[obj.status] + 1 : 1
        return acc
      },
      {} as Record<string, number>,
    )

    const objectiveStatusData: ObjectiveStatusChartDataItem[] = Object.entries(
      statusData,
    ).map(([status, count]) => ({ type: status, count }))

    // group by status with count
    const healthData = objectives.reduce(
      (acc, obj) => {
        acc[obj.health] = acc[obj.health] ? acc[obj.health] + 1 : 1
        return acc
      },
      {} as Record<string, number>,
    )

    const objectiveHealthData: ObjectiveHealthChartDataItem[] = Object.entries(
      healthData,
    ).map(([health, count]) => ({ type: health, count }))

    return { objectiveStatusData, objectiveHealthData }
  }, [objectivesData])

  if (!planningInterval) return null

  return (
    <>
      <Row>
        <Col xs={24} sm={24} md={11} lg={13} xl={16}>
          <Descriptions
            size="small"
            column={{ xs: 1, sm: 1, md: 1, lg: 3, xl: 4, xxl: 5 }}
            items={detailsItems}
          />
          {planningInterval.description && (
            <Descriptions layout="vertical" size="small">
              <Item label="Description">
                <ModaMarkdownDescription
                  content={planningInterval.description}
                />
              </Item>
            </Descriptions>
          )}
        </Col>
        <Col xs={24} sm={24} md={13} lg={11} xl={8}>
          <PlanningIntervalIterationsList id={planningInterval.id} />
        </Col>
      </Row>
      <Divider />
      <Space align="start" wrap>
        {daysCountdownMetric}
        {planningIntervalPredictability}
        {teamPredictabilityChart}
        {planningInterval && planningInterval.state !== 'Future' && (
          <>
            <ObjectiveStatusChart data={objectiveStatusData} />
            <ObjectiveHealthChart data={objectiveHealthData} />
          </>
        )}
        <LinksCard objectId={planningInterval.id} />
      </Space>
    </>
  )
}

export default PlanningIntervalDetails
