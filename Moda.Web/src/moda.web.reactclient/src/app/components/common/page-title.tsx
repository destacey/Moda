import { Button, Col, Row, Space, Typography } from 'antd'
import Profile from '../Profile'
import { Header } from 'antd/es/layout/layout'
import { RightCircleFilled } from '@ant-design/icons'

const { Title, Text } = Typography

export interface PageTitleProps {
  title: string
  subtitle?: string
  actions?: React.ReactNode | null
}

// TODO: align actions to the right/end when not the xs or sm breakpoint
const PageTitle = ({ title, subtitle, actions }: PageTitleProps) => {
  return (
    <>
      <Row align="middle" style={{ marginBottom: '12px' }}>
        <Col sm={24} md={12}>
          <Title level={2} style={{ marginBottom: '0px', fontWeight: '400' }}>
            {title}
          </Title>
          {subtitle && <Text>{subtitle}</Text>}
        </Col>
        {actions && (
          <Col sm={24} md={12}>
            {actions}
          </Col>
        )}
      </Row>
    </>
  )
}

export default PageTitle
