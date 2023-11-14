import { MenuOutlined } from '@ant-design/icons'
import { Button, Dropdown, Tag } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import ReactMarkdown from 'react-markdown'
import './moda-grid-cell-renderers.css'
import HealthCheckTag from './health-check/health-check-tag'
import { PlanningHealthCheckDto } from '@/src/services/moda-api'

export interface ModaGridRowMenuCellRendererProps {
  menuItems: ItemType[]
}

export const ModaGridRowMenuCellRenderer = (
  props: ModaGridRowMenuCellRendererProps,
) => {
  if (props.menuItems.length === 0) return null

  return (
    <Dropdown menu={{ items: props.menuItems }} trigger={['click']}>
      <Button type="text" size="small" icon={<MenuOutlined />} />
    </Dropdown>
  )
}

export const MarkdownCellRenderer = ({ value }) => {
  if (!value) return null
  return (
    <div className="grid-react-markdown">
      <ReactMarkdown>{value}</ReactMarkdown>
    </div>
  )
}

export interface HealthCheckStatusCellRendererProps {
  value: PlanningHealthCheckDto
}
export const HealthCheckStatusCellRenderer = ({
  value,
}: HealthCheckStatusCellRendererProps) => {
  if (!value) return null
  return <HealthCheckTag healthCheck={value} />
}
