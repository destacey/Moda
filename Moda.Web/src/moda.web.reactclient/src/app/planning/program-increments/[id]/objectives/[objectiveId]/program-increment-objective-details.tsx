import { ProgramIncrementObjectiveDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Progress, Row, Tooltip } from 'antd'
import Link from 'next/link'
import { ReactMarkdown } from 'react-markdown/lib/react-markdown'

const { Item } = Descriptions

const ProgramIncrementObjectiveDetails = (
  objective: ProgramIncrementObjectiveDetailsDto
) => {
  const startDate = objective.startDate
    ? new Date(objective.startDate)?.toLocaleDateString()
    : null

  const targetDate = objective.targetDate
    ? new Date(objective.targetDate)?.toLocaleDateString()
    : null

  const teamLink =
    objective.team?.type === 'Team'
      ? `/organizations/teams/${objective.team?.localId}`
      : `/organizations/team-of-teams/${objective.team?.localId}`

  return (
    <>
      <Row>
        <Col span={12} offset={6}>
          <Tooltip title="Progress">
            <Progress percent={objective.progress} />
          </Tooltip>
        </Col>
      </Row>
      <Row>
        <Col xs={24} md={12}>
          <Descriptions>
            <Item label="PI">
              <Link
                href={`/planning/program-increments/${objective.programIncrement?.localId}`}
              >
                {objective.programIncrement?.name}
              </Link>
            </Item>
            <Item label="Team">
              <Link href={teamLink}>{objective.team?.name}</Link>
            </Item>
            <Item label="Status">{objective.status?.name}</Item>
            <Item label="Is Stretch?">{objective.isStretch?.toString()}</Item>
            <Item label="Start">{startDate}</Item>
            <Item label="Target">{targetDate}</Item>
            <Item label="Type">{objective.type?.name}</Item>
          </Descriptions>
        </Col>
        <Col xs={24} md={12}>
          <Descriptions layout="vertical">
            <Item label="Description">
              <ReactMarkdown>{objective.description}</ReactMarkdown>
            </Item>
          </Descriptions>
        </Col>
      </Row>
    </>
  )
}

export default ProgramIncrementObjectiveDetails
