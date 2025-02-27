'use client'

import { ReactNode, useEffect, useMemo, useState } from 'react'
import { Button, Card, Divider, Flex, Space, Switch, Typography } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import {
  RoadmapActivityListDto,
  RoadmapDetailsDto,
  RoadmapItemListDto,
  RoadmapMilestoneListDto,
  RoadmapTimeboxListDto,
} from '@/src/services/moda-api'
import { ControlItemsMenu } from '@/src/components/common/control-items-menu'
import {
  ModaDataGroup,
  ModaTimeline,
  ModaTimelineOptions,
} from '@/src/components/common/timeline'
import {
  ModaDataItem,
  TimelineTemplate,
} from '@/src/components/common/timeline/types'
import dayjs from 'dayjs'
import { MinusSquareOutlined, PlusSquareOutlined } from '@ant-design/icons'
import { getLuminance } from '@/src/utils/color-helper'

const { Text } = Typography

export interface RoadmapsTimelineProps {
  roadmap: RoadmapDetailsDto
  roadmapItems: RoadmapItemListDto[]
  isRoadmapItemsLoading: boolean
  refreshRoadmapItems: () => void
  viewSelector?: ReactNode
  openRoadmapItemDrawer: (itemId: string) => void
}

interface RoadmapTimelineItem extends ModaDataItem<RoadmapItemListDto, string> {
  id: string
  end: Date
  order?: number
  treeLevel: number
  openRoadmapItemDrawer: (itemId: string) => void
}

enum RoadmapItemType {
  Roadmap = 'roadmap',
  Activity = 'activity',
  Milestone = 'milestone',
  Timebox = 'timebox',
}

interface ProcessedRoadmapData {
  items: RoadmapTimelineItem[]
  maxLevel: number
}

function flattenRoadmapItems(
  items: RoadmapItemListDto[],
  openRoadmapItemDrawer: (itemId: string) => void,
  treeLevel: number = 1,
): RoadmapTimelineItem[] {
  return items.reduce<RoadmapTimelineItem[]>((acc, item) => {
    const baseTimelineItem: Partial<RoadmapTimelineItem> = {
      id: item.id,
      title: item.name,
      content: item.name,
      itemColor: item.color,
      group: item.parent?.id,
      treeLevel: treeLevel,
      objectData: item,
      openRoadmapItemDrawer: openRoadmapItemDrawer,
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
          acc.push(
            ...flattenRoadmapItems(
              activityItem.children,
              openRoadmapItemDrawer,
              treeLevel + 1,
            ),
          )
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

      case RoadmapItemType.Roadmap: {
        const activityItem = item as RoadmapActivityListDto
        const timelineItem: RoadmapTimelineItem = {
          ...baseTimelineItem,
          treeLevel: 0,
          type: 'range',
          start: dayjs(activityItem.start).toDate(),
          end: dayjs(activityItem.end).toDate(),
          order: 1,
        } as RoadmapTimelineItem

        acc.push(timelineItem)

        if (activityItem.children?.length > 0) {
          acc.push(
            ...flattenRoadmapItems(
              activityItem.children,
              openRoadmapItemDrawer,
              1,
            ),
          )
        }
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
  const groupItems = items.filter((item) => item.treeLevel < currentLevel)

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
      treeLevel: item.treeLevel,
      objectData: item.objectData,
    }),
  )

  return groups
}

export const DrillThroughControl = (props: {
  currentLevel: number
  maxLevel: number
  onLevelChange: (treeLevel: number) => void
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

export const RoadmapRangeItemTemplate: TimelineTemplate<
  RoadmapTimelineItem
> = ({ item, fontColor, foregroundColor }) => {
  const adjustedfontColor =
    getLuminance(item.itemColor ?? '') > 0.6 ? '#4d4d4d' : '#FFFFFF'

  return (
    <Text style={{ padding: '5px', color: adjustedfontColor }}>
      <a
        onClick={() => item.openRoadmapItemDrawer(item.id)}
        style={{ color: adjustedfontColor, textDecoration: 'none' }}
        onMouseOver={(e) =>
          (e.currentTarget.style.textDecoration = 'underline')
        }
        onMouseOut={(e) => (e.currentTarget.style.textDecoration = 'none')}
      >
        {item.content}
      </a>
    </Text>
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
    if (!props.roadmap || props.isRoadmapItemsLoading || !props.roadmapItems) {
      return null
    }

    // TODO: this is a hack to get the roadmap itself as an item
    const roadmapAsItem: RoadmapActivityListDto = {
      $type: RoadmapItemType.Roadmap,
      id: props.roadmap.id,
      name: props.roadmap.name,
      type: {
        id: 1,
        name: 'Roadmap',
      },
      start: props.roadmap.start,
      end: props.roadmap.end,
      children: props.roadmapItems.map((item) => ({
        ...item,
        parent: props.roadmap,
      })),
    }

    const items = flattenRoadmapItems(
      [roadmapAsItem],
      props.openRoadmapItemDrawer,
    )

    const maxLevel = items.reduce(
      (max, item) => Math.max(max, item.treeLevel),
      0,
    )

    return { items, maxLevel }
  }, [
    props.isRoadmapItemsLoading,
    props.openRoadmapItemDrawer,
    props.roadmap,
    props.roadmapItems,
  ])

  const processedGroups = useMemo(() => {
    if (!processedData || currentLevel <= 1) return undefined

    const potentialGroups = processedData.items.filter(
      (item) =>
        item.objectData.$type === RoadmapItemType.Activity ||
        item.objectData.$type === RoadmapItemType.Roadmap,
    )

    return createNestedGroups(potentialGroups, currentLevel)
  }, [processedData, currentLevel])

  const filteredItems = useMemo(() => {
    return (
      processedData?.items.filter(
        (item) =>
          item.treeLevel === currentLevel ||
          (item.treeLevel < currentLevel &&
            item.objectData.$type !== RoadmapItemType.Roadmap &&
            item.objectData.$type !== RoadmapItemType.Activity),
      ) ?? []
    )
  }, [processedData?.items, currentLevel])

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

  const onLevelChange = (treeLevel: number) => {
    setCurrentLevel(treeLevel)
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
      <Card size="small" variant="borderless">
        <ModaTimeline
          data={filteredItems}
          groups={processedGroups}
          isLoading={isLoading}
          options={timelineOptions}
          rangeItemTemplate={RoadmapRangeItemTemplate}
          allowFullScreen={true}
          allowSaveAsImage={true}
        />
      </Card>
    </>
  )
}

export default RoadmapsTimeline
