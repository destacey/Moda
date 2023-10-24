'use client'

import LinksCard from '@/src/app/components/common/links/links-card'
import { ProgramIncrementDetailsDto } from '@/src/services/moda-api'
import { Card, Col, Descriptions, Row, Space, Statistic } from 'antd'
import dayjs from 'dayjs'
import { daysRemaining } from '@/src/utils'
import ReactMarkdown from 'react-markdown'
import TeamPredictabilityRadarChart from './team-predictability-radar-chart'
import { useGetProgramIncrementPredictability } from '@/src/services/queries/planning-queries'
import { useMemo } from 'react'

const { Item } = Descriptions

interface ProgramIncrementDetailsProps {
  programIncrement: ProgramIncrementDetailsDto
}

const ProgramIncrementDetails = ({
  programIncrement,
}: ProgramIncrementDetailsProps) => {
  const { data: programPredictabilityData } =
    useGetProgramIncrementPredictability(programIncrement?.id)

  const daysCountdownMetric = useMemo(() => {
    if (!programIncrement) return null

    switch (programIncrement.state) {
      case 'Future':
        return (
          <Card>
            <Statistic
              title="Days Until Start"
              value={daysRemaining(programIncrement.start)}
              suffix="days"
            />
          </Card>
        )
      case 'Active':
        return (
          <Card>
            <Statistic
              title="Days Remaining"
              value={daysRemaining(programIncrement.end)}
              suffix="days"
            />
          </Card>
        )
      default:
        return null
    }
  }, [programIncrement])

  const programIncrementPredictability = useMemo(() => {
    if (!programIncrement || programIncrement.predictability == null)
      return null
    return (
      <Card>
        <Statistic
          title="PI Predictability"
          value={programIncrement.predictability}
          suffix="%"
        />
      </Card>
    )
  }, [programIncrement])

  const teamPredictabilityChart = useMemo(() => {
    if (
      !programIncrement ||
      programIncrement.state === 'Future' ||
      !programPredictabilityData
    )
      return null
    return (
      <Card size="small">
        <TeamPredictabilityRadarChart
          predictability={programPredictabilityData}
        />
      </Card>
    )
  }, [programIncrement, programPredictabilityData])

  if (!programIncrement) return null

  return (
    <>
      <Row>
        <Col xs={24} md={12}>
          <Descriptions>
            <Item label="Start">
              {dayjs(programIncrement.start).format('M/D/YYYY')}
            </Item>
            <Item label="End">
              {dayjs(programIncrement.end).format('M/D/YYYY')}
            </Item>
            <Item label="State">{programIncrement.state}</Item>
            <Item label="Objectives Locked?">
              {programIncrement.objectivesLocked ? 'Yes' : 'No'}
            </Item>
          </Descriptions>
        </Col>
        <Col xs={24} md={12}>
          <Descriptions layout="vertical">
            <Item label="Description">
              <Space direction="vertical">
                <ReactMarkdown>{programIncrement.description}</ReactMarkdown>
              </Space>
            </Item>
          </Descriptions>
        </Col>
      </Row>
      <Space align="start" wrap={true}>
        {daysCountdownMetric}
        {programIncrementPredictability}
        {teamPredictabilityChart}
        <LinksCard objectId={programIncrement.id} />
      </Space>
    </>
  )
}

export default ProgramIncrementDetails
