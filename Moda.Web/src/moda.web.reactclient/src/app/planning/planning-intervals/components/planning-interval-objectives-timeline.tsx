'use client'

import React, { useEffect, useMemo, useState } from 'react'
import { Card, Flex, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { UseQueryResult } from 'react-query'
import {
  DataGroup,
} from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import {
  PlanningIntervalCalendarDto,
  PlanningIntervalObjectiveListDto,
} from '@/src/services/moda-api'
import {
  ModaDataGroup,
  ModaDataItem,
  ModaTimeline,
  ModaTimelineOptions,
  TimelineTemplate
} from '@/src/app/components/common/timeline'

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

interface ObjectiveDataItem
  extends ModaDataItem<PlanningIntervalObjectiveListDto, string> {
  planningIntervalKey?: number
  zIndex?: number
}

export const ObjectiveTimelineTemplate: TimelineTemplate<ObjectiveDataItem> = ({
  item,
  fontColor,
  foregroundColor,
}) => {
  return (
    <div
      style={{
        width: `${item.objectData?.progress}%`,
        backgroundColor: foregroundColor,
      }}
    >
      <Text style={{ padding: '5px', color: fontColor }}>
        <Link
          href={`/planning/planning-intervals/${item.objectData?.planningInterval?.key}/objectives/${item.objectData?.key}`}
        >
          {item.objectData?.key}
        </Link>
        <span> - </span> {item.objectData?.name}
      </Text>
    </div>
  )
}

const getDataGroups = (
  teamNames: string[],
  objectives: ObjectiveDataItem[],
): DataGroup[] => {
  let groups: string[]
  if (!teamNames || teamNames.length === 0) {
    groups = objectives.reduce((acc, item) => {
      if (!item.group)
        return acc

      if (!acc.includes(item.group)) {
        acc.push(item.group)
      }

      return acc
    }, [] as NonNullable<ObjectiveDataItem['group']>[])
  } else {
    groups = teamNames
  }

  return groups.map((group): ModaDataGroup => ({
    id: group,
    content: group
  }))
}

const PlanningIntervalObjectivesTimeline = ({
  objectivesData,
  planningIntervalCalendarQuery,
  enableGroups = false,
  teamNames,
  viewSelector,
}: PlanningIntervalObjectivesTimelineProps) => {
  const [isLoading, setIsLoading] = useState(true)
  const [piStart, setPiStart] = useState<Date | undefined>(undefined)
  const [piEnd, setPiEnd] = useState<Date | undefined>(undefined)
  const [iterations, setIterations] = useState<ObjectiveDataItem[]>([])
  const [objectives, setObjectives] = useState<ObjectiveDataItem[]>([])

  const timelineOptions = useMemo(
    (): ModaTimelineOptions<ObjectiveDataItem> =>
      // TODO: start,end,min,max types don't allow undefined, but initial state is undefined
      ({
        maxHeight: 650,
        start: piStart,
        end: piEnd,
        min: piStart,
        max: piEnd,
        groupOrder: 'content',
      }),
    [piEnd, piStart],
  )

  useEffect(() => {
    if (!objectivesData || !planningIntervalCalendarQuery?.data) return

    setPiStart(planningIntervalCalendarQuery.data.start)
    setPiEnd(planningIntervalCalendarQuery.data.end)

    setIterations(
      planningIntervalCalendarQuery.data.iterationSchedules?.map(
        (i): ObjectiveDataItem => ({
          id: i.key,
          planningIntervalKey: planningIntervalCalendarQuery.data.key,
          title: i.name,
          content: i.name ?? '',
          start: dayjs(i.start).toDate(),
          end: dayjs(i.end).add(1, 'day').subtract(1, 'second').toDate(),
          type: 'background',
        }),
      ) ?? [],
    )

    setObjectives(
      objectivesData
        .filter((obj) => obj.status?.name !== 'Canceled')
        .map((obj): ObjectiveDataItem => ({
          id: obj.key,
          title: `${obj.name} (${obj.status?.name}) - ${obj.progress}%`,
          content: obj.name ?? '',
          start: dayjs(
            obj.startDate ?? planningIntervalCalendarQuery.data.start
          ).toDate(),
          end: dayjs(
            obj.targetDate ?? planningIntervalCalendarQuery.data.end
          ).toDate(),
          group: obj.team?.name,
          type: 'range',
          zIndex: 1,
          objectData: obj
        })),
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
            enableGroups
              ? getDataGroups(teamNames ?? [], objectives)
              : undefined
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
