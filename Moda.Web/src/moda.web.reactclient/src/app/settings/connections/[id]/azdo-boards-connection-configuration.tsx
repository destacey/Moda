import { AzureDevOpsBoardsConnectionConfigurationDto } from '@/src/services/moda-api'
import { Descriptions } from 'antd'

const { Item } = Descriptions

interface AzdoBoardsConnectionConfigurationProps {
  configuration: AzureDevOpsBoardsConnectionConfigurationDto
}

const AzdoBoardsConnectionConfiguration = ({
  configuration,
}: AzdoBoardsConnectionConfigurationProps) => {
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

export default AzdoBoardsConnectionConfiguration
