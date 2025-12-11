'use client'

import { ControlItemsMenu } from '@/src/components/common/control-items-menu'
import {
  ModaDataItem,
  ModaTimeline,
  ModaTimelineOptions,
  TimelineTemplate,
} from '@/src/components/common/timeline'
import { ProgramListDto } from '@/src/services/moda-api'
import { Card, Divider, Flex, Space, Switch, Typography } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import dayjs from 'dayjs'
import { ReactNode, useCallback, useMemo, useState } from 'react'
import { ProgramDrawer } from '.'

const { Text } = Typography

export interface ProgramsTimelineProps {
  programs: ProgramListDto[]
  isLoading: boolean
  refetch: () => void
  viewSelector?: ReactNode
}

interface ProgramTimelineItem extends ModaDataItem<ProgramListDto, string> {
  id: string
  openProgramDrawer: (programKey: number) => void
}

export const ProgramRangeItemTemplate: TimelineTemplate<
  ProgramTimelineItem
> = ({ item, fontColor, foregroundColor }) => {
  return (
    <Text style={{ padding: '5px' }}>
      <a
        onClick={() => item.openProgramDrawer(item.objectData.key)}
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

const ProgramsTimeline: React.FC<ProgramsTimelineProps> = (props) => {
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

  const openProgramDrawer = useCallback(
    (programKey: number) => {
      setSelectedItemKey(programKey)
      showDrawer()
    },
    [showDrawer],
  )

  // Derive timeline items and window synchronously so the timeline receives data on first render
  const processedPrograms = useMemo((): ProgramTimelineItem[] => {
    if (props.isLoading || !props.programs) return []

    return props.programs
      .filter((program) => program.start && program.end)
      .map((program) => ({
        id: String(program.id),
        title: program.name,
        content: program.name,
        objectData: program,
        type: 'range',
        start: new Date(program.start),
        end: new Date(program.end),
        openProgramDrawer: openProgramDrawer,
      }))
  }, [openProgramDrawer, props.isLoading, props.programs])

  const timelineWindow = useMemo(() => {
    let minDate = dayjs()
    let maxDate = dayjs()

    processedPrograms.forEach((program) => {
      if (program.start && dayjs(program.start).isBefore(minDate)) {
        minDate = dayjs(program.start)
      }
      if (program.end && dayjs(program.end).isAfter(maxDate)) {
        maxDate = dayjs(program.end)
      }
    })

    minDate = minDate.subtract(14, 'days')
    maxDate = maxDate.add(1, 'month')

    return { start: minDate.toDate(), end: maxDate.toDate() }
  }, [processedPrograms])

  const timelineOptions = useMemo(
    (): ModaTimelineOptions<ProgramTimelineItem> => ({
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
        <Divider vertical style={{ height: '30px' }} />
        {props.viewSelector}
      </Flex>
      <Card size="small" variant="borderless">
        <ModaTimeline
          data={processedPrograms}
          isLoading={isLoading}
          options={timelineOptions}
          rangeItemTemplate={ProgramRangeItemTemplate}
          allowFullScreen={true}
          allowSaveAsImage={true}
        />
      </Card>
      {selectedItemKey && (
        <ProgramDrawer
          programKey={selectedItemKey}
          drawerOpen={drawerOpen}
          onDrawerClose={onDrawerClose}
        />
      )}
    </>
  )
}

export default ProgramsTimeline
