'use client'

import { WorkItemProgressRollupDto } from '@/src/services/moda-api'
import { Progress, Tooltip } from 'antd'
import { round } from 'lodash'
import { memo, useMemo } from 'react'

export interface WorkProgressProps {
  progress: WorkItemProgressRollupDto
}

interface ProgressSummary {
  proposed: number
  proposedPercentage: number
  active: number
  activePercentage: number
  done: number
  donePercentage: number
  total: number
}
const calculateProgressPercentages = (
  rollup: WorkItemProgressRollupDto,
): ProgressSummary => {
  if (rollup.total === 0) {
    return {
      proposed: 0,
      proposedPercentage: 0,
      active: 0,
      activePercentage: 0,
      done: 0,
      donePercentage: 0,
      total: 0,
    }
  }

  return {
    proposed: rollup.proposed,
    proposedPercentage: round((rollup.proposed / rollup.total) * 100, 2),
    active: rollup.active,
    activePercentage: round((rollup.active / rollup.total) * 100, 2),
    done: rollup.done,
    donePercentage: round((rollup.done / rollup.total) * 100, 2),
    total: rollup.total,
  }
}

const WorkProgress = memo(({ progress }: WorkProgressProps) => {
  const progressSummary = useMemo(
    () => (progress ? calculateProgressPercentages(progress) : null),
    [progress],
  )

  const titleText = useMemo(() => {
    if (!progressSummary) return null
    return (
      <ul style={{ paddingLeft: 20 }}>
        <li>
          Proposed: {progressSummary.proposed} (
          {progressSummary.proposedPercentage}%)
        </li>
        <li>
          Active: {progressSummary.active} ({progressSummary.activePercentage}%)
        </li>
        <li>
          Done: {progressSummary.done} ({progressSummary.donePercentage}%)
        </li>
        <li>Total: {progressSummary.total}</li>
      </ul>
    )
  }, [progressSummary])

  if (!progressSummary) return null

  return (
    <>
      {progressSummary.total > 0 && (
        <Tooltip title={titleText}>
          <Progress
            percent={
              progressSummary.activePercentage + progressSummary.donePercentage
            }
            format={() => `${progressSummary.donePercentage}% done`}
            percentPosition={{ align: 'center', type: 'outer' }}
            success={{ percent: progressSummary.donePercentage }}
          />
        </Tooltip>
      )}
    </>
  )
})

// Set display name for the memoized component
WorkProgress.displayName = 'WorkProgress'

export default WorkProgress
