import { MoreOutlined } from '@ant-design/icons'
import { Button, Dropdown } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import './moda-grid-cell-renderers.css'
import HealthCheckTag from './health-check/health-check-tag'
import {
  NavigationDto,
  PlanningIntervalObjectiveListDto,
  SimpleNavigationDto,
} from '@/src/services/moda-api'
import Link from 'next/link'
import { MarkdownRenderer } from './markdown'

export interface HealthCheckStatusColumn {
  id?: string
  status?: SimpleNavigationDto
  expiration?: Date
}

export interface NestedHealthCheckStatusCellRendererProps {
  data: {
    healthCheck: HealthCheckStatusColumn | null
  } | null
}
export const NestedHealthCheckStatusCellRenderer = ({
  data,
}: NestedHealthCheckStatusCellRendererProps) => {
  return HealthCheckStatusCellRenderer({ data: data?.healthCheck })
}

export interface HealthCheckStatusCellRendererProps {
  data: HealthCheckStatusColumn | null
}
export const HealthCheckStatusCellRenderer = ({
  data,
}: HealthCheckStatusCellRendererProps) => {
  if (!data) return null
  return <HealthCheckTag healthCheck={data} />
}

export const MarkdownCellRenderer = ({ value }) => {
  if (!value) return null
  return (
    <div className="grid-react-markdown">
      <MarkdownRenderer markdown={value} />
    </div>
  )
}

export interface TeamNameLinkColumn extends NavigationDto {
  type?: string
}

export interface NestedTeamNameLinkCellRendererProps {
  data: {
    team: TeamNameLinkColumn | null
  } | null
}
export const NestedTeamNameLinkCellRenderer = ({
  data,
}: NestedTeamNameLinkCellRendererProps) => {
  return TeamNameLinkCellRenderer({ data: data?.team })
}

// TODO: how can we merge these two nested renderers?
export interface NestedTeamOfTeamsNameLinkCellRendererProps {
  data: {
    teamOfTeams: TeamNameLinkColumn | null
  } | null
}
export const NestedTeamOfTeamsNameLinkCellRenderer = ({
  data,
}: NestedTeamOfTeamsNameLinkCellRendererProps) => {
  return TeamNameLinkCellRenderer({ data: data?.teamOfTeams })
}

export interface TeamNameLinkCellRendererProps {
  data: TeamNameLinkColumn
}
export const TeamNameLinkCellRenderer = ({
  data,
}: TeamNameLinkCellRendererProps) => {
  if (!data) return null
  const teamLink =
    data.type === 'Team'
      ? `/organizations/teams/${data.key}`
      : `/organizations/team-of-teams/${data.key}`
  return <Link href={teamLink}>{data.name}</Link>
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

export interface NestedPlanningIntervalLinkCellRendererProps {
  data: {
    planningInterval: NavigationDto | null
  } | null
}
export const NestedPlanningIntervalLinkCellRenderer = ({
  data,
}: NestedPlanningIntervalLinkCellRendererProps) => {
  return PlanningIntervalLinkCellRenderer({ data: data?.planningInterval })
}

export interface PlanningIntervalLinkCellRendererProps {
  data: NavigationDto
}
export const PlanningIntervalLinkCellRenderer = ({
  data,
}: PlanningIntervalLinkCellRendererProps) => {
  if (!data) return null
  return (
    <Link href={`/planning/planning-intervals/${data.key}`}>{data.name}</Link>
  )
}

export interface RowMenuCellRendererProps {
  menuItems: ItemType[]
}
export const RowMenuCellRenderer = (props: RowMenuCellRendererProps) => {
  if (props.menuItems.length === 0) return null

  return (
    <Dropdown menu={{ items: props.menuItems }} trigger={['click']}>
      <Button type="text" size="small" icon={<MoreOutlined />} />
    </Dropdown>
  )
}

export interface WorkspaceLinkCellRendererProps {
  data: NavigationDto
}
export const WorkspaceLinkCellRenderer = ({
  data,
}: WorkspaceLinkCellRendererProps) => {
  if (!data) return null
  return <Link href={`/work/workspaces/${data.key}`}>{data.name}</Link>
}
