'use client'

import { MetricCard } from '@/src/components/common/metrics'
import useTheme from '@/src/components/contexts/theme'
import { getProjectsClient } from '@/src/services/clients'
import { ProjectListDto, ProjectPlanSummaryDto } from '@/src/services/moda-api'
import { Col, Row, Skeleton } from 'antd'
import { FC, RefObject, useEffect, useMemo, useRef, useState } from 'react'
import styles from '../my-projects-dashboard.module.css'

export interface MyProjectsSummaryBarProps {
  projects: ProjectListDto[] | undefined
  isLoading: boolean
  containerRef?: RefObject<HTMLDivElement | null>
}

interface AggregatedMetrics {
  overdue: number
  dueThisWeek: number
  upcoming: number
}

const MyProjectsSummaryBar: FC<MyProjectsSummaryBarProps> = ({
  projects,
  isLoading,
  containerRef,
}) => {
  const { token } = useTheme()
  const [metrics, setMetrics] = useState<AggregatedMetrics>({
    overdue: 0,
    dueThisWeek: 0,
    upcoming: 0,
  })
  const [metricsLoading, setMetricsLoading] = useState(false)
  const staleRef = useRef(false)

  const projectKeys = useMemo(
    () => (projects ?? []).map((p) => p.key),
    [projects],
  )

  const EMPTY_METRICS: AggregatedMetrics = { overdue: 0, dueThisWeek: 0, upcoming: 0 }

  // Reset state synchronously during render when projectKeys change
  const [prevProjectKeys, setPrevProjectKeys] = useState(projectKeys)
  if (prevProjectKeys !== projectKeys) {
    setPrevProjectKeys(projectKeys)
    if (projectKeys.length === 0) {
      setMetrics(EMPTY_METRICS)
      setMetricsLoading(false)
    } else {
      setMetricsLoading(true)
    }
  }

  useEffect(() => {
    if (projectKeys.length === 0) return

    staleRef.current = false

    const client = getProjectsClient()
    const promises = projectKeys.map((key) =>
      client
        .getProjectPlanSummary(key)
        .catch(() => null as ProjectPlanSummaryDto | null),
    )

    Promise.all(promises).then((results) => {
      if (staleRef.current) return

      const aggregated = results.reduce<AggregatedMetrics>(
        (acc, summary) => {
          if (summary) {
            acc.overdue += summary.overdue
            acc.dueThisWeek += summary.dueThisWeek
            acc.upcoming += summary.upcoming
          }
          return acc
        },
        { overdue: 0, dueThisWeek: 0, upcoming: 0 },
      )

      setMetrics(aggregated)
      setMetricsLoading(false)
    })

    return () => { staleRef.current = true }
  }, [projectKeys])

  if (isLoading) {
    return (
      <div ref={containerRef} className={styles.summaryBar}>
        <Skeleton active paragraph={{ rows: 1 }} />
      </div>
    )
  }

  const projectCount = projects?.length ?? 0

  return (
    <div ref={containerRef} className={styles.summaryBar}>
      <Row gutter={[8, 8]}>
        <Col xs={8} sm={6} md={4} lg={3}>
          <MetricCard title="Projects" value={projectCount} />
        </Col>
        <Col xs={8} sm={6} md={4} lg={3}>
          <MetricCard
            title="Overdue"
            value={metricsLoading ? '-' : metrics.overdue}
            valueStyle={
              metrics.overdue > 0 ? { color: token.colorError } : undefined
            }
            tooltip="Sum of overdue tasks across visible projects"
          />
        </Col>
        <Col xs={8} sm={6} md={4} lg={3}>
          <MetricCard
            title="Due This Week"
            value={metricsLoading ? '-' : metrics.dueThisWeek}
            valueStyle={
              metrics.dueThisWeek > 0
                ? { color: token.colorWarning }
                : undefined
            }
            tooltip="Sum of tasks due this week across visible projects"
          />
        </Col>
        <Col xs={8} sm={6} md={4} lg={3}>
          <MetricCard
            title="Upcoming"
            value={metricsLoading ? '-' : metrics.upcoming}
            tooltip="Sum of tasks due next week across visible projects"
          />
        </Col>
      </Row>
    </div>
  )
}

export default MyProjectsSummaryBar
