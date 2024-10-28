'use client'

import { ReactNode, useEffect, useMemo, useState } from 'react'
import { Button, Card, Divider, Flex, Space, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
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

interface ProcessedRoadmapData {
  items: RoadmapTimelineItem[]
  maxLevel: number
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

        if (activityItem.children?.length > 0) {
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
    }

    return acc
  }, [])
}

function createNestedGroups(
  items: RoadmapTimelineItem[],
  currentLevel: number,
): ModaDataGroup<RoadmapItemListDto>[] {
  // Get all items up to but not including current level
  const groupItems = items.filter((item) => item.level < currentLevel)

  // Create a map to store parent-child relationships
  const parentChildMap = new Map<string, string[]>()

  // Build the parent-child relationships
  groupItems.forEach((item) => {
    if (item.group) {
      const parentChildren = parentChildMap.get(item.group) || []
      parentChildren.push(item.id)
      parentChildMap.set(item.group, parentChildren)
    }
  })

  // Create the nested groups structure - include all items
  const groups: ModaDataGroup<RoadmapItemListDto>[] = groupItems.map(
    (item) => ({
      id: item.id,
      content: item.objectData?.name || '',
      nestedGroups: parentChildMap.get(item.id) || undefined,
    }),
  )

  return groups
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
  const [hasUserChangedLevel, setHasUserChangedLevel] = useState(false)

  const [showCurrentTime, setShowCurrentTime] = useState<boolean>(true)

  const processedData: ProcessedRoadmapData = useMemo(() => {
    if (props.isRoadmapItemsLoading || !props.roadmapItems) {
      return null
    }

    const items = flattenRoadmapItems(props.roadmapItems)
    const maxLevel = items.reduce((max, item) => Math.max(max, item.level), 0)

    return { items, maxLevel }
  }, [props.isRoadmapItemsLoading, props.roadmapItems])

  const processedGroups = useMemo(() => {
    if (!processedData || currentLevel <= 1) return undefined
    return createNestedGroups(processedData.items, currentLevel)
  }, [processedData, currentLevel])

  useEffect(() => {
    if (processedData && processedData.maxLevel > 1 && !hasUserChangedLevel) {
      setCurrentLevel(2) // TODO: make this configurable
    }
  }, [processedData, hasUserChangedLevel])

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
          maxLevel={processedData?.maxLevel ?? 0}
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
          data={
            processedData?.items.filter(
              (item) => item.level === currentLevel,
            ) ?? []
          }
          //groups={processedGroups}  // TODO: need to work on the nested groups styling
          groups={
            processedData?.maxLevel > 0
              ? (processedData?.items.filter(
                  (item) => item.level === currentLevel - 1,
                ) ?? [])
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
