import { Flex, Tooltip, Typography } from 'antd'
import { ReactNode } from 'react'

const { Text } = Typography

interface LabeledContentProps {
  label: string
  tooltip?: string
  children: ReactNode
}

const LabeledContent = ({ label, tooltip, children }: LabeledContentProps) => (
  <Flex vertical gap={2}>
    {tooltip ? (
      <Tooltip title={tooltip}>
        <Text type="secondary" style={{ fontSize: 12, paddingTop: 1, cursor: 'help' }}>
          {label}
        </Text>
      </Tooltip>
    ) : (
      <Text type="secondary" style={{ fontSize: 12, paddingTop: 1 }}>
        {label}
      </Text>
    )}
    {children}
  </Flex>
)

export default LabeledContent
