import { ConnectionDetailsDto } from '@/src/services/moda-api'
import { Col, Descriptions, Row, Space } from 'antd'
import { ReactMarkdown } from 'react-markdown/lib/react-markdown'

const { Item } = Descriptions

interface ConnectionDetailsProps {
  connection: ConnectionDetailsDto
}

const ConnectionDetails = ({ connection }: ConnectionDetailsProps) => {
  if (!connection) return null
  return (
    <>
      <Row>
        <Col xs={24} md={12}>
          <Descriptions column={2}>
            <Item label="Connector">{connection.connector}</Item>
            <Item label="Is Active?">{connection.isActive ? 'Yes' : 'No'}</Item>
            <Item label="Is Valid Configuration?">
              {connection.isValidConfiguration ? 'Yes' : 'No'}
            </Item>
          </Descriptions>
        </Col>
        <Col xs={24} md={12}>
          <Descriptions layout="vertical">
            <Item label="Description">
              <Space direction="vertical">
                <ReactMarkdown>{connection.description}</ReactMarkdown>
              </Space>
            </Item>
          </Descriptions>
        </Col>
      </Row>
    </>
  )
}

export default ConnectionDetails
