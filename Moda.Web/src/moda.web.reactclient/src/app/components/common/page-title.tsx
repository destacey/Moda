import { Col, Row, Space, Typography } from 'antd'

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
      <Row align={'middle'} style={{ marginBottom: '12px' }}>
        <Col sm={24} md={12}>
          <Title level={2} style={{ margin: '0px', fontWeight: '400' }}>
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
