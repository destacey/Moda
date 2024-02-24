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
import React, { useEffect, useMemo, useRef, useState } from 'react'
import { createRoot } from 'react-dom/client'
import { ModaEmpty } from '../../components/common'
import { UseQueryResult } from 'react-query'
import { DataGroup } from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import Link from 'next/link'
import { Card, Flex, Spin, Typography } from 'antd'
import useTheme from '../../components/contexts/theme'

const { Text } = Typography

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
      <Text style={{ padding: '5px', color: timelineFontColor }}>
        <Link
          href={`/planning/planning-intervals/${objective.planningInterval.key}/objectives/${objective.key}`}
        >
          {objective.key}
        </Link>
        <span> - </span> {objective.name}
      </Text>
    </div>
  )
}

const getDataGroups = (
  teamNames: string[],
  objectives: TimelineItem[],
  timelineFontColor: string,
): DataGroup[] => {
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

  return teams.map((team) => {
    return {
      id: team,
      content: team,
      style: `color: ${timelineFontColor};`,
    } as DataGroup
  })
}

const PlanningIntervalObjectivesTimeline = ({
  objectivesQuery,
  planningIntervalCalendarQuery,
  enableGroups = false,
  teamNames,
  viewSelector,
}: PlanningIntervalObjectivesTimelineProps) => {
  const [isLoading, setIsLoading] = useState(true)
  const [piStart, setPiStart] = useState<Date>(undefined)
  const [piEnd, setPiEnd] = useState<Date>(undefined)
  const [iterations, setIterations] = useState<TimelineItem[]>([])
  const [objectives, setObjectives] = useState<TimelineItem[]>([])
  const timelineRef = useRef<HTMLDivElement>(null)

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
      selectable: true,
      orientation: 'top',
      maxHeight: 650,
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
              <Text>{item.title}</Text>
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

    setIsLoading(false)
  }, [
    objectivesQuery,
    objectivesQuery.data,
    planningIntervalCalendarQuery,
    planningIntervalCalendarQuery.data,
    timelineBackgroundColor,
    timelineFontColor,
  ])

  useEffect(() => {
    if (!objectives || objectives.length === 0 || isLoading) return

    // TODO: add the ability for content to overflow if the text is too long
    const items: TimelineItem[] = [...iterations, ...objectives]

    const timeline = new Timeline(
      timelineRef.current,
      items,
      options as TimelineOptions,
    )

    if (enableGroups === true) {
      timeline.setGroups(
        getDataGroups(teamNames, objectives, timelineFontColor),
      )
    }
  }, [
    enableGroups,
    isLoading,
    iterations,
    objectives,
    options,
    teamNames,
    timelineFontColor,
  ])

  const TimelineChart = () => {
    if (!objectives || objectives.length === 0) {
      return <ModaEmpty message="No objectives" />
    }

    return (
      <>
        {viewSelector && (
          <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
            {viewSelector}
          </Flex>
        )}
        <Card size="small" bordered={false}>
          <div ref={timelineRef} id="timeline-vis"></div>
        </Card>
      </>
    )
  }

  return (
    <Spin spinning={isLoading} tip="Loading timeline..." size="large">
      <TimelineChart />
    </Spin>
  )
}

export default PlanningIntervalObjectivesTimeline
