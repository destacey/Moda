'use client'

import { ReactNode, useEffect, useMemo, useState } from 'react'
import { Card, Divider, Flex, Space, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import dayjs from 'dayjs'
import { DataGroup } from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import { RoadmapChildrenDto, RoadmapDetailsDto } from '@/src/services/moda-api'
import { useGetRoadmapChildrenQuery } from '@/src/store/features/planning/roadmaps-api'
import { ControlItemsMenu } from '@/src/app/components/common/control-items-menu'
import {
  ModaDataGroup,
  ModaTimeline,
  ModaTimelineOptions,
} from '@/src/app/components/common/timeline'
import { ModaDataItem } from '@/src/app/components/common/timeline/types'

export interface RoadmapsTimelineProps {
  roadmap: RoadmapDetailsDto
  roadmapChildren: RoadmapChildrenDto[]
  isChildrenLoading: boolean
  refreshChildren: () => void
  viewSelector?: ReactNode | undefined
}

interface RoadmapTimelineItem extends ModaDataItem<RoadmapChildrenDto, string> {
  id: number
  end: Date
  order?: number
}

const getDataGroups = (
  groupItems: RoadmapTimelineItem[],
  roadmaps: RoadmapTimelineItem[],
): DataGroup[] => {
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

const RoadmapsTimeline = (props: RoadmapsTimelineProps) => {
  const [isLoading, setIsLoading] = useState(true)
  const [timelineStart, setTimelineStart] = useState<Date | undefined>(
    undefined,
  )
  const [timelineEnd, setTimelineEnd] = useState<Date | undefined>(undefined)
  const [levelOneRoadmaps, setLevelOneRoadmaps] = useState<
    RoadmapTimelineItem[]
  >([])
  const [levelTwoRoadmaps, setLevelTwoRoadmaps] = useState<
    RoadmapTimelineItem[]
  >([])

  const [drillDown, setDrillDown] = useState<boolean>(false)
  const [showCurrentTime, setShowCurrentTime] = useState<boolean>(true)

  const { data: roadmapLinksData, isLoading: isLoadingRoadmapLinks } =
    useGetRoadmapChildrenQuery(
      props.roadmapChildren?.map((r) => r.id ?? '') ?? [],
      {
        skip: !props.roadmapChildren || props.roadmapChildren.length === 0,
      },
    )

  useEffect(() => {
    if (!props.roadmapChildren) return

    setTimelineStart(props.roadmap.start)
    setTimelineEnd(props.roadmap.end)

    const levelOneRoadmaps = props.roadmapChildren.map(
      (roadmap): RoadmapTimelineItem => {
        return {
          id: roadmap.key ?? 0,
          title: `${roadmap.key} - ${roadmap.name}`,
          content: roadmap.name ?? '',
          start: dayjs(roadmap.start).toDate(),
          end: dayjs(roadmap.end).toDate(),
          itemColor: roadmap.color,
          group: undefined,
          type: 'range',
          order: roadmap.order,
          objectData: roadmap,
        }
      },
    )
    setLevelOneRoadmaps(levelOneRoadmaps)

    const levelTwoRoadmaps =
      roadmapLinksData?.map((roadmap): RoadmapTimelineItem => {
        return {
          id: roadmap.key ?? 0,
          title: `${roadmap.key} - ${roadmap.name}`,
          content: roadmap.name ?? '',
          start: dayjs(roadmap.start).toDate(),
          end: dayjs(roadmap.end).toDate(),
          itemColor: roadmap.color,
          group: roadmap.parent?.id,
          type: 'range',
          order: roadmap.order,
          objectData: roadmap,
        }
      }) ?? []
    setLevelTwoRoadmaps(levelTwoRoadmaps)

    setIsLoading(props.isChildrenLoading || isLoadingRoadmapLinks)
  }, [drillDown, isLoadingRoadmapLinks, props, roadmapLinksData])

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

  const onDrillDownChange = (checked: boolean) => {
    setDrillDown(checked)
  }

  const onShowCurrentTimeChange = (checked: boolean) => {
    setShowCurrentTime(checked)
  }

  const controlItems = (): ItemType[] => {
    const items: ItemType[] = []

    if (levelTwoRoadmaps && levelTwoRoadmaps.length > 0) {
      items.push({
        label: (
          <Space>
            <Switch
              size="small"
              checked={drillDown}
              onChange={onDrillDownChange}
            />
            Drill Down
          </Space>
        ),
        key: 'drill-down',
        onClick: () => onDrillDownChange(!drillDown),
      })
    }

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

  return (
    <>
      <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
        <ControlItemsMenu items={controlItems()} />
        <Divider type="vertical" style={{ height: '30px' }} />
        {props.viewSelector}
      </Flex>
      <Card size="small" bordered={false}>
        <ModaTimeline
          data={drillDown ? levelTwoRoadmaps : levelOneRoadmaps}
          groups={
            drillDown
              ? getDataGroups(levelOneRoadmaps, levelTwoRoadmaps)
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
