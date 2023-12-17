'use client'

import LinksCard from '@/src/app/components/common/links/links-card'
import { PlanningIntervalDetailsDto } from '@/src/services/moda-api'
import { Card, Col, Descriptions, Row, Space, Statistic } from 'antd'
import dayjs from 'dayjs'
import { daysRemaining } from '@/src/utils'
import ReactMarkdown from 'react-markdown'
import TeamPredictabilityRadarChart from './team-predictability-radar-chart'
import { useGetPlanningIntervalPredictability } from '@/src/services/queries/planning-queries'
import { useMemo } from 'react'

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
        <Col xs={24} md={12}>
          <Descriptions>
            <Item label="Start">
              {dayjs(planningInterval.start).format('M/D/YYYY')}
            </Item>
            <Item label="End">
              {dayjs(planningInterval.end).format('M/D/YYYY')}
            </Item>
            <Item label="State">{planningInterval.state}</Item>
            <Item label="Objectives Locked?">
              {planningInterval.objectivesLocked ? 'Yes' : 'No'}
            </Item>
          </Descriptions>
        </Col>
        <Col xs={24} md={12}>
          <Descriptions layout="vertical">
            <Item label="Description">
              <Space direction="vertical">
                <ReactMarkdown>{planningInterval.description}</ReactMarkdown>
              </Space>
            </Item>
          </Descriptions>
        </Col>
      </Row>
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
