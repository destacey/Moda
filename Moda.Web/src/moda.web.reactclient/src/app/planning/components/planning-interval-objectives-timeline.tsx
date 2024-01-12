'use client'

import {
  DataItem,
  Timeline,
  TimelineOptions,
} from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import 'vis-timeline/styles/vis-timeline-graph2d.css'
import './planning-interval-objectives-timeline.css'
import {
  PlanningIntervalCalendarDto,
  PlanningIntervalObjectiveListDto,
} from '@/src/services/moda-api'
import dayjs from 'dayjs'
import React, { useEffect, useMemo, useState } from 'react'
import { createRoot } from 'react-dom/client'
import { ModaEmpty } from '../../components/common'
import { UseQueryResult } from 'react-query'
import { DataGroup } from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import Link from 'next/link'
import { Space, Typography } from 'antd'
import useTheme from '../../components/contexts/theme'

interface PlanningIntervalObjectivesTimelineProps {
  objectivesQuery: UseQueryResult<PlanningIntervalObjectiveListDto[], unknown>
  planningIntervalCalendarQuery: UseQueryResult<
    PlanningIntervalCalendarDto,
    unknown
  >
  enableGroups?: boolean
  teamNames?: string[]
  viewSelector?: React.ReactNode
}

interface TimelineItem extends DataItem {
  id: number
  planningIntervalKey: number
  title?: string
  content: string
  start: Date
  end: Date
  group?: string
  type?: string
  objective?: PlanningIntervalObjectiveListDto
}

interface ObjectiveTimelineTemplateProps {
  objective: PlanningIntervalObjectiveListDto
  timelineFontColor: string
  timelineForegroundColor: string
}
export const ObjectiveTimelineTemplate = ({
  objective,
  timelineFontColor,
  timelineForegroundColor,
}: ObjectiveTimelineTemplateProps) => {
  return (
    <div
      style={{
        width: `${objective.progress}%`,
        backgroundColor: timelineForegroundColor,
      }}
    >
      <Typography.Text style={{ padding: '5px', color: timelineFontColor }}>
        <Link
          href={`/planning/planning-intervals/${objective.planningInterval.key}/objectives/${objective.key}`}
        >
          {objective.key}
        </Link>
        <span> - </span> {objective.name}
      </Typography.Text>
    </div>
  )
}

const PlanningIntervalObjectivesTimeline = ({
  objectivesQuery,
  planningIntervalCalendarQuery,
  enableGroups = false,
  teamNames,
  viewSelector,
}: PlanningIntervalObjectivesTimelineProps) => {
  const [piStart, setPiStart] = useState<Date>(undefined)
  const [piEnd, setPiEnd] = useState<Date>(undefined)
  const [iterations, setIterations] = useState<TimelineItem[]>([])
  const [objectives, setObjectives] = useState<TimelineItem[]>([])
  const { currentThemeName } = useTheme()
  const timelineBackgroundColor =
    currentThemeName === 'light' ? '#f5f5f5' : '#303030'
  const timelineForegroundColor =
    currentThemeName === 'light' ? '#c7edff' : '#17354d'
  const timelineFontColor = currentThemeName === 'light' ? '#4d4d4d' : '#FFFFFF'

  // TODO: add the ability to export/save as svg or png
  // TODO: update the styles to match the rest of the app.  Especially for dark mode.
  const options = useMemo(() => {
    return {
      editable: false,
      orientation: 'top',
      maxHeight: 700,
      minHeight: 200,
      moveable: true,
      showCurrentTime: true,
      verticalScroll: true,
      zoomKey: 'ctrlKey',
      start: dayjs(piStart).subtract(1, 'week').toDate(),
      end: dayjs(piEnd).add(1, 'week').toDate(),
      min: dayjs(piStart).subtract(4, 'week').toDate(),
      max: dayjs(piEnd).add(4, 'week').toDate(),
      groupOrder: 'content',
      xss: { disabled: false },
      template: (item: TimelineItem, element: HTMLElement, data: any) => {
        if (item.type === 'range') {
          // TODO: this is throwing a lot of warnings in the console.  You are calling ReactDOMClient.createRoot() on a container that has already been passed to createRoot() before. Instead, call root.render() on the existing root instead if you want to update it.
          createRoot(element).render(
            <ObjectiveTimelineTemplate
              objective={item.objective}
              timelineFontColor={timelineFontColor}
              timelineForegroundColor={timelineForegroundColor}
            />,
          )
        } else if (item.type === 'background') {
          // TODO: styling could use some work
          createRoot(element).render(
            <div>
              <Typography.Text>{item.title}</Typography.Text>
            </div>,
          )
        }
      },
    }
  }, [piStart, piEnd, timelineFontColor, timelineForegroundColor])

  useEffect(() => {
    if (!objectivesQuery?.data || !planningIntervalCalendarQuery?.data) return

    setPiStart(planningIntervalCalendarQuery.data.start)
    setPiEnd(planningIntervalCalendarQuery.data.end)

    setIterations(
      planningIntervalCalendarQuery.data.iterationSchedules.map((i, index) => {
        return {
          id: i.key,
          planningIntervalKey: planningIntervalCalendarQuery.data.key,
          title: i.name,
          content: i.name,
          start: dayjs(i.start).toDate(),
          end: dayjs(i.end).add(1, 'day').subtract(1, 'second').toDate(),
          className: 'iteration-background',
          type: 'background',
        } as TimelineItem
      }),
    )

    setObjectives(
      objectivesQuery?.data
        .filter((obj) => obj.status?.name !== 'Canceled')
        .map((obj) => {
          return {
            id: obj.key,
            planningIntervalKey: obj.planningInterval.key,
            title: `${obj.name} (${obj.status?.name}) - ${obj.progress}%`,
            content: '',
            start: dayjs(
              obj.startDate ?? planningIntervalCalendarQuery.data.start,
            ).toDate(),
            end: dayjs(
              obj.targetDate ?? planningIntervalCalendarQuery.data.end,
            ).toDate(),
            group: obj.team?.name,
            type: 'range',
            style: `background: ${timelineBackgroundColor}; border-color: ${timelineBackgroundColor};`,
            zIndex: 1,
            objective: obj,
          } as TimelineItem
        }),
    )
  }, [
    objectivesQuery?.data,
    planningIntervalCalendarQuery.data,
    timelineBackgroundColor,
    timelineFontColor,
  ])

  useEffect(() => {
    if (!objectives || objectives.length === 0) return

    // TODO: add the ability for content to overflow if the text is too long
    const items: TimelineItem[] = [...iterations, ...objectives]

    var container = document.getElementById('timeline-vis')
    const timeline = new Timeline(container, items, options as TimelineOptions)

    if (enableGroups === true) {
      let teams = []
      if (!teamNames || teamNames.length === 0) {
        teams = objectives.reduce((acc, obj) => {
          if (!acc.includes(obj.group)) {
            acc.push(obj.group)
          }
          return acc
        }, [])
      } else {
        teams = teamNames
      }

      const groups = teams.map((team, index) => {
        return {
          id: team,
          content: team,
          style: `color: ${timelineFontColor};`,
        } as DataGroup
      })

      timeline.setGroups(groups)
    }
  }, [
    enableGroups,
    objectives,
    options,
    piStart,
    piEnd,
    teamNames,
    timelineFontColor,
    iterations,
  ])

  const TimelineChart = () => {
    if (!objectives || objectives.length === 0) {
      return <ModaEmpty message="No objectives" />
    }

    return (
      <>
        {viewSelector && (
          <Space
            style={{
              display: 'flex',
              justifyContent: 'flex-end',
              alignItems: 'center',
              paddingBottom: '16px',
            }}
          >
            {viewSelector}
          </Space>
        )}
        <div id="timeline-vis"></div>
      </>
    )
  }

  // TODO: add a loading indicator
  return <TimelineChart />
}

export default PlanningIntervalObjectivesTimeline
