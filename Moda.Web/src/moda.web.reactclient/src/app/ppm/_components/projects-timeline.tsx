'use client'

import { ControlItemsMenu } from '@/src/components/common/control-items-menu'
import {
  ModaDataItem,
  ModaTimeline,
  ModaTimelineOptions,
  TimelineTemplate,
} from '@/src/components/common/timeline'
import { ProjectListDto } from '@/src/services/moda-api'
import { Card, Divider, Flex, Space, Switch, Typography } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import dayjs from 'dayjs'
import { ReactNode, useCallback, useMemo, useState } from 'react'
import { ProjectDrawer } from '.'

const { Text } = Typography

export interface ProjectsTimelineProps {
  projects: ProjectListDto[]
  isLoading: boolean
  refetch: () => void
  viewSelector?: ReactNode
}

interface ProjectTimelineItem extends ModaDataItem<ProjectListDto, string> {
  id: string
  openProjectDrawer: (projectKey: number) => void
}

export const ProjectRangeItemTemplate: TimelineTemplate<
  ProjectTimelineItem
> = ({ item, fontColor, foregroundColor }) => {
  return (
    <Text style={{ padding: '5px' }}>
      <a
        onClick={() => item.openProjectDrawer(item.objectData.key)}
        style={{ textDecoration: 'none' }}
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

const ProjectsTimeline: React.FC<ProjectsTimelineProps> = (props) => {
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [selectedItemKey, setSelectedItemKey] = useState<number | null>(null)
  const [showCurrentTime, setShowCurrentTime] = useState<boolean>(true)
  // ...existing code...

  const showDrawer = useCallback(() => {
    setDrawerOpen(true)
  }, [])

  const onDrawerClose = useCallback(() => {
    setDrawerOpen(false)
    setSelectedItemKey(null)
  }, [])

  const openProjectDrawer = useCallback(
    (projectKey: number) => {
      setSelectedItemKey(projectKey)
      showDrawer()
    },
    [showDrawer],
  )

  // Derive timeline items and window synchronously so the timeline receives data on first render
  const processedProjects = useMemo((): ProjectTimelineItem[] => {
    if (props.isLoading || !props.projects) return []

    return props.projects
      .filter((project) => project.start && project.end)
      .map((project) => ({
        id: String(project.id),
        title: project.name,
        content: project.name,
        objectData: project,
        type: 'range',
        start: new Date(project.start),
        end: new Date(project.end),
        openProjectDrawer: openProjectDrawer,
      }))
  }, [openProjectDrawer, props.isLoading, props.projects])

  const timelineWindow = useMemo(() => {
    let minDate = dayjs()
    let maxDate = dayjs()

    processedProjects.forEach((project) => {
      if (project.start && dayjs(project.start).isBefore(minDate)) {
        minDate = dayjs(project.start)
      }
      if (project.end && dayjs(project.end).isAfter(maxDate)) {
        maxDate = dayjs(project.end)
      }
    })

    minDate = minDate.subtract(14, 'days')
    maxDate = maxDate.add(1, 'month')

    return { start: minDate.toDate(), end: maxDate.toDate() }
  }, [processedProjects])

  const timelineOptions = useMemo(
    (): ModaTimelineOptions<ProjectTimelineItem> => ({
      showCurrentTime: showCurrentTime,
      maxHeight: 650,
      start: timelineWindow.start,
      end: timelineWindow.end,
      min: timelineWindow.start,
      max: timelineWindow.end,
    }),
    [showCurrentTime, timelineWindow.end, timelineWindow.start],
  )

  const onShowCurrentTimeChange = useCallback((checked: boolean) => {
    setShowCurrentTime(checked)
  }, [])

  const controlItems = useCallback((): ItemType[] => {
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
  }, [showCurrentTime, onShowCurrentTimeChange])

  const isLoading = props.isLoading

  return (
    <>
      <Flex justify="end" align="center">
        <ControlItemsMenu items={controlItems()} />
        <Divider type="vertical" style={{ height: '30px' }} />
        {props.viewSelector}
      </Flex>
      <Card size="small" variant="borderless">
        <ModaTimeline
          data={processedProjects}
          isLoading={isLoading}
          options={timelineOptions}
          rangeItemTemplate={ProjectRangeItemTemplate}
          allowFullScreen={true}
          allowSaveAsImage={true}
        />
      </Card>
      {selectedItemKey && (
        <ProjectDrawer
          projectKey={selectedItemKey}
          drawerOpen={drawerOpen}
          onDrawerClose={onDrawerClose}
        />
      )}
    </>
  )
}

export default ProjectsTimeline
