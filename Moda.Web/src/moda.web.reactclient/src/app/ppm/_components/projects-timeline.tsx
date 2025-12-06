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
import { DataGroup } from 'vis-timeline/standalone'

const { Text } = Typography

export interface ProjectsTimelineProps {
  projects: ProjectListDto[]
  isLoading: boolean
  refetch: () => void
  viewSelector?: ReactNode
  groupByProgram?: boolean
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

  const groups: DataGroup[] = useMemo((): DataGroup[] => {
    if (!props.groupByProgram) {
      return []
    }

    const programGroups: { [key: string]: DataGroup } = {}

    props.projects.forEach((project) => {
      const programName = project.program?.name || 'No Program'
      if (!programGroups[programName]) {
        programGroups[programName] = {
          id: programName,
          content: programName,
        }
      }
    })

    const groups = Object.values(programGroups)

    // if the only group is "No Program", return an empty list
    if (groups.length === 1 && groups[0].id === 'No Program') {
      return []
    }

    // return alphabetically sorted groups with 'No Program' last
    return groups.sort((a, b) => {
      if (a.id === 'No Program') return 1
      if (b.id === 'No Program') return -1
      return String(a.id).localeCompare(String(b.id))
    })
  }, [props.groupByProgram, props.projects])

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
        group: project.program?.name ?? 'No Program',
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

  const projectsNoDatesCount = useMemo(() => {
    return props.projects.filter((project) => !project.start).length
  }, [props.projects])

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
          groups={groups.length > 0 ? groups : undefined}
          isLoading={props.isLoading}
          options={timelineOptions}
          rangeItemTemplate={ProjectRangeItemTemplate}
          allowFullScreen={true}
          allowSaveAsImage={true}
        />
      </Card>
      {projectsNoDatesCount > 0 && (
        <Text type="warning">
          Note: {projectsNoDatesCount}{' '}
          {projectsNoDatesCount === 1 ? 'project is' : 'projects are'} not shown
          on the timeline due to missing dates.
        </Text>
      )}
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
