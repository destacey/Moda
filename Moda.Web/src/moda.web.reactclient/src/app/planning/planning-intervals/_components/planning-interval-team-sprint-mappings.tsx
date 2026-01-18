'use client'

import { useMemo } from 'react'
import Link from 'next/link'
import dayjs from 'dayjs'
import { Flex, Spin, Table, Tag, Tooltip, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
  PlanningIntervalDetailsDto,
  SprintListDto,
} from '@/src/services/moda-api'
import {
  useGetIterationSprintsQuery,
  useGetPlanningIntervalTeamsQuery,
} from '@/src/store/features/planning/planning-interval-api'
import ModaEmpty from '@/src/components/common/moda-empty'
import useTheme from '@/src/components/contexts/theme'

const { Text } = Typography

interface PlanningIntervalTeamSprintMappingsProps {
  planningInterval: PlanningIntervalDetailsDto
}

interface TeamRowData {
  key: string
  teamId: string
  teamKey: number
  teamName: string
  teamCode: string
  teamOfTeamsName: string | null
  sprintsByIteration: Record<string, SprintListDto | null>
}

const formatDateRange = (start: Date, end: Date): string => {
  return `${dayjs(start).format('MMM D')} - ${dayjs(end).format('MMM D')}`
}

const isActiveIteration = (start: Date, end: Date): boolean => {
  const today = dayjs().startOf('day')
  const startDate = dayjs(start).startOf('day')
  const endDate = dayjs(end).startOf('day')
  return (
    (today.isAfter(startDate) || today.isSame(startDate)) &&
    (today.isBefore(endDate) || today.isSame(endDate))
  )
}

interface SprintCellProps {
  sprint: SprintListDto | null
}

const SprintCell = ({ sprint }: SprintCellProps) => {
  if (!sprint) {
    return (
      <Tag color="default" style={{ width: '100%', textAlign: 'center' }}>
        No mapped sprint
      </Tag>
    )
  }

  return (
    <div style={{ textAlign: 'center' }}>
      <div>
        <Link href={`/planning/sprints/${sprint.key}`}>{sprint.name}</Link>
      </div>
      <Text type="secondary" style={{ fontSize: '11px' }}>
        {formatDateRange(sprint.start, sprint.end)}
      </Text>
    </div>
  )
}

export const PlanningIntervalTeamSprintMappings = ({
  planningInterval,
}: PlanningIntervalTeamSprintMappingsProps) => {
  const { token } = useTheme()
  const { data: teamsData, isLoading: teamsLoading } =
    useGetPlanningIntervalTeamsQuery(planningInterval.key)

  const { data: iterationSprintsData, isLoading: sprintsLoading } =
    useGetIterationSprintsQuery({
      idOrKey: planningInterval.key.toString(),
    })

  const isLoading = teamsLoading || sprintsLoading

  // Build sprint lookup by team and iteration
  const sprintsByTeamAndIteration = useMemo(() => {
    if (!iterationSprintsData) return new Map<string, SprintListDto>()

    const lookup = new Map<string, SprintListDto>()

    iterationSprintsData.forEach((iteration) => {
      iteration.sprints?.forEach((sprint) => {
        const key = `${sprint.team.id}:${iteration.id}`
        lookup.set(key, sprint)
      })
    })

    return lookup
  }, [iterationSprintsData])

  // Build table data with team rows, sorted alphabetically by team name
  const tableData = useMemo((): TeamRowData[] => {
    if (!teamsData || !iterationSprintsData) return []

    return [...teamsData]
      .sort((a, b) => a.name.localeCompare(b.name))
      .map((team) => {
        const sprintsByIteration: Record<string, SprintListDto | null> = {}

        iterationSprintsData.forEach((iteration) => {
          const key = `${team.id}:${iteration.id}`
          sprintsByIteration[iteration.id] =
            sprintsByTeamAndIteration.get(key) ?? null
        })

        return {
          key: team.id,
          teamId: team.id,
          teamKey: team.key,
          teamName: team.name,
          teamCode: team.code,
          teamOfTeamsName: team.teamOfTeams?.name ?? null,
          sprintsByIteration,
        }
      })
  }, [teamsData, iterationSprintsData, sprintsByTeamAndIteration])

  // Build table columns dynamically based on iterations
  const columns = useMemo((): ColumnsType<TeamRowData> => {
    const cols: ColumnsType<TeamRowData> = [
      {
        title: 'Team',
        dataIndex: 'teamName',
        key: 'team',
        fixed: 'left',
        width: 180,
        render: (_, record) => (
          <div>
            <Link href={`/organizations/teams/${record.teamKey}`}>
              {record.teamName}
            </Link>
            {record.teamOfTeamsName && (
              <div>
                <Text type="secondary" style={{ fontSize: '12px' }}>
                  {record.teamOfTeamsName}
                </Text>
              </div>
            )}
          </div>
        ),
      },
    ]

    if (iterationSprintsData) {
      const totalRows = teamsData?.length ?? 0

      iterationSprintsData.forEach((iteration) => {
        const isActive = isActiveIteration(iteration.start, iteration.end)
        const borderColor = token.colorPrimary
        const borderWidth = '2px'

        cols.push({
          title: (
            <div style={{ textAlign: 'center' }}>
              <div>{iteration.name}</div>
              <Text type="secondary" style={{ fontSize: '11px' }}>
                {formatDateRange(iteration.start, iteration.end)}
              </Text>
              <div>
                <Text type="secondary" style={{ fontSize: '11px' }}>
                  {iteration.category?.name}
                </Text>
              </div>
            </div>
          ),
          dataIndex: ['sprintsByIteration', iteration.id],
          key: iteration.id,
          width: 160,
          render: (sprint: SprintListDto | null) => (
            <SprintCell sprint={sprint} />
          ),
          onHeaderCell: () =>
            isActive
              ? {
                  style: {
                    borderTop: `${borderWidth} solid ${borderColor}`,
                    borderLeft: `${borderWidth} solid ${borderColor}`,
                    borderRight: `${borderWidth} solid ${borderColor}`,
                  },
                }
              : {},
          onCell: (_, index) =>
            isActive
              ? {
                  style: {
                    borderLeft: `${borderWidth} solid ${borderColor}`,
                    borderRight: `${borderWidth} solid ${borderColor}`,
                    ...(index === totalRows - 1 && {
                      borderBottom: `${borderWidth} solid ${borderColor}`,
                    }),
                  },
                }
              : {},
        })
      })
    }

    return cols
  }, [iterationSprintsData, token.colorPrimary, teamsData?.length])

  if (isLoading) {
    return (
      <Flex justify="center" align="center" style={{ padding: '48px' }}>
        <Spin size="large" />
      </Flex>
    )
  }

  if (!teamsData || teamsData.length === 0) {
    return (
      <ModaEmpty message="No teams configured for this planning interval" />
    )
  }

  return (
    <Table
      dataSource={tableData}
      columns={columns}
      pagination={false}
      scroll={{ x: 'max-content' }}
      size="small"
      bordered
    />
  )
}

export default PlanningIntervalTeamSprintMappings
