'use client'

import { WorkItemProgressRollupDto } from '@/src/services/moda-api'
import { Progress, Tooltip } from 'antd'
import { round } from 'lodash'
import React, { useEffect } from 'react'
import { useState } from 'react'

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

const WorkProgress = (props: WorkProgressProps) => {
  const [progressSummary, setProgressSummary] =
    useState<ProgressSummary | null>(null)

  useEffect(() => {
    setProgressSummary(
      props.progress ? calculateProgressPercentages(props.progress) : null,
    )
  }, [props.progress])

  if (!progressSummary) return null

  const titleText = (
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

  return (
    <>
      {progressSummary && progressSummary.total > 0 && (
        <Tooltip title={titleText}>
          <Progress
            percent={
              progressSummary.activePercentage + progressSummary.donePercentage
            }
            format={() => progressSummary.donePercentage + '% done'}
            percentPosition={{ align: 'center', type: 'outer' }}
            success={{ percent: progressSummary.donePercentage }}
          />
        </Tooltip>
      )}
    </>
  )
}

export default WorkProgress
