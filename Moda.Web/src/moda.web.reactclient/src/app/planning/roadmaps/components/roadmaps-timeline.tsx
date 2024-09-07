'use client'

import {
  DataItem,
  Timeline,
  TimelineItem,
  TimelineOptions,
} from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import 'vis-timeline/styles/vis-timeline-graph2d.css'
import './roadmaps-timeline.css'
import { DataGroup } from 'vis-timeline/standalone/esm/vis-timeline-graph2d'
import Link from 'next/link'
import { Card, Flex, Spin, Typography } from 'antd'
import { RoadmapDetailsDto, RoadmapListDto } from '@/src/services/moda-api'
import { use, useEffect, useMemo, useRef, useState } from 'react'
import { createRoot } from 'react-dom/client'
import useTheme from '@/src/app/components/contexts/theme'
import dayjs from 'dayjs'
import { set } from 'lodash'
import { ModaEmpty } from '@/src/app/components/common'

const { Text } = Typography

interface RoadmapsTimelineProps {
  roadmap: RoadmapDetailsDto
  roadmaps: RoadmapListDto[]
  isLoading: boolean
  refreshRoadmap: () => void
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
  groupNames: string[],
  roadmaps: TimelineItem[],
  timelineFontColor: string,
): DataGroup[] => {
  let groups = []
  if (!groupNames || groupNames.length === 0) {
    groups = roadmaps.reduce((acc, roadmap) => {
      if (!acc.includes(roadmap.group)) {
        acc.push(roadmap.group)
      }
      return acc
    }, [])
  } else {
    groups = groupNames
  }

  return groups.map((roadmap) => {
    return {
      id: roadmap,
      content: roadmap,
      style: `color: ${timelineFontColor};`,
    } as DataGroup
  })
}

const RoadmapsTimeline = (props: RoadmapsTimelineProps) => {
  const [isLoading, setIsLoading] = useState(true)
  const [timelineStart, setTimelineStart] = useState<Date>(undefined)
  const [timelineEnd, setTimelineEnd] = useState<Date>(undefined)
  const [roadmaps, setRoadmaps] = useState<RoadmapTimelineItem[]>([])
  const timelineRef = useRef<HTMLDivElement>(null)

  const { currentThemeName } = useTheme()
  const timelineBackgroundColor =
    currentThemeName === 'light' ? '#f5f5f5' : '#303030'
  const timelineForegroundColor =
    currentThemeName === 'light' ? '#c7edff' : '#17354d'
  const timelineFontColor = currentThemeName === 'light' ? '#4d4d4d' : '#FFFFFF'

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
      showCurrentTime: true,
      verticalScroll: true,
      zoomKey: 'ctrlKey',
      start: dayjs(timelineStart).toDate(),
      end: dayjs(timelineEnd).toDate(),
      min: dayjs(timelineStart).toDate(),
      max: dayjs(timelineEnd).toDate(),
      groupOrder: 'content',
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
  }, [timelineStart, timelineEnd, timelineFontColor, timelineForegroundColor])

  useEffect(() => {
    if (!props.roadmaps) return

    setTimelineStart(props.roadmap.start)
    setTimelineEnd(props.roadmap.end)

    const roadmaps = props.roadmaps.map((roadmap) => {
      return {
        id: roadmap.key,
        title: roadmap.name,
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

    setRoadmaps(roadmaps)
    setIsLoading(props.isLoading)
  }, [props, timelineBackgroundColor])

  useEffect(() => {
    if (!roadmaps || roadmaps.length === 0 || isLoading) return

    // TODO: add the ability for content to overflow if the text is too long
    const items: RoadmapTimelineItem[] = [...roadmaps]

    const timeline = new Timeline(
      timelineRef.current,
      items,
      options as TimelineOptions,
    )

    // if (enableGroups === true) {
    //   timeline.setGroups(
    //     getDataGroups(teamNames, objectives, timelineFontColor),
    //   )
    // }
  }, [isLoading, options, roadmaps])

  const TimelineChart = () => {
    if (!roadmaps || roadmaps.length === 0) {
      return <ModaEmpty message="No roadmaps" />
    }

    return (
      <>
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
