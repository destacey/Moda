'use client'

import {
  DataItem,
  Timeline,
  TimelineOptions,
} from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import 'vis-timeline/styles/vis-timeline-graph2d.css'
import './program-increment-objectives-timeline.css'
import { renderToString } from 'react-dom/server'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementObjectiveListDto,
} from '@/src/services/moda-api'
import dayjs from 'dayjs'
import React, { useCallback, useEffect, useMemo, useState } from 'react'
import { ModaEmpty } from '../../components/common'
import { UseQueryResult } from 'react-query'
import { DataGroup } from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import Link from 'next/link'
import { Typography } from 'antd'
import useTheme from '../../components/contexts/theme'

interface ProgramIncrementObjectivesTimelineProps {
  objectivesQuery: UseQueryResult<ProgramIncrementObjectiveListDto[], unknown>
  programIncrement: ProgramIncrementDetailsDto
  enableGroups?: boolean
  teamNames?: string[]
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
}

const ProgramIncrementObjectivesTimeline = ({
  objectivesQuery,
  programIncrement,
  enableGroups = false,
  teamNames,
}: ProgramIncrementObjectivesTimelineProps) => {
  const [objectives, setObjectives] = useState<TimelineItem[]>([])
  const { currentThemeName } = useTheme()
  const timelineBackgroundColor =
    currentThemeName === 'light' ? '#C7EDFF' : '#13283A'
  const timelineForegroundColor =
    currentThemeName === 'light' ? '#1272CC' : '#1F83D2'
  const timelineFontColor = currentThemeName === 'light' ? '#000000' : '#FFFFFF'

  console.log('background color', timelineBackgroundColor)
  console.log('foreground color', timelineForegroundColor)
  console.log('font color', timelineFontColor)

  // TODO: setup the template function to render the content
  // TODO: add the ability to export/save as svg or png
  // TODO: update the styles to match the rest of the app.  Especially for dark mode.
  const options: TimelineOptions = useMemo(() => {
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
      xss: { disabled: true },
    }
  }, [programIncrement.end, programIncrement.start])

  const ObjectiveContent = useCallback(
    (objective: ProgramIncrementObjectiveListDto) => {
      return (
        <div
          style={{
            width: `${objective.progress}%`,
            backgroundColor: `${timelineForegroundColor}`,
          }}
        >
          <Typography.Text style={{ padding: '5px' }}>
            <Link
              style={{
                color: `${timelineFontColor}`,
              }}
              href={`/planning/program-increments/${objective.programIncrement.key}/objectives/${objective.key}`}
            >
              {objective.key}
            </Link>
            <span> - </span> {objective.name}
          </Typography.Text>
        </div>
      )
    },
    [timelineFontColor, timelineForegroundColor],
  )

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
            content: renderToString(ObjectiveContent(obj)),
            start: dayjs(obj.startDate ?? programIncrement.start).toDate(),
            end: dayjs(obj.targetDate ?? programIncrement.end).toDate(),
            group: obj.team?.name,
            type: 'range',
            style: `background: ${timelineBackgroundColor}; border-color: ${timelineBackgroundColor}; color: ${timelineFontColor};`,
            zIndex: 1,
          }
        }),
    )
  }, [
    ObjectiveContent,
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
    const timeline = new Timeline(container, items, options)

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
        <div id="timeline-vis"></div>
      </>
    )
  }

  // TODO: add a loading indicator
  return <TimelineChart />
}

export default ProgramIncrementObjectivesTimeline
