import { ProgramIncrementDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Row } from 'antd'
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
          </Descriptions>
        </Col>
        <Col xs={24} md={12}>
          <Descriptions layout="vertical">
            <Item label="Description">
              <ReactMarkdown>{programIncrement.description}</ReactMarkdown>
            </Item>
          </Descriptions>
        </Col>
      </Row>
    </>
  )
}

export default ProgramIncrementDetails
