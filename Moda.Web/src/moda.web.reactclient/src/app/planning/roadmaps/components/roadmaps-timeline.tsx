'use client'

import { ReactNode, useEffect, useMemo, useState } from 'react'
import { Button, Card, Divider, Flex, Space, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { DataGroup } from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import {
  RoadmapActivityListDto,
  RoadmapDetailsDto,
  RoadmapItemListDto,
  RoadmapMilestoneListDto,
  RoadmapTimeboxListDto,
} from '@/src/services/moda-api'
import { ControlItemsMenu } from '@/src/app/components/common/control-items-menu'
import {
  ModaDataGroup,
  ModaTimeline,
  ModaTimelineOptions,
} from '@/src/app/components/common/timeline'
import { ModaDataItem } from '@/src/app/components/common/timeline/types'
import dayjs from 'dayjs'
import { MinusSquareOutlined, PlusSquareOutlined } from '@ant-design/icons'

export interface RoadmapsTimelineProps {
  roadmap: RoadmapDetailsDto
  roadmapItems: RoadmapItemListDto[]
  isRoadmapItemsLoading: boolean
  refreshRoadmapItems: () => void
  viewSelector?: ReactNode | undefined
}

interface RoadmapTimelineItem extends ModaDataItem<RoadmapItemListDto, string> {
  id: string
  end: Date
  order?: number
  level: number
}

enum RoadmapItemType {
  Activity = 'RoadmapActivityListDto',
  Milestone = 'RoadmapMilestoneListDto',
  Timebox = 'RoadmapTimeboxListDto',
}

function flattenRoadmapItems(
  items: RoadmapItemListDto[],
  level: number = 1,
): RoadmapTimelineItem[] {
  return items.reduce<RoadmapTimelineItem[]>((acc, item) => {
    const baseTimelineItem: Partial<RoadmapTimelineItem> = {
      id: item.id,
      title: item.name,
      content: item.name,
      itemColor: item.color,
      group: item.parent?.id,
      level,
      objectData: item,
    }

    switch (item.$type) {
      case RoadmapItemType.Activity: {
        const activityItem = item as RoadmapActivityListDto
        const timelineItem: RoadmapTimelineItem = {
          ...baseTimelineItem,
          type: 'range',
          start: dayjs(activityItem.start).toDate(),
          end: dayjs(activityItem.end).toDate(),
          order: activityItem.order,
        } as RoadmapTimelineItem

        acc.push(timelineItem)

        // Recursively process children if they exist
        if (activityItem.children && activityItem.children.length > 0) {
          acc.push(...flattenRoadmapItems(activityItem.children, level + 1))
        }
        break
      }

      case RoadmapItemType.Milestone: {
        const milestoneItem = item as RoadmapMilestoneListDto
        acc.push({
          ...baseTimelineItem,
          type: 'point',
          start: dayjs(milestoneItem.date).toDate(),
        } as RoadmapTimelineItem)
        break
      }

      case RoadmapItemType.Timebox: {
        const timeboxItem = item as RoadmapTimeboxListDto
        acc.push({
          ...baseTimelineItem,
          type: 'background',
          start: dayjs(timeboxItem.start).toDate(),
          end: dayjs(timeboxItem.end).toDate(),
        } as RoadmapTimelineItem)
        break
      }

      default: {
        console.warn(`Unknown roadmap item type: ${item.$type}`)
      }
    }

    return acc
  }, [])
}

function getDataGroups(
  groupItems: RoadmapTimelineItem[],
  roadmaps: RoadmapTimelineItem[],
): DataGroup[] {
  let groups: Array<string | RoadmapTimelineItem>
  if (!groupItems || groupItems.length === 0) {
    groups = roadmaps.reduce(
      (acc, roadmap) => {
        if (!roadmap.group) return acc

        if (!acc.includes(roadmap.group)) {
          acc.push(roadmap.group)
        }
        return acc
      },
      [] as NonNullable<RoadmapTimelineItem['group']>[],
    )
  } else {
    groups = groupItems
  }

  return groups.map(
    (group): ModaDataGroup =>
      typeof group === 'string'
        ? {
            id: group ?? '',
            content: group ?? '',
          }
        : {
            id: group.objectData?.id ?? '',
            content: group.objectData?.name ?? '',
          },
  )
}

export const DrillThroughControl = (props: {
  currentLevel: number
  maxLevel: number
  onLevelChange: (level: number) => void
}) => {
  const onLevelDown = () => {
    props.onLevelChange(props.currentLevel - 1)
  }

  const onLevelUp = () => {
    props.onLevelChange(props.currentLevel + 1)
  }

  return (
    <div>
      <Button
        type="text"
        shape="circle"
        icon={<MinusSquareOutlined />}
        disabled={props.currentLevel === 1}
        onClick={onLevelDown}
      />
      <Button
        type="text"
        shape="circle"
        icon={<PlusSquareOutlined />}
        disabled={props.currentLevel === props.maxLevel || props.maxLevel === 0}
        onClick={onLevelUp}
      />
    </div>
  )
}

const RoadmapsTimeline = (props: RoadmapsTimelineProps) => {
  const [isLoading, setIsLoading] = useState(true)
  const [timelineStart, setTimelineStart] = useState<Date | undefined>(
    undefined,
  )
  const [timelineEnd, setTimelineEnd] = useState<Date | undefined>(undefined)
  const [currentLevel, setCurrentLevel] = useState<number | undefined>(1)
  const [maxLevel, setMaxLevel] = useState<number>(0)
  const [hasUserChangedLevel, setHasUserChangedLevel] = useState(false)

  const [showCurrentTime, setShowCurrentTime] = useState<boolean>(true)

  const roadmapItems = useMemo(() => {
    if (props.isRoadmapItemsLoading || !props.roadmapItems) return []

    const items = flattenRoadmapItems(props.roadmapItems)

    // get max level
    const itemsMaxLevel = items.reduce((acc, item) => {
      return Math.max(acc, item.level)
    }, 0)
    if (!hasUserChangedLevel && itemsMaxLevel > 1) setCurrentLevel(2) // TODO: make this configurable
    setMaxLevel(itemsMaxLevel)

    return items
  }, [hasUserChangedLevel, props.isRoadmapItemsLoading, props.roadmapItems])

  useEffect(() => {
    if (props.isRoadmapItemsLoading) return

    setTimelineStart(props.roadmap.start)
    setTimelineEnd(props.roadmap.end)

    setIsLoading(props.isRoadmapItemsLoading)
  }, [props])

  const timelineOptions = useMemo(
    (): ModaTimelineOptions<RoadmapTimelineItem> =>
      // TODO: start,end,min,max types don't allow undefined, but initial state is undefined
      ({
        showCurrentTime: showCurrentTime,
        maxHeight: 650,
        start: timelineStart,
        end: timelineEnd,
        min: timelineStart,
        max: timelineEnd,
      }),
    [showCurrentTime, timelineEnd, timelineStart],
  )

  const onShowCurrentTimeChange = (checked: boolean) => {
    setShowCurrentTime(checked)
  }

  const controlItems = (): ItemType[] => {
    const items: ItemType[] = []

    items.push({
      label: (
        <Space>
          <Switch
            size="small"
            checked={showCurrentTime}
            onChange={onShowCurrentTimeChange}
          />
          Show Current Time
        </Space>
      ),
      key: 'show-current-time',
      onClick: () => setShowCurrentTime(!showCurrentTime),
    })

    return items
  }

  const onLevelChange = (level: number) => {
    setCurrentLevel(level)
    setHasUserChangedLevel(true)
  }

  return (
    <>
      <Flex
        justify="space-between"
        align="center"
        style={{ paddingTop: '8px', paddingBottom: '4px' }}
      >
        <DrillThroughControl
          currentLevel={currentLevel}
          maxLevel={maxLevel}
          onLevelChange={onLevelChange}
        />
        <Flex justify="end" align="center">
          <ControlItemsMenu items={controlItems()} />
          <Divider type="vertical" style={{ height: '30px' }} />
          {props.viewSelector}
        </Flex>
      </Flex>
      <Card size="small" bordered={false}>
        <ModaTimeline
          data={roadmapItems.filter((item) => item.level === currentLevel)}
          groups={
            maxLevel > 0
              ? getDataGroups(
                  roadmapItems.filter(
                    (item) => item.level === currentLevel - 1,
                  ),
                  roadmapItems.filter((item) => item.level === currentLevel),
                )
              : undefined
          }
          isLoading={isLoading}
          options={timelineOptions}
        />
      </Card>
    </>
  )
}

export default RoadmapsTimeline
