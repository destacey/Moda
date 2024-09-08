'use client'

import {
  DataItem,
  Timeline,
  TimelineOptions,
} from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import 'vis-timeline/styles/vis-timeline-graph2d.css'
import './roadmaps-timeline.css'
import { DataGroup } from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import {
  Button,
  Card,
  Divider,
  Dropdown,
  Flex,
  Space,
  Spin,
  Switch,
  Typography,
} from 'antd'
import { RoadmapDetailsDto, RoadmapListDto } from '@/src/services/moda-api'
import { useEffect, useMemo, useRef, useState } from 'react'
import { createRoot } from 'react-dom/client'
import useTheme from '@/src/app/components/contexts/theme'
import dayjs from 'dayjs'
import { ModaEmpty } from '@/src/app/components/common'
import { ItemType } from 'antd/es/menu/interface'
import { ControlOutlined } from '@ant-design/icons'
import { useGetRoadmapLinksQuery } from '@/src/store/features/planning/roadmaps-api'

const { Text } = Typography

interface RoadmapsTimelineProps {
  roadmap: RoadmapDetailsDto
  roadmaps: RoadmapListDto[]
  isLoading: boolean
  refreshRoadmap: () => void
  viewSelector?: React.ReactNode | undefined
}

// TODO: abstract this into a shared component
interface RoadmapTimelineItem extends DataItem {
  id: number
  title?: string
  content: string
  start: Date
  end: Date
  group?: string
  type?: string
  roadmap?: RoadmapListDto
  order?: number
}

interface RoadmapTimelineTemplateProps {
  roadmap: RoadmapListDto
  timelineFontColor: string
  timelineForegroundColor: string
}

export const RoadmapTimelineTemplate = ({
  roadmap,
  timelineFontColor,
  timelineForegroundColor,
}: RoadmapTimelineTemplateProps) => {
  return (
    <div>
      <Text style={{ padding: '5px', color: timelineFontColor }}>
        {roadmap.name}
      </Text>
    </div>
  )
}

const getDataGroups = (
  groupItems: RoadmapTimelineItem[],
  roadmaps: RoadmapTimelineItem[],
  timelineFontColor: string,
): DataGroup[] => {
  let groups = []
  if (!groupItems || groupItems.length === 0) {
    groups = roadmaps.reduce((acc, roadmap) => {
      if (!acc.includes(roadmap.group)) {
        acc.push(roadmap.group)
      }
      return acc
    }, [])
  } else {
    groups = groupItems
  }

  return groups.map((group) => {
    return {
      id: group.roadmap.id,
      content: group.roadmap.name,
      style: `color: ${timelineFontColor};`,
    } as DataGroup
  })
}

const RoadmapsTimeline = (props: RoadmapsTimelineProps) => {
  const [isLoading, setIsLoading] = useState(true)
  const [timelineStart, setTimelineStart] = useState<Date>(undefined)
  const [timelineEnd, setTimelineEnd] = useState<Date>(undefined)
  const [levelOneRoadmaps, setLevelOneRoadmaps] = useState<
    RoadmapTimelineItem[]
  >([])
  const [levelTwoRoadmaps, setLevelTwoRoadmaps] = useState<
    RoadmapTimelineItem[]
  >([])
  const timelineRef = useRef<HTMLDivElement>(null)

  const [drillDown, setDrillDown] = useState<boolean>(false)
  const [showCurrentTime, setShowCurrentTime] = useState<boolean>(true)

  const { currentThemeName } = useTheme()
  const timelineBackgroundColor =
    currentThemeName === 'light' ? '#f5f5f5' : '#303030'
  const timelineForegroundColor =
    currentThemeName === 'light' ? '#c7edff' : '#17354d'
  const timelineFontColor = currentThemeName === 'light' ? '#4d4d4d' : '#FFFFFF'

  const {
    data: roadmapLinksData,
    isLoading: isLoadingRoadmapLinks,
    error: errorRoadmapLinks,
    refetch: refetchRoadmapLinks,
  } = useGetRoadmapLinksQuery(props.roadmaps?.map((r) => r.id) || [], {
    skip: !props.roadmaps && props.roadmaps.length === 0,
  })

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
      showCurrentTime: showCurrentTime,
      verticalScroll: true,
      zoomKey: 'ctrlKey',
      start: dayjs(timelineStart).toDate(),
      end: dayjs(timelineEnd).toDate(),
      min: dayjs(timelineStart).toDate(),
      max: dayjs(timelineEnd).toDate(),
      groupOrder: 'order',
      xss: { disabled: false },
      template: (
        item: RoadmapTimelineItem,
        element: HTMLElement,
        data: any,
      ) => {
        if (item.type === 'range') {
          // TODO: this is throwing a lot of warnings in the console.  You are calling ReactDOMClient.createRoot() on a container that has already been passed to createRoot() before. Instead, call root.render() on the existing root instead if you want to update it.
          createRoot(element).render(
            <RoadmapTimelineTemplate
              roadmap={item.roadmap}
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
  }, [
    showCurrentTime,
    timelineStart,
    timelineEnd,
    timelineFontColor,
    timelineForegroundColor,
  ])

  useEffect(() => {
    if (!props.roadmaps) return

    setTimelineStart(props.roadmap.start)
    setTimelineEnd(props.roadmap.end)

    const levelOneRoadmaps = props.roadmaps.map((roadmap) => {
      return {
        id: roadmap.key,
        title: `${roadmap.key} - ${roadmap.name}`,
        content: '',
        start: dayjs(roadmap.start).toDate(),
        end: dayjs(roadmap.end).toDate(),
        group: null,
        type: 'range',
        style: `background: ${timelineBackgroundColor}; border-color: ${timelineBackgroundColor};`,
        zIndex: 1,
        roadmap: roadmap,
      } as RoadmapTimelineItem
    })
    setLevelOneRoadmaps(levelOneRoadmaps)

    const levelTwoRoadmaps = roadmapLinksData?.map((roadmapLink) => {
      return {
        id: roadmapLink.roadmap.key,
        title: `${roadmapLink.roadmap.key} - ${roadmapLink.roadmap.name}`,
        content: '',
        start: dayjs(roadmapLink.roadmap.start).toDate(),
        end: dayjs(roadmapLink.roadmap.end).toDate(),
        group: roadmapLink.parentId,
        type: 'range',
        style: `background: ${timelineBackgroundColor}; border-color: ${timelineBackgroundColor};`,
        zIndex: 1,
        roadmap: roadmapLink.roadmap,
      } as RoadmapTimelineItem
    })
    setLevelTwoRoadmaps(levelTwoRoadmaps)

    setIsLoading(props.isLoading)
  }, [drillDown, props, roadmapLinksData, timelineBackgroundColor])

  useEffect(() => {
    // TODO: add the ability for content to overflow if the text is too long
    let items: RoadmapTimelineItem[]
    if (drillDown) {
      if (!levelTwoRoadmaps || levelTwoRoadmaps.length === 0 || isLoading)
        return
      items = [...levelTwoRoadmaps]
    } else {
      if (!levelOneRoadmaps || levelOneRoadmaps.length === 0 || isLoading)
        return
      items = [...levelOneRoadmaps]
    }

    const timeline = new Timeline(
      timelineRef.current,
      items,
      options as TimelineOptions,
    )

    if (drillDown === true) {
      const groups = getDataGroups(
        levelOneRoadmaps,
        levelTwoRoadmaps,
        timelineFontColor,
      )

      timeline.setGroups(groups)
    }
  }, [
    drillDown,
    isLoading,
    options,
    levelOneRoadmaps,
    levelTwoRoadmaps,
    timelineFontColor,
  ])

  const onDrillDownChange = (checked: boolean) => {
    setDrillDown(checked)
  }

  const onShowCurrentTimeChange = (checked: boolean) => {
    setShowCurrentTime(checked)
  }

  const controlItems: ItemType[] = [
    {
      label: (
        <>
          <Space direction="vertical" size="small">
            {levelTwoRoadmaps && levelTwoRoadmaps.length > 0 && (
              <Space>
                <Switch
                  size="small"
                  checked={drillDown}
                  onChange={onDrillDownChange}
                />
                Drill Down
              </Space>
            )}
            <Space>
              <Switch
                size="small"
                checked={showCurrentTime}
                onChange={onShowCurrentTimeChange}
              />
              Show Current Time
            </Space>
          </Space>
        </>
      ),
      key: '0',
    },
  ]

  const TimelineChart = () => {
    if (!levelOneRoadmaps || levelOneRoadmaps.length === 0) {
      return <ModaEmpty message="No roadmaps" />
    }

    return (
      <>
        <Flex justify="end" align="center" style={{ paddingBottom: '16px' }}>
          <Dropdown menu={{ items: controlItems }} trigger={['click']}>
            <Button type="text" shape="circle" icon={<ControlOutlined />} />
          </Dropdown>
          <Divider type="vertical" style={{ height: '30px' }} />
          {props.viewSelector}
        </Flex>
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

export default RoadmapsTimeline
