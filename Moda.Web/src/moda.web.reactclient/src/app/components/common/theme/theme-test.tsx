import { Button, Card, Space } from 'antd'
import useTheme from '../../contexts/theme/use-theme'

const ThemeTest = () => {
  const { currentThemeName, token } = useTheme()

  return (
    <Card title="Theme Test">
      <Space direction="vertical">
        <p>Current Theme: {currentThemeName}</p>
        <p>Primary Color: {token.colorPrimary}</p>
        <p>Border Radius: {token.borderRadius}px</p>
        <p>Wireframe: {String(token.wireframe)}</p>
        <Button type="primary">Test Button</Button>
      </Space>
    </Card>
  )
}

export default ThemeTest
