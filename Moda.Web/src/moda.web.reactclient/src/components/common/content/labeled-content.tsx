import { Flex, Typography } from 'antd'
import { ReactNode } from 'react'

const { Text } = Typography

interface LabeledContentProps {
  label: string
  children: ReactNode
}

const LabeledContent = ({ label, children }: LabeledContentProps) => (
  <Flex vertical gap={2}>
    <Text type="secondary" style={{ fontSize: 12, paddingTop: 1 }}>
      {label}
    </Text>
    {children}
  </Flex>
)

export default LabeledContent
