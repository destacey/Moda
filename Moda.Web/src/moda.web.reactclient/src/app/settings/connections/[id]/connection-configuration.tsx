import { AzureDevOpsBoardsConnectionConfigurationDto } from '@/src/services/moda-api'
import { Descriptions } from 'antd'

const { Item } = Descriptions

interface ConnectionConfigurationProps {
  configuration: AzureDevOpsBoardsConnectionConfigurationDto
}

const ConnectionConfiguration = ({
  configuration,
}: ConnectionConfigurationProps) => {
  if (!configuration) return null

  return (
    <>
      <Descriptions column={1}>
        <Item label="Organization">{configuration.organization}</Item>
        <Item label="PAT">{configuration.personalAccessToken}</Item>
      </Descriptions>
    </>
  )
}

export default ConnectionConfiguration
