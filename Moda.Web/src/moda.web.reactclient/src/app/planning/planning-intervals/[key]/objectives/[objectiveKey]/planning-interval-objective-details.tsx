import HealthReportChart from '@/src/app/components/common/health-check/health-report-chart'
import LinksCard from '@/src/app/components/common/links/links-card'
import { PlanningIntervalObjectiveDetailsDto } from '@/src/services/moda-api'
import { Card, Col, Descriptions, Progress, Row, Space, Tooltip } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import ReactMarkdown from 'react-markdown'
import PlanningIntervalObjectiveWorkItemsCard from './planning-interval-objective-work-items-card'

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
                dayjs(objective.startDate).format('M/D/YYYY')}
            </Item>
            <Item label="Target">
              {objective.targetDate &&
                dayjs(objective.targetDate).format('M/D/YYYY')}
            </Item>
            <Item label="Type">{objective.type?.name}</Item>
          </Descriptions>
        </Col>
        <Col xs={24} md={12}>
          <Descriptions layout="vertical">
            <Item label="Description">
              <Space direction="vertical">
                <ReactMarkdown>{objective.description}</ReactMarkdown>
              </Space>
            </Item>
          </Descriptions>
        </Col>
      </Row>
      <Space align="start" wrap>
        <Card size="small">
          <HealthReportChart objectId={objective.id} />
        </Card>
        <PlanningIntervalObjectiveWorkItemsCard
          planningIntervalId={objective.planningInterval?.id}
          objectiveId={objective.id}
          canLinkWorkItems={canManageObjectives}
        />
        <LinksCard objectId={objective.id} />
      </Space>
    </>
  )
}

export default PlanningIntervalObjectiveDetails
