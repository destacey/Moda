import { MenuOutlined } from '@ant-design/icons'
import { Button, Dropdown, Tag } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import ReactMarkdown from 'react-markdown'
import './moda-grid-cell-renderers.css'
import HealthCheckTag from './health-check/health-check-tag'
import {
  NavigationDto,
  PlanningHealthCheckDto,
  PlanningTeamNavigationDto,
  PlanningIntervalListDto,
  PlanningIntervalObjectiveDetailsDto,
  PlanningIntervalObjectiveListDto,
} from '@/src/services/moda-api'
import Link from 'next/link'

export interface HealthCheckStatusCellRendererProps {
  value: PlanningHealthCheckDto
}
export const HealthCheckStatusCellRenderer = ({
  value,
}: HealthCheckStatusCellRendererProps) => {
  if (!value) return null
  return <HealthCheckTag healthCheck={value} />
}

export const MarkdownCellRenderer = ({ value }) => {
  if (!value) return null
  return (
    <div className="grid-react-markdown">
      <ReactMarkdown>{value}</ReactMarkdown>
    </div>
  )
}

export interface PlanningTeamLinkCellRendererProps {
  value: PlanningTeamNavigationDto
}
export const PlanningTeamLinkCellRenderer = ({
  value,
}: PlanningTeamLinkCellRendererProps) => {
  if (!value) return null
  const teamLink =
    value.type === 'Team'
      ? `/organizations/teams/${value.key}`
      : `/organizations/team-of-teams/${value.key}`
  return <Link href={teamLink}>{value.name}</Link>
}

export interface PlanningIntervalObjectiveLinkCellRendererProps {
  data: PlanningIntervalObjectiveListDto
}
export const PlanningIntervalObjectiveLinkCellRenderer = ({
  data,
}: PlanningIntervalObjectiveLinkCellRendererProps) => {
  if (!data) return null
  return (
    <Link
      href={`/planning/planning-intervals/${data.planningInterval?.key}/objectives/${data.key}`}
    >
      {data.name}
    </Link>
  )
}

export interface PlanningIntervalLinkCellRendererProps {
  value: NavigationDto
}
export const PlanningIntervalLinkCellRenderer = ({
  value,
}: PlanningIntervalLinkCellRendererProps) => {
  if (!value) return null
  return (
    <Link href={`/planning/planning-intervals/${value.key}`}>{value.name}</Link>
  )
}

export interface RowMenuCellRendererProps {
  menuItems: ItemType[]
}
export const RowMenuCellRenderer = (props: RowMenuCellRendererProps) => {
  if (props.menuItems.length === 0) return null

  return (
    <Dropdown menu={{ items: props.menuItems }} trigger={['click']}>
      <Button type="text" size="small" icon={<MenuOutlined />} />
    </Dropdown>
  )
}
