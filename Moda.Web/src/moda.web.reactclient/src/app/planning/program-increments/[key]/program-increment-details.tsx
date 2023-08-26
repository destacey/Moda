import { ProgramIncrementDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Row, Space, Typography } from 'antd'
import dayjs from 'dayjs'
import { ReactMarkdown } from 'react-markdown/lib/react-markdown'

const { Item } = Descriptions

const ProgramIncrementDetails = (
  programIncrement: ProgramIncrementDetailsDto
) => {
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
    </>
  )
}

export default ProgramIncrementDetails
