'use client'

import { CheckCircleFilled, CloseCircleFilled } from '@ant-design/icons'
import { ProjectPhaseListDto } from '@/src/services/moda-api'
import { Steps, Tooltip } from 'antd'
import dayjs from 'dayjs'
import { FC } from 'react'
import styles from './phase-timeline.module.css'

type PhaseStatus = 'completed' | 'in-progress' | 'not-started' | 'cancelled'

function mapPhaseStatus(statusName: string): PhaseStatus {
  switch (statusName) {
    case 'Completed':
      return 'completed'
    case 'In Progress':
      return 'in-progress'
    case 'Cancelled':
      return 'cancelled'
    default:
      return 'not-started'
  }
}

function mapStepStatus(
  status: PhaseStatus,
): 'finish' | 'process' | 'wait' | 'error' {
  switch (status) {
    case 'completed':
      return 'finish'
    case 'in-progress':
      return 'process'
    case 'cancelled':
      return 'error'
    default:
      return 'wait'
  }
}

function getIcon(status: PhaseStatus) {
  const tooltip = buildTooltip(status)
  switch (status) {
    case 'completed':
      return <Tooltip title={tooltip}><CheckCircleFilled className={styles.iconCompleted} /></Tooltip>
    case 'in-progress':
      return <Tooltip title={tooltip}><span className={styles.dotInProgress} /></Tooltip>
    case 'cancelled':
      return <Tooltip title={tooltip}><CloseCircleFilled className={styles.iconCancelled} /></Tooltip>
    default:
      return <Tooltip title={tooltip}><span className={styles.dotNotStarted} /></Tooltip>
  }
}

function formatDateRange(start?: Date, end?: Date): string | null {
  if (!start && !end) return null
  const format = 'MMM D, YYYY'
  if (start && end) {
    const startStr = dayjs(start).isSame(dayjs(end), 'year')
      ? dayjs(start).format('MMM D')
      : dayjs(start).format(format)
    return `${startStr} - ${dayjs(end).format(format)}`
  }
  if (start) return `Starts ${dayjs(start).format(format)}`
  return `Ends ${dayjs(end).format(format)}`
}

function buildTooltip(status: PhaseStatus) {
  const statusLabel = status.replace('-', ' ').replace(/\b\w/g, (c) => c.toUpperCase())
  return statusLabel
}

export interface PhaseTimelineProps {
  phases: ProjectPhaseListDto[]
}

const PhaseTimeline: FC<PhaseTimelineProps> = ({ phases }) => {
  if (phases.length === 0) return null

  const sorted = [...phases].sort((a, b) => a.order - b.order)

  const items = sorted.map((phase) => {
    const status = mapPhaseStatus(phase.status.name)
    const tooltip = buildTooltip(status)
    return {
      title: (
        <Tooltip title={tooltip}>
          {phase.name}
        </Tooltip>
      ),
      content: (() => {
        const dateRange = formatDateRange(phase.start, phase.end)
        const hasContent = dateRange || phase.progress != null
        if (!hasContent) return undefined
        return (
          <div className={styles.description}>
            {dateRange && <div className={styles.dates}>{dateRange}</div>}
            {phase.progress != null && <div className={styles.progress}>{phase.progress}%</div>}
          </div>
        )
      })(),
      status: mapStepStatus(status),
      icon: getIcon(status),
    }
  })

  return (
    <Steps
      items={items}
      size="small"
      titlePlacement="vertical"
    />
  )
}

export default PhaseTimeline
