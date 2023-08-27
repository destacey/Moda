import { ProgramIncrementObjectiveDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Progress, Row, Space, Tooltip } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { ReactMarkdown } from 'react-markdown/lib/react-markdown'

const { Item } = Descriptions

interface ProgramIncrementObjectiveDetailsProps {
  objective: ProgramIncrementObjectiveDetailsDto
}

const ProgramIncrementObjectiveDetails = ({
  objective,
}: ProgramIncrementObjectiveDetailsProps) => {
  if (!objective) return null

  const progressStatus =
    objective.status?.name === 'Canceled' ? 'exception' : undefined

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
                href={`/planning/program-increments/${objective.programIncrement?.key}`}
              >
                {objective.programIncrement?.name}
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
    </>
  )
}

export default ProgramIncrementObjectiveDetails