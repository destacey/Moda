'use client'

import { ControlItemsMenu } from '@/src/components/common/control-items-menu'
import {
  ModaDataItem,
  ModaTimeline,
  ModaTimelineOptions,
  TimelineTemplate,
} from '@/src/components/common/timeline'
import { StrategicInitiativeListDto } from '@/src/services/moda-api'
import { Card, Divider, Flex, Space, Switch, Typography } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { MessageInstance } from 'antd/es/message/interface'
import dayjs from 'dayjs'
import { ReactNode, useCallback, useEffect, useMemo, useState } from 'react'
import { StrategicInitiativeDrawer } from '.'

const { Text } = Typography

export interface StrategicInitiativesTimelineProps {
  strategicInitiatives: StrategicInitiativeListDto[]
  isLoading: boolean
  refetch: () => void
  messageApi: MessageInstance
  viewSelector?: ReactNode
}

interface StrategicInitiativeTimelineItem
  extends ModaDataItem<StrategicInitiativeListDto, string> {
  id: string
  openStrategicInitiativeDrawer: (strategicInitiativeKey: number) => void
}

export const StrategicInitiativeRangeItemTemplate: TimelineTemplate<
  StrategicInitiativeTimelineItem
> = ({ item, fontColor, foregroundColor }) => {
  return (
    <Text style={{ padding: '5px' }}>
      <a
        onClick={() => item.openStrategicInitiativeDrawer(item.objectData.key)}
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

const StrategicInitiativesTimeline: React.FC<
  StrategicInitiativesTimelineProps
> = (props) => {
  const [isLoading, setIsLoading] = useState(true)
  const [strategicInitiatives, setStrategicInitiatives] = useState<
    StrategicInitiativeTimelineItem[]
  >([])
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

  const openStrategicInitiativeDrawer = useCallback(
    (strategicInitiativeKey: number) => {
      setSelectedItemKey(strategicInitiativeKey)
      showDrawer()
    },
    [showDrawer],
  )

  useEffect(() => {
    if (props.isLoading) return

    // filter strategic initiatives to exclude those without start/end dates
    const filteredStrategicInitiatives: StrategicInitiativeTimelineItem[] =
      props.strategicInitiatives
        .filter((initiative) => initiative.start && initiative.end)
        .map((initiative) => ({
          id: initiative.id,
          title: initiative.name,
          content: initiative.name,
          objectData: initiative,
          type: 'range',
          start: new Date(initiative.start),
          end: new Date(initiative.end),
          openStrategicInitiativeDrawer: openStrategicInitiativeDrawer,
        }))

    setStrategicInitiatives(filteredStrategicInitiatives)

    let minDate = dayjs()
    let maxDate = dayjs()

    filteredStrategicInitiatives.forEach((initiative) => {
      if (initiative.start && dayjs(initiative.start).isBefore(minDate)) {
        minDate = dayjs(initiative.start)
      }
      if (initiative.end && dayjs(initiative.end).isAfter(maxDate)) {
        maxDate = dayjs(initiative.end)
      }
    })

    minDate = minDate.subtract(14, 'days')
    maxDate = maxDate.add(1, 'month')

    setTimelineStart(minDate.toDate())
    setTimelineEnd(maxDate.toDate())

    setIsLoading(props.isLoading)
  }, [
    openStrategicInitiativeDrawer,
    props.isLoading,
    props.strategicInitiatives,
  ])

  const timelineOptions = useMemo(
    (): ModaTimelineOptions<StrategicInitiativeTimelineItem> => ({
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
          data={strategicInitiatives}
          isLoading={isLoading}
          options={timelineOptions}
          rangeItemTemplate={StrategicInitiativeRangeItemTemplate}
          allowFullScreen={true}
          allowSaveAsImage={true}
        />
      </Card>
      {selectedItemKey && (
        <StrategicInitiativeDrawer
          strategicInitiativeKey={selectedItemKey}
          drawerOpen={drawerOpen}
          onDrawerClose={onDrawerClose}
          messageApi={props.messageApi}
        />
      )}
    </>
  )
}

export default StrategicInitiativesTimeline
