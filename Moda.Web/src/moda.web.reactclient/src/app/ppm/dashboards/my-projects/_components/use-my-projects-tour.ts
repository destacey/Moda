'use client'

import { useLocalStorageState } from '@/src/hooks'
import { TourProps } from 'antd'
import { useCallback, useRef } from 'react'

const TOUR_SEEN_KEY = 'my-projects-tour-seen'

export interface MyProjectsTourRefs {
  filterBarRef: React.RefObject<HTMLDivElement | null>
  summaryBarRef: React.RefObject<HTMLDivElement | null>
  leftPanelRef: React.RefObject<HTMLDivElement | null>
  rightPanelRef: React.RefObject<HTMLDivElement | null>
}

export interface MyProjectsTourResult {
  refs: MyProjectsTourRefs
  tourOpen: boolean
  tourSteps: TourProps['steps']
  onTourClose: () => void
  onTourStart: () => void
  /** The step index for the "Project Details" step, used to auto-select a project */
  detailStepIndex: number
}

export const useMyProjectsTour = (): MyProjectsTourResult => {
  const [tourSeen, setTourSeen] = useLocalStorageState(TOUR_SEEN_KEY, false)

  const filterBarRef = useRef<HTMLDivElement>(null)
  const summaryBarRef = useRef<HTMLDivElement>(null)
  const leftPanelRef = useRef<HTMLDivElement>(null)
  const rightPanelRef = useRef<HTMLDivElement>(null)

  const tourOpen = !tourSeen

  const onTourClose = useCallback(() => {
    setTourSeen(true)
  }, [setTourSeen])

  const onTourStart = useCallback(() => {
    setTourSeen(false)
  }, [setTourSeen])

  const detailStepIndex = 4

  const tourSteps: TourProps['steps'] = [
    {
      title: 'Welcome to My Projects',
      description:
        'This dashboard gives you a personalized view of all projects you are involved in. Let\'s walk through the key areas.',
      cover: null,
      target: null,
    },
    {
      title: 'Filter Your Projects',
      description:
        'Use these filters to narrow down projects by your role (Sponsor, Owner, PM, Member, or Task Assignee) and by project status. Your filter selections are saved automatically.',
      target: () => filterBarRef.current!,
      placement: 'bottom',
    },
    {
      title: 'Summary Metrics',
      description:
        'See aggregated task metrics across all visible projects at a glance — total project count, overdue tasks, tasks due this week, and upcoming tasks.',
      target: () => summaryBarRef.current!,
      placement: 'bottom',
    },
    {
      title: 'Project List',
      description:
        'Your projects are grouped by portfolio. Each card shows your role, project status, phase timeline, task statistics, and team members.',
      target: () => leftPanelRef.current!,
      placement: 'right',
    },
    {
      title: 'Project Details',
      description:
        'Clicking a project card opens its details here, including phases, task summary, and the full project plan with deliverables and tasks.',
      target: () => rightPanelRef.current!,
      placement: 'left',
    },
  ]

  return {
    refs: { filterBarRef, summaryBarRef, leftPanelRef, rightPanelRef },
    tourOpen,
    tourSteps,
    onTourClose,
    onTourStart,
    detailStepIndex,
  }
}
