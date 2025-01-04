import { Typography } from 'antd'
import React from 'react'

const { Paragraph } = Typography

interface MarkdownTableProps
  extends React.DetailedHTMLProps<
    React.TableHTMLAttributes<HTMLTableElement>,
    HTMLTableElement
  > {}

const MarkdownTable: React.FC<MarkdownTableProps> = (props) => {
  // Check if the component has children
  const hasChildren = React.Children.count(props.children) > 0

  if (!hasChildren) {
    return null
  }

  return (
    <Paragraph
      style={{
        overflowX: 'auto',
      }}
    >
      <table {...props} />
    </Paragraph>
  )
}

export default MarkdownTable
