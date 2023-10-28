'use client'

import {
  DataItem,
  Timeline,
  TimelineOptions,
} from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import 'vis-timeline/styles/vis-timeline-graph2d.css'
import './program-increment-objectives-timeline.css'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveListDto,
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

interface ProgramIncrementObjectivesTimelineProps {
  objectivesQuery: UseQueryResult<ProgramIncrementObjectiveListDto[], unknown>
  programIncrement: ProgramIncrementDetailsDto
  enableGroups?: boolean
  teamNames?: string[]
  viewSelector?: React.ReactNode
}

interface TimelineItem extends DataItem {
  id: number
  programIncrementKey: number
  title?: string
  content: string
  start: Date
  end: Date
  group?: string
  type?: string
  objective?: ProgramIncrementObjectiveListDto
}

interface ObjectiveTimelineTemplateProps {
  objective: ProgramIncrementObjectiveListDto
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
          href={`/planning/program-increments/${objective.programIncrement.key}/objectives/${objective.key}`}
        >
          {objective.key}
        </Link>
        <span> - </span> {objective.name}
      </Typography.Text>
    </div>
  )
}

const ProgramIncrementObjectivesTimeline = ({
  objectivesQuery,
  programIncrement,
  enableGroups = false,
  teamNames,
  viewSelector,
}: ProgramIncrementObjectivesTimelineProps) => {
  const [objectives, setObjectives] = useState<TimelineItem[]>([])
  const { currentThemeName } = useTheme()
  const timelineBackgroundColor =
    currentThemeName === 'light' ? '#f5f5f5' : '#000000'
  const timelineForegroundColor =
    currentThemeName === 'light' ? '#c7edff' : '#13283a'
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
      start: dayjs(programIncrement.start).subtract(1, 'week').toDate(),
      end: dayjs(programIncrement.end).add(1, 'week').toDate(),
      min: dayjs(programIncrement.start).subtract(4, 'week').toDate(),
      max: dayjs(programIncrement.end).add(4, 'week').toDate(),
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
        }
      },
    }
  }, [
    programIncrement.end,
    programIncrement.start,
    timelineFontColor,
    timelineForegroundColor,
  ])

  useEffect(() => {
    if (!objectivesQuery?.data) return

    setObjectives(
      objectivesQuery?.data
        .filter((obj) => obj.status?.name !== 'Canceled')
        .map((obj, index) => {
          return {
            id: obj.key,
            programIncrementKey: obj.programIncrement.key,
            title: `${obj.name} (${obj.status?.name}) - ${obj.progress}%`,
            content: '',
            start: dayjs(obj.startDate ?? programIncrement.start).toDate(),
            end: dayjs(obj.targetDate ?? programIncrement.end).toDate(),
            group: obj.team?.name,
            type: 'range',
            style: `background: ${timelineBackgroundColor}; border-color: ${timelineBackgroundColor};`,
            zIndex: 1,
            objective: obj,
          }
        }),
    )
  }, [
    objectivesQuery?.data,
    programIncrement.end,
    programIncrement.key,
    programIncrement.start,
    timelineBackgroundColor,
    timelineFontColor,
  ])

  useEffect(() => {
    if (!objectives || objectives.length === 0) return

    // TODO: add the ability for content to overflow if the text is too long
    const items: TimelineItem[] = [
      {
        id: -1,
        programIncrementKey: -1,
        title: 'PI Start',
        content: '',
        start: dayjs(programIncrement.start).toDate(),
        end: dayjs(programIncrement.start).add(1, 'day').toDate(),
        type: 'background',
      },
      {
        id: -2,
        programIncrementKey: -2,
        title: 'PI End',
        content: '',
        start: dayjs(programIncrement.end).toDate(),
        end: dayjs(programIncrement.end).add(1, 'day').toDate(),
        type: 'background',
      },
      ...objectives,
    ]

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

      const groups = teams.map((team) => {
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
    programIncrement.end,
    programIncrement.start,
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

export default ProgramIncrementObjectivesTimeline
