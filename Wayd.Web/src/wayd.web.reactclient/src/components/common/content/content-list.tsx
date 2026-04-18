import { Typography } from 'antd'

const { Text } = Typography

interface ContentListProps {
  items: string[]
  emptyText?: string
}

const ContentList = ({ items, emptyText = undefined }: ContentListProps) =>
  items.length === 0 ? (
    emptyText ? (
      <Text>{emptyText}</Text>
    ) : null
  ) : (
    <ul style={{ margin: 0, paddingLeft: 16 }}>
      {items.map((name) => (
        <li key={name}>
          <Text>{name}</Text>
        </li>
      ))}
    </ul>
  )

export default ContentList
