'use client'

import {
  DataGroup,
  DataItem,
} from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import {
  PlanningIntervalCalendarDto,
  PlanningIntervalObjectiveListDto,
} from '@/src/services/moda-api'
import dayjs from 'dayjs'
import React, { useEffect, useMemo, useState } from 'react'
import { UseQueryResult } from 'react-query'
import Link from 'next/link'
import { Card, Flex, Typography } from 'antd'
import {
  ModaDataItem,
  ModaTimeline,
  ModaTimelineOptions,
  RangeItemTemplateProps,
} from '../../components/common/timeline'

const { Text } = Typography

interface PlanningIntervalObjectivesTimelineProps {
  objectivesData: PlanningIntervalObjectiveListDto[]
  planningIntervalCalendarQuery: UseQueryResult<
    PlanningIntervalCalendarDto,
    unknown
  >
  enableGroups?: boolean
  teamNames?: string[]
  viewSelector?: React.ReactNode
}

export const ObjectiveTimelineTemplate = ({
  item,
  fontColor,
  foregroundColor,
}: RangeItemTemplateProps) => {
  return (
    <div
      style={{
        width: `${item.objectData.progress}%`,
        backgroundColor: foregroundColor,
      }}
    >
      <Text style={{ padding: '5px', color: fontColor }}>
        <Link
          href={`/planning/planning-intervals/${item.objectData.planningInterval.key}/objectives/${item.objectData.key}`}
        >
          {item.objectData.key}
        </Link>
        <span> - </span> {item.objectData.name}
      </Text>
    </div>
  )
}

const getDataGroups = (
  teamNames: string[],
  objectives: ModaDataItem<PlanningIntervalObjectiveListDto>[],
): DataGroup[] => {
  let groups = []
  if (!teamNames || teamNames.length === 0) {
    groups = objectives.reduce((acc, item) => {
      if (!acc.includes(item.objectData.group)) {
        acc.push(item.objectData.group)
      }
      return acc
    }, [])
  } else {
    groups = teamNames
  }

  return groups.map((group) => {
    return {
      id: group,
      content: group,
    } as DataGroup
  })
}

const PlanningIntervalObjectivesTimeline = ({
  objectivesData,
  planningIntervalCalendarQuery,
  enableGroups = false,
  teamNames,
  viewSelector,
}: PlanningIntervalObjectivesTimelineProps) => {
  const [isLoading, setIsLoading] = useState(true)
  const [piStart, setPiStart] = useState<Date>(undefined)
  const [piEnd, setPiEnd] = useState<Date>(undefined)
  const [iterations, setIterations] = useState<DataItem[]>([])
  const [objectives, setObjectives] = useState<
    ModaDataItem<PlanningIntervalObjectiveListDto>[]
  >([])

  const timelineOptions = useMemo((): ModaTimelineOptions => {
    return {
      maxHeight: 650,
      start: piStart,
      end: piEnd,
      min: piStart,
      max: piEnd,
      groupOrder: 'content',
    }
  }, [piEnd, piStart])

  useEffect(() => {
    if (!objectivesData || !planningIntervalCalendarQuery?.data) return

    setPiStart(planningIntervalCalendarQuery.data.start)
    setPiEnd(planningIntervalCalendarQuery.data.end)

    setIterations(
      planningIntervalCalendarQuery.data.iterationSchedules.map((i) => {
        return {
          id: i.key,
          planningIntervalKey: planningIntervalCalendarQuery.data.key,
          title: i.name,
          content: i.name,
          start: dayjs(i.start).toDate(),
          end: dayjs(i.end).add(1, 'day').subtract(1, 'second').toDate(),
          type: 'background',
        } as DataItem
      }),
    )

    setObjectives(
      objectivesData
        .filter((obj) => obj.status?.name !== 'Canceled')
        .map((obj) => {
          return {
            id: obj.key,
            title: `${obj.name} (${obj.status?.name}) - ${obj.progress}%`,
            content: obj.name,
            start: dayjs(
              obj.startDate ?? planningIntervalCalendarQuery.data.start,
            ).toDate(),
            end: dayjs(
              obj.targetDate ?? planningIntervalCalendarQuery.data.end,
            ).toDate(),
            group: obj.team?.name,
            type: 'range',
            zIndex: 1,
            objectData: obj,
          } as ModaDataItem<PlanningIntervalObjectiveListDto>
        }),
    )

    setIsLoading(false)
  }, [
    objectivesData,
    planningIntervalCalendarQuery,
    planningIntervalCalendarQuery.data,
  ])

  return (
    <>
      {viewSelector && (
        <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
          {viewSelector}
        </Flex>
      )}
      <Card size="small" bordered={false}>
        <ModaTimeline
          data={[...iterations, ...objectives]}
          groups={
            enableGroups ? getDataGroups(teamNames, objectives) : undefined
          }
          isLoading={isLoading}
          options={timelineOptions}
          rangeItemTemplate={ObjectiveTimelineTemplate}
        />
      </Card>
    </>
  )
}

export default PlanningIntervalObjectivesTimeline
