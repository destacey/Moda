import { Form, FormInstance, Input } from 'antd'

const { Item } = Form

export interface ConfigSectionProps {
  form: FormInstance
  mode: 'create' | 'edit'
}

export const AzureOpenAIConfigurationSection: React.FC<ConfigSectionProps> = ({
}) => {
  return (
    <>
      <Item label="Base URL" name="baseUrl" rules={[{ required: true }]}>
        <Input
          maxLength={256}
          placeholder="https://your-resource.openai.azure.com"
        />
      </Item>

      <Item label="API Key" name="apiKey" rules={[{ required: true }]}>
        <Input.Password maxLength={256} />
      </Item>

      <Item
        label="Deployment Name"
        name="deploymentName"
        rules={[{ required: true }]}
      >
        <Input maxLength={128} placeholder="gpt-4" />
      </Item>
    </>
  )
}
