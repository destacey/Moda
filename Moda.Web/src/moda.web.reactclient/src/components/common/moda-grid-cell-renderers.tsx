import { MoreOutlined } from '@ant-design/icons'
import { Button, Dropdown } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import './moda-grid-cell-renderers.css'
import HealthCheckTag from './health-check/health-check-tag'
import {
  NavigationDto,
  PlanningIntervalObjectiveListDto,
  SimpleNavigationDto,
  WorkIterationNavigationDto,
} from '@/src/services/moda-api'
import Link from 'next/link'
import { MarkdownRenderer } from './markdown'
import { ICellRendererParams } from 'ag-grid-community'
import WorkStatusTag from './work/work-status-tag'
import { DependencyHealth, WorkStatusCategory } from '../types'
import DependencyHealthTag from './work/dependency-health-tag'

export interface HealthCheckStatusColumn {
  id: string
  status: SimpleNavigationDto
  expiration: Date
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

export const WorkStatusTagCellRenderer = ({
  value,
  data,
}: ICellRendererParams<any>) => {
  if (!data || !data.status) return null
  return (
    <WorkStatusTag
      status={value}
      category={data.statusCategory.id as WorkStatusCategory}
    />
  )
}

export const DependencyHealthCellRenderer = ({
  value,
  data,
}: ICellRendererParams<any>) => {
  if (!data || !data.health) return null
  return (
    <DependencyHealthTag
      name={value}
      health={data.health.id as DependencyHealth}
    />
  )
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

export interface PortfolioLinkCellRendererProps {
  data: NavigationDto
}
export const PortfolioLinkCellRenderer = ({
  data,
}: PortfolioLinkCellRendererProps) => {
  if (!data) return null
  return <Link href={`/ppm/portfolios/${data.key}`}>{data.name}</Link>
}

export interface ProjectLinkCellRendererProps {
  data: NavigationDto | { project: NavigationDto | null } | null
}

export const ProjectLinkCellRenderer = ({
  data,
}: ProjectLinkCellRendererProps) => {
  if (!data) return null

  // Handle both direct NavigationDto and nested project cases
  const projectData = 'project' in data ? data.project : data
  if (!projectData) return null

  return (
    <Link href={`/ppm/projects/${projectData.key}`}>{projectData.name}</Link>
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

export interface NestedWorkSprintLinkCellRendererProps {
  data: {
    sprint: WorkIterationNavigationDto | null
  } | null
  showTeamCode?: boolean
}
export const NestedWorkSprintLinkCellRenderer = ({
  data,
  showTeamCode = true,
}: NestedWorkSprintLinkCellRendererProps) => {
  return WorkSprintLinkCellRenderer({ data: data?.sprint, showTeamCode })
}

export interface WorkSprintLinkCellRendererProps {
  data: WorkIterationNavigationDto | null
  showTeamCode?: boolean
}
export const WorkSprintLinkCellRenderer = ({
  data,
  showTeamCode = true,
}: WorkSprintLinkCellRendererProps) => {
  if (!data) return null

  const displayText =
    showTeamCode && data.team?.code
      ? `${data.name} (${data.team.code})`
      : data.name

  return <Link href={`/planning/sprints/${data.key}`}>{displayText}</Link>
}
