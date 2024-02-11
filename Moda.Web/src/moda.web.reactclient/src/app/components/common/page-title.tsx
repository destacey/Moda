import { Col, Row, Space, Typography } from 'antd'

const { Title, Text } = Typography

export interface PageTitleProps {
  title: string | React.ReactNode
  subtitle?: string
  tags?: React.ReactNode | null
  actions?: React.ReactNode | null
}

// TODO: align actions to the right/end when not the xs or sm breakpoint
const PageTitle = ({ title, subtitle, tags, actions }: PageTitleProps) => {
  const titleMdSize = actions ? 16 : 24
  return (
    <>
      <Row align={'middle'} style={{ marginBottom: '12px' }}>
        <Col xs={24} sm={24} md={titleMdSize}>
          <Space>
            <div>
              <Title level={2} style={{ margin: '0px', fontWeight: '400' }}>
                {title}
              </Title>
              {subtitle && <Text>{subtitle}</Text>}
            </div>
            {tags && <div>{tags}</div>}
          </Space>
        </Col>
        {actions && (
          <Col xs={24} sm={24} md={8}>
            <Space style={{ display: 'flex', justifyContent: 'flex-end' }}>
              {actions}
            </Space>
          </Col>
        )}
      </Row>
    </>
  )
}

export default PageTitle
