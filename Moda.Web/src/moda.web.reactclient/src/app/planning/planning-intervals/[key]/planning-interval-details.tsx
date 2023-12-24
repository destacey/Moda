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

interface PlanningIntervalDetailsProps {
  planningInterval: PlanningIntervalDetailsDto
}

const PlanningIntervalDetails = ({
  planningInterval,
}: PlanningIntervalDetailsProps) => {
  const { data: piPredictabilityData } = useGetPlanningIntervalPredictability(
    planningInterval?.id,
  )

  const detailsItems: DescriptionsProps['items'] = [
    {
      key: 'start',
      label: 'Start',
      children: dayjs(planningInterval.start).format('M/D/YYYY'),
    },
    {
      key: 'end',
      label: 'End',
      children: dayjs(planningInterval.end).format('M/D/YYYY'),
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
      !piPredictabilityData
    )
      return null
    return (
      <Card size="small">
        <TeamPredictabilityRadarChart predictability={piPredictabilityData} />
      </Card>
    )
  }, [planningInterval, piPredictabilityData])

  if (!planningInterval) return null

  return (
    <>
      <Row>
        <Col xs={24} sm={24} md={16}>
          <Descriptions
            size="small"
            column={{ xs: 1, sm: 1, md: 2, lg: 3, xl: 3, xxl: 4 }}
            items={detailsItems}
          />
          {planningInterval.description && (
            <Descriptions layout="vertical" size="small">
              <Descriptions.Item label="Description">
                <ModaMarkdownDescription
                  content={planningInterval.description}
                />
              </Descriptions.Item>
            </Descriptions>
          )}
        </Col>
        <Col xs={24} sm={24} md={8}>
          <PlanningIntervalIterationsList id={planningInterval.id} />
        </Col>
      </Row>
      <Divider />
      <Space align="start" wrap>
        {daysCountdownMetric}
        {planningIntervalPredictability}
        {teamPredictabilityChart}
        <LinksCard objectId={planningInterval.id} />
      </Space>
    </>
  )
}

export default PlanningIntervalDetails
