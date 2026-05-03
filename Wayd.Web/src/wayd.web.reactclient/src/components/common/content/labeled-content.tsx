import { Flex, Typography } from 'antd'
import WaydTooltip from '@/src/components/common/wayd-tooltip'
import { ReactNode } from 'react'

const { Text } = Typography

interface LabeledContentProps {
  label: string
  tooltip?: string
  children: ReactNode
}

const LabeledContent = ({ label, tooltip, children }: LabeledContentProps) => (
  <Flex vertical gap={2} align="flex-start">
    {tooltip ? (
      <WaydTooltip title={tooltip}>
        <Text
          type="secondary"
          style={{ fontSize: 12, paddingTop: 1, cursor: 'help', width: 'fit-content' }}
        >
          {label}
        </Text>
      </WaydTooltip>
    ) : (
      <Text type="secondary" style={{ fontSize: 12, paddingTop: 1 }}>
        {label}
      </Text>
    )}
    {children}
  </Flex>
)

export default LabeledContent
