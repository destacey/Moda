import { Col, Flex, Row, Space, Typography } from 'antd'
import { ReactNode } from 'react'

const { Title, Text } = Typography

export interface PageTitleProps {
  title: string | ReactNode
  subtitle?: string
  tags?: ReactNode | null
  actions?: ReactNode | null
  extra?: ReactNode | null
}

// TODO: align actions to the right/end when not the xs or sm breakpoint
const PageTitle = ({
  title,
  subtitle,
  tags,
  actions,
  extra,
}: PageTitleProps) => {
  const titleMdSize = actions ? 16 : 24
  return (
    <>
      <Flex vertical gap={8} style={{ marginBottom: 12 }}>
        <Row align={'middle'}>
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
        {extra && <Row>{extra}</Row>}
      </Flex>
    </>
  )
}

export default PageTitle
