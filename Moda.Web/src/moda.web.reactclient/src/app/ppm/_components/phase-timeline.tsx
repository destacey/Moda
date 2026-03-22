'use client'

import { CheckCircleFilled, CloseCircleFilled } from '@ant-design/icons'
import { ProjectPhaseListDto } from '@/src/services/moda-api'
import { Steps, Tooltip } from 'antd'
import dayjs from 'dayjs'
import { FC, useEffect, useRef, useState } from 'react'
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

function getIcon(status: PhaseStatus, tooltipContent: React.ReactNode) {
  switch (status) {
    case 'completed':
      return (
        <Tooltip title={tooltipContent}>
          <CheckCircleFilled className={styles.iconCompleted} />
        </Tooltip>
      )
    case 'in-progress':
      return (
        <Tooltip title={tooltipContent}>
          <span className={styles.dotInProgress} />
        </Tooltip>
      )
    case 'cancelled':
      return (
        <Tooltip title={tooltipContent}>
          <CloseCircleFilled className={styles.iconCancelled} />
        </Tooltip>
      )
    default:
      return (
        <Tooltip title={tooltipContent}>
          <span className={styles.dotNotStarted} />
        </Tooltip>
      )
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

type DisplayMode = 'default' | 'small' | 'vertical'

function buildTooltip(
  phase: ProjectPhaseListDto,
  status: PhaseStatus,
  mode: DisplayMode,
) {
  const statusLabel = status
    .replace('-', ' ')
    .replace(/\b\w/g, (c) => c.toUpperCase())

  // default and vertical show details inline — tooltip is just the status
  if (mode !== 'small') return statusLabel

  // small mode packs details into the tooltip
  const dateRange = formatDateRange(phase.start, phase.end)
  return (
    <div>
      <div>{statusLabel}</div>
      {dateRange && <div>{dateRange}</div>}
      {phase.progress != null && <div>Progress: {phase.progress}%</div>}
    </div>
  )
}

function buildContent(phase: ProjectPhaseListDto, mode: DisplayMode) {
  // only small (horizontal) hides inline content
  if (mode === 'small') return undefined

  const dateRange = formatDateRange(phase.start, phase.end)
  const hasContent = dateRange || phase.progress != null
  if (!hasContent) return undefined

  return (
    <div className={styles.description}>
      {dateRange && <div className={styles.dates}>{dateRange}</div>}
      {phase.progress != null && (
        <div className={styles.progress}>{phase.progress}%</div>
      )}
    </div>
  )
}

// Minimum width per step for each display mode
const DEFAULT_WIDTH_PER_STEP = 120
const VERTICAL_WIDTH_PER_STEP = 70
const PAGE_VERTICAL_THRESHOLD = 500

function getDisplayMode(
  containerWidth: number,
  stepCount: number,
): DisplayMode {
  if (window.innerWidth < PAGE_VERTICAL_THRESHOLD) return 'vertical'
  if (containerWidth >= stepCount * DEFAULT_WIDTH_PER_STEP) return 'default'
  if (containerWidth >= stepCount * VERTICAL_WIDTH_PER_STEP) return 'small'
  return 'vertical'
}

export interface PhaseTimelineProps {
  phases: ProjectPhaseListDto[]
  size?: 'default' | 'small'
}

const PhaseTimeline: FC<PhaseTimelineProps> = ({ phases, size }) => {
  const containerRef = useRef<HTMLDivElement>(null)
  const [autoMode, setAutoMode] = useState<DisplayMode>('default')
  const stepCount = phases.length

  useEffect(() => {
    if (size) return

    const el = containerRef.current
    if (!el) return

    const observer = new ResizeObserver((entries) => {
      const width = entries[0]?.contentRect.width ?? 0
      setAutoMode(getDisplayMode(width, stepCount))
    })

    observer.observe(el)
    return () => observer.disconnect()
  }, [size, stepCount])

  if (phases.length === 0) return null

  const mode: DisplayMode = size ?? autoMode
  const isVertical = mode === 'vertical'
  const stepsSize = mode === 'default' ? 'default' : 'small'
  const sorted = [...phases].sort((a, b) => a.order - b.order)

  const items = sorted.map((phase) => {
    const status = mapPhaseStatus(phase.status?.name)
    const tooltip = buildTooltip(phase, status, mode)
    return {
      title: (
        <Tooltip title={tooltip}>
          <span className={mode === 'small' ? styles.titleSmall : undefined}>
            {phase.name}
          </span>
        </Tooltip>
      ),
      content: buildContent(phase, mode),
      status: mapStepStatus(status),
      icon: getIcon(status, tooltip),
    }
  })

  return (
    <div ref={containerRef}>
      <Steps
        items={items}
        size={stepsSize}
        orientation={isVertical ? 'vertical' : 'horizontal'}
        titlePlacement={isVertical ? undefined : 'vertical'}
        responsive={false}
      />
    </div>
  )
}

export default PhaseTimeline
