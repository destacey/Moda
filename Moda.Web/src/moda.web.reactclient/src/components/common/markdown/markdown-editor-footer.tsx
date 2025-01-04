import React from 'react'
import { Typography, Flex } from 'antd'

const { Text } = Typography

interface MarkdownEditorFooterProps {
  currentLength: number
  maxLength: number
}

const MarkdownEditorFooter: React.FC<MarkdownEditorFooterProps> = ({
  currentLength,
  maxLength,
}) => {
  return (
    <Flex justify="space-between">
      <Text type="secondary">Markdown enabled</Text>
      <Text type="secondary">
        {currentLength} / {maxLength}
      </Text>
    </Flex>
  )
}

export default MarkdownEditorFooter
