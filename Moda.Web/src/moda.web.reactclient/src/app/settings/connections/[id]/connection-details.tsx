import {
  AzureDevOpsBoardsConnectionConfigurationDto,
  ConnectionDetailsDto,
} from '@/src/services/moda-api'
import { Col, Descriptions, Row, Space, Typography } from 'antd'
import { ReactMarkdown } from 'react-markdown/lib/react-markdown'
import ConnectionConfiguration from './connection-configuration'

const { Item } = Descriptions

interface ConnectionDetailsProps {
  connection: ConnectionDetailsDto
  configuration: AzureDevOpsBoardsConnectionConfigurationDto
}

const ConnectionDetails = ({
  connection,
  configuration,
}: ConnectionDetailsProps) => {
  if (!connection) return null
  return (
    <>
      <Row>
        <Col xs={24} md={12}>
          <Descriptions column={1}>
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
      <Row>
        <Typography.Title level={4}>Configuration</Typography.Title>
        <ConnectionConfiguration configuration={configuration} />
      </Row>
    </>
  )
}

export default ConnectionDetails
