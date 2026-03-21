'use client'

import { getProjectsClient } from '@/src/services/clients'
import { ProjectPlanSummaryDto } from '@/src/services/moda-api'
import { Flex } from 'antd'
import { FC, useEffect, useState } from 'react'
import styles from '../my-projects-dashboard.module.css'

const ProjectStatPills: FC<{ projectKey: string }> = ({ projectKey }) => {
  const [summary, setSummary] = useState<ProjectPlanSummaryDto | null>(null)

  useEffect(() => {
    let cancelled = false
    getProjectsClient()
      .getProjectPlanSummary(projectKey)
      .then((data) => {
        if (!cancelled) setSummary(data)
      })
      .catch(() => {})
    return () => {
      cancelled = true
    }
  }, [projectKey])

  if (!summary) return null

  const { overdue, dueThisWeek, upcoming } = summary
  if (overdue === 0 && dueThisWeek === 0 && upcoming === 0) return null

  return (
    <Flex gap={6} wrap>
      {overdue > 0 && (
        <span className={`${styles.statPill} ${styles.statPillOverdue}`}>
          {overdue} overdue
        </span>
      )}
      {dueThisWeek > 0 && (
        <span className={`${styles.statPill} ${styles.statPillDueThisWeek}`}>
          {dueThisWeek} this week
        </span>
      )}
      {upcoming > 0 && (
        <span className={`${styles.statPill} ${styles.statPillUpcoming}`}>
          {upcoming} upcoming
        </span>
      )}
    </Flex>
  )
}

export default ProjectStatPills
