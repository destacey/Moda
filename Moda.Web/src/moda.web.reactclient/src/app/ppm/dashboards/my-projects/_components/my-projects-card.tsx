'use client'

import useAuth from '@/src/components/contexts/auth'
import { useGetMyProjectsSummaryQuery } from '@/src/store/features/ppm/projects-api'
import { RightOutlined } from '@ant-design/icons'
import { Badge, Card, Divider, Flex, Skeleton, Tag, Tooltip, Typography } from 'antd'
import { useRouter } from 'next/navigation'
import { FC, useMemo } from 'react'

const { Text, Title } = Typography

interface RoleCount {
  label: string
  count: number
}

function getRoleCounts(summary: {
  sponsorCount: number
  ownerCount: number
  managerCount: number
  memberCount: number
  assigneeCount: number
}): RoleCount[] {
  const entries: { label: string; count: number }[] = [
    { label: 'Sponsor', count: summary.sponsorCount },
    { label: 'Owner', count: summary.ownerCount },
    { label: 'PM', count: summary.managerCount },
    { label: 'Member', count: summary.memberCount },
    { label: 'Task Assignee', count: summary.assigneeCount },
  ]
  return entries.filter((e) => e.count > 0)
}

const MyProjectsCard: FC = () => {
  const { hasPermissionClaim } = useAuth()
  const router = useRouter()

  const canViewProjects = hasPermissionClaim('Permissions.Projects.View')

  const { data: summary, isLoading } = useGetMyProjectsSummaryQuery(
    { status: [5, 2] }, // Approved, Active
    { skip: !canViewProjects },
  )

  const roleCounts = useMemo(
    () => (summary ? getRoleCounts(summary) : []),
    [summary],
  )

  if (!canViewProjects) return null
  if (!isLoading && (!summary || summary.totalCount === 0)) return null

  return (
    <Card
      size="small"
      hoverable
      onClick={() => router.push('/ppm/dashboards/my-projects')}
      style={{ cursor: 'pointer' }}
    >
      {isLoading ? (
        <Skeleton active paragraph={{ rows: 2 }} />
      ) : (
        <Flex vertical gap={0}>
          <Flex justify="space-between" align="flex-start">
            <Flex vertical gap={2}>
              <Text
                type="secondary"
                strong
                style={{
                  fontSize: 11,
                  textTransform: 'uppercase',
                  letterSpacing: 0.5,
                }}
              >
                My Projects
              </Text>
              <Flex align="baseline" gap={6}>
                <Title level={3} style={{ margin: 0 }}>
                  {summary!.totalCount}
                </Title>
                <Text type="secondary">
                  {summary!.totalCount === 1 ? 'project' : 'projects'}
                </Text>
              </Flex>
            </Flex>
            <Text type="secondary" style={{ fontSize: 12 }}>
              View all <RightOutlined style={{ fontSize: 10 }} />
            </Text>
          </Flex>

          <Divider style={{ margin: '10px 0' }} />

          <Flex gap={6} wrap>
            {roleCounts.map(({ label, count }) => (
              <Tooltip
                key={label}
                title={`You are a ${label} on ${count} ${count === 1 ? 'project' : 'projects'}`}
              >
                <Tag>
                  {label}{' '}
                  <Badge
                    count={count}
                    color="var(--ant-color-primary)"
                    size="small"
                  />
                </Tag>
              </Tooltip>
            ))}
          </Flex>
        </Flex>
      )}
    </Card>
  )
}

export default MyProjectsCard
