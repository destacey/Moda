import LinksCard from '@/src/app/components/common/links/links-card'
import { ProgramIncrementDetailsDto } from '@/src/services/moda-api'
import { Card, Col, Descriptions, Row, Space, Statistic } from 'antd'
import dayjs from 'dayjs'
import { programIncrementDaysRemaining } from '@/src/utils'
import ReactMarkdown from 'react-markdown'

const { Item } = Descriptions

interface ProgramIncrementDetailsProps {
  programIncrement: ProgramIncrementDetailsDto
}

const ProgramIncrementDetails = ({
  programIncrement,
}: ProgramIncrementDetailsProps) => {
  if (!programIncrement) return null

  const DaysRemaining = () => {
    const daysRemaining = programIncrementDaysRemaining(programIncrement.end)
    if (daysRemaining < 0) return null
    return (
      <Card>
        <Statistic title="Days Remaining" value={daysRemaining} suffix="days" />
      </Card>
    )
  }

  const ProgramIncrementPredictability = () => {
    if (programIncrement.predictability == null) return null
    return (
      <Card>
        <Statistic
          title="PI Predictability"
          value={programIncrement.predictability}
          suffix="%"
        />
      </Card>
    )
  }

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
      <Space align="start">
        <DaysRemaining />
        <ProgramIncrementPredictability />
        <LinksCard objectId={programIncrement.id} />
      </Space>
    </>
  )
}

export default ProgramIncrementDetails
