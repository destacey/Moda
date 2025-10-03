'use client'

import React, { useMemo } from 'react'
import { Card, Flex, Typography } from 'antd'
import dayjs from 'dayjs'
import Link from 'next/link'
import { DataGroup } from 'vis-timeline/standalone'
import {
  PlanningIntervalCalendarDto,
  PlanningIntervalObjectiveListDto,
} from '@/src/services/moda-api'
import {
  ModaDataGroup,
  ModaDataItem,
  ModaTimeline,
  ModaTimelineOptions,
  TimelineTemplate,
} from '@/src/components/common/timeline'

const { Text } = Typography

interface PlanningIntervalObjectivesTimelineProps {
  objectivesData: PlanningIntervalObjectiveListDto[]
  planningIntervalCalendar: PlanningIntervalCalendarDto
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
    groups = objectives.reduce(
      (acc, item) => {
        if (!item.group) return acc

        if (!acc.includes(item.group)) {
          acc.push(item.group)
        }

        return acc
      },
      [] as NonNullable<ObjectiveDataItem['group']>[],
    )
  } else {
    groups = teamNames
  }

  return groups.map(
    (group): ModaDataGroup => ({
      id: group,
      content: group,
    }),
  )
}

const PlanningIntervalObjectivesTimeline = ({
  objectivesData,
  planningIntervalCalendar: planningIntervalCalendar,
  enableGroups = false,
  teamNames,
  viewSelector,
}: PlanningIntervalObjectivesTimelineProps) => {
  const timelineOptions = useMemo(
    (): ModaTimelineOptions<ObjectiveDataItem> => ({
      maxHeight: 650,
      start: planningIntervalCalendar?.start ?? dayjs().toDate(),
      end: planningIntervalCalendar?.end ?? dayjs().toDate(),
      min: planningIntervalCalendar?.start ?? dayjs().toDate(),
      max: planningIntervalCalendar?.end ?? dayjs().toDate(),
      groupOrder: 'content',
    }),
    [planningIntervalCalendar],
  )

  const iterations = useMemo((): ObjectiveDataItem[] => {
    return (
      planningIntervalCalendar?.iterationSchedules?.map(
        (i): ObjectiveDataItem => ({
          id: String(i.key),
          planningIntervalKey: planningIntervalCalendar?.key,
          title: i.name,
          content: i.name ?? '',
          start: dayjs(i.start).toDate(),
          end: dayjs(i.end).add(1, 'day').subtract(1, 'second').toDate(),
          type: 'background',
        }),
      ) ?? []
    )
  }, [planningIntervalCalendar])

  const objectives = useMemo((): ObjectiveDataItem[] => {
    return (
      (objectivesData ?? [])
        .filter((obj) => obj.status?.name !== 'Canceled')
        .map(
          (obj): ObjectiveDataItem => ({
            id: String(obj.key),
            title: `${obj.name} (${obj.status?.name}) - ${obj.progress}%`,
            content: obj.name ?? '',
            start: dayjs(
              obj.startDate ?? planningIntervalCalendar?.start,
            ).toDate(),
            end: dayjs(
              obj.targetDate ?? planningIntervalCalendar?.end,
            ).toDate(),
            group: obj.team?.name,
            type: 'range',
            zIndex: 1,
            objectData: obj,
          }),
        ) ?? []
    )
  }, [objectivesData, planningIntervalCalendar])

  const combinedItems = useMemo(
    () => [...iterations, ...objectives],
    [iterations, objectives],
  )
  const derivedIsLoading = !(planningIntervalCalendar && objectivesData)

  // Hooks must run unconditionally; perform an early return after hooks
  if (!planningIntervalCalendar) return null

  return (
    <>
      {viewSelector && (
        <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
          {viewSelector}
        </Flex>
      )}
      <Card size="small" variant="borderless">
        <ModaTimeline
          data={combinedItems}
          groups={
            enableGroups
              ? getDataGroups(teamNames ?? [], objectives)
              : undefined
          }
          isLoading={derivedIsLoading}
          options={timelineOptions}
          rangeItemTemplate={ObjectiveTimelineTemplate}
        />
      </Card>
    </>
  )
}

export default PlanningIntervalObjectivesTimeline
