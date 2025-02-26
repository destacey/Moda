'use client'

import { ControlItemsMenu } from '@/src/components/common/control-items-menu'
import {
  ModaDataItem,
  ModaTimeline,
  ModaTimelineOptions,
} from '@/src/components/common/timeline'
import { ProjectListDto } from '@/src/services/moda-api'
import { Card, Divider, Flex, Space, Switch, Typography } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { MessageInstance } from 'antd/es/message/interface'
import dayjs from 'dayjs'
import { ReactNode, useEffect, useMemo, useState } from 'react'

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
}

const ProjectsTimeline: React.FC<ProjectsTimelineProps> = (props) => {
  const [isLoading, setIsLoading] = useState(true)
  const [projects, setProjects] = useState<ProjectTimelineItem[]>([])
  const [timelineStart, setTimelineStart] = useState<Date | undefined>(
    undefined,
  )
  const [timelineEnd, setTimelineEnd] = useState<Date | undefined>(undefined)

  const [showCurrentTime, setShowCurrentTime] = useState<boolean>(true)

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

    // Adjust minDate and maxDate at the end of processing the list
    minDate = minDate.subtract(14, 'days')
    maxDate = maxDate.add(1, 'month')

    setTimelineStart(minDate.toDate())
    setTimelineEnd(maxDate.toDate())

    setIsLoading(props.isLoading)
  }, [props.isLoading, props.projects])

  const timelineOptions = useMemo(
    (): ModaTimelineOptions<ProjectTimelineItem> =>
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

  return (
    <>
      <Flex justify="end" align="center">
        <ControlItemsMenu items={controlItems()} />
        <Divider type="vertical" style={{ height: '30px' }} />
        {props.viewSelector}
      </Flex>
      <Card size="small" bordered={false}>
        <ModaTimeline
          data={projects}
          isLoading={isLoading}
          options={timelineOptions}
          allowFullScreen={true}
          allowSaveAsImage={true}
        />
      </Card>
    </>
  )
}

export default ProjectsTimeline
