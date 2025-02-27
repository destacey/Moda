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
import { MessageInstance } from 'antd/es/message/interface'
import dayjs from 'dayjs'
import { ReactNode, useCallback, useEffect, useMemo, useState } from 'react'
import { ProjectDrawer } from '.'

const { Text } = Typography

export interface ProjectsTimelineProps {
  projects: ProjectListDto[]
  isLoading: boolean
  refetch: () => void
  messageApi: MessageInstance
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
  const [isLoading, setIsLoading] = useState(true)
  const [projects, setProjects] = useState<ProjectTimelineItem[]>([])
  const [timelineStart, setTimelineStart] = useState<Date>(dayjs().toDate())
  const [timelineEnd, setTimelineEnd] = useState<Date>(dayjs().toDate())
  const [drawerOpen, setDrawerOpen] = useState(false)
  const [selectedItemKey, setSelectedItemKey] = useState<number | null>(null)

  const [showCurrentTime, setShowCurrentTime] = useState<boolean>(true)

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

  useEffect(() => {
    if (props.isLoading) return

    // filter projects to exclude those without start/end dates
    const filteredProjects: ProjectTimelineItem[] = props.projects
      .filter((project) => project.start && project.end)
      .map((project) => ({
        id: project.id,
        title: project.name,
        content: project.name,
        objectData: project,
        type: 'range',
        start: new Date(project.start),
        end: new Date(project.end),
        openProjectDrawer: openProjectDrawer,
      }))

    setProjects(filteredProjects)

    let minDate = dayjs()
    let maxDate = dayjs()

    filteredProjects.forEach((project) => {
      if (project.start && dayjs(project.start).isBefore(minDate)) {
        minDate = dayjs(project.start)
      }
      if (project.end && dayjs(project.end).isAfter(maxDate)) {
        maxDate = dayjs(project.end)
      }
    })

    minDate = minDate.subtract(14, 'days')
    maxDate = maxDate.add(1, 'month')

    setTimelineStart(minDate.toDate())
    setTimelineEnd(maxDate.toDate())

    setIsLoading(props.isLoading)
  }, [openProjectDrawer, props.isLoading, props.projects])

  const timelineOptions = useMemo(
    (): ModaTimelineOptions<ProjectTimelineItem> => ({
      showCurrentTime: showCurrentTime,
      maxHeight: 650,
      start: timelineStart,
      end: timelineEnd,
      min: timelineStart,
      max: timelineEnd,
    }),
    [showCurrentTime, timelineEnd, timelineStart],
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

  return (
    <>
      <Flex justify="end" align="center">
        <ControlItemsMenu items={controlItems()} />
        <Divider type="vertical" style={{ height: '30px' }} />
        {props.viewSelector}
      </Flex>
      <Card size="small" variant="borderless">
        <ModaTimeline
          data={projects}
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
          messageApi={props.messageApi}
        />
      )}
    </>
  )
}

export default ProjectsTimeline
