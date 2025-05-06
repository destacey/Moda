'use client'

import LinksCard from '@/src/components/common/links/links-card'
import { PlanningIntervalObjectiveDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Progress, Row, Space, Tooltip } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import PlanningIntervalObjectiveWorkItemsCard from './planning-interval-objective-work-items-card'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import HealthReportChart from '@/src/components/common/health-check/health-report-chart'

const { Item } = Descriptions

interface PlanningIntervalObjectiveDetailsProps {
  objective: PlanningIntervalObjectiveDetailsDto
  canManageObjectives: boolean
}

const PlanningIntervalObjectiveDetails = ({
  objective,
  canManageObjectives,
}: PlanningIntervalObjectiveDetailsProps) => {
  if (!objective) return null

  const progressStatus = ['Canceled', 'Missed'].includes(objective.status?.name)
    ? 'exception'
    : undefined

  const teamLink =
    objective.team?.type === 'Team'
      ? `/organizations/teams/${objective.team?.key}`
      : `/organizations/team-of-teams/${objective.team?.key}`

  return (
    <>
      <Row>
        <Col span={12} offset={6}>
          <Tooltip title="Progress">
            <Progress percent={objective.progress} status={progressStatus} />
          </Tooltip>
        </Col>
      </Row>
      <Row>
        <Col xs={24} md={12}>
          <Descriptions>
            <Item label="PI">
              <Link
                href={`/planning/planning-intervals/${objective.planningInterval?.key}`}
              >
                {objective.planningInterval?.name}
              </Link>
            </Item>
            <Item label="Team">
              <Link href={teamLink}>{objective.team?.name}</Link>
            </Item>
            <Item label="Status">{objective.status?.name}</Item>
            <Item label="Is Stretch?">{objective.isStretch?.toString()}</Item>
            <Item label="Start">
              {objective.startDate &&
                dayjs(objective.startDate).format('MMM D, YYYY')}
            </Item>
            <Item label="Target">
              {objective.targetDate &&
                dayjs(objective.targetDate).format('MMM D, YYYY')}
            </Item>
            {objective?.closedDate && (
              <Item label="Closed Date">
                {dayjs(objective?.closedDate).format('MMM D, YYYY')}
              </Item>
            )}
            <Item label="Type">{objective.type?.name}</Item>
          </Descriptions>
        </Col>
        <Col xs={24} md={12}>
          <Descriptions layout="vertical">
            <Item label="Description">
              <MarkdownRenderer markdown={objective?.description} />
            </Item>
          </Descriptions>
        </Col>
      </Row>
      <Space align="start" wrap>
        <HealthReportChart objectId={objective.id} />
        <PlanningIntervalObjectiveWorkItemsCard
          planningIntervalKey={objective.planningInterval?.key}
          objectiveKey={objective.key}
          canLinkWorkItems={canManageObjectives}
          width={400}
        />
        <LinksCard objectId={objective.id} />
      </Space>
    </>
  )
}

export default PlanningIntervalObjectiveDetails
