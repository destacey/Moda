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
import dayjs from 'dayjs'
import { ReactNode, useCallback, useMemo, useState } from 'react'
import { StrategicInitiativeDrawer } from '.'

const { Text } = Typography

export interface StrategicInitiativesTimelineProps {
  strategicInitiatives: StrategicInitiativeListDto[]
  isLoading: boolean
  refetch: () => void
  viewSelector?: ReactNode
}

interface StrategicInitiativeTimelineItem extends ModaDataItem<
  StrategicInitiativeListDto,
  string
> {
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

  const processedStrategicInitiatives =
    useMemo((): StrategicInitiativeTimelineItem[] => {
      if (props.isLoading || !props.strategicInitiatives) return []

      return props.strategicInitiatives
        .filter((initiative) => initiative.start && initiative.end)
        .map((initiative) => ({
          id: String(initiative.id),
          title: initiative.name,
          content: initiative.name,
          objectData: initiative,
          type: 'range',
          start: new Date(initiative.start),
          end: new Date(initiative.end),
          openStrategicInitiativeDrawer: openStrategicInitiativeDrawer,
        }))
    }, [
      openStrategicInitiativeDrawer,
      props.isLoading,
      props.strategicInitiatives,
    ])

  const timelineWindow = useMemo(() => {
    let minDate = dayjs()
    let maxDate = dayjs()

    processedStrategicInitiatives.forEach((initiative) => {
      if (initiative.start && dayjs(initiative.start).isBefore(minDate)) {
        minDate = dayjs(initiative.start)
      }
      if (initiative.end && dayjs(initiative.end).isAfter(maxDate)) {
        maxDate = dayjs(initiative.end)
      }
    })

    minDate = minDate.subtract(14, 'days')
    maxDate = maxDate.add(1, 'month')

    return { start: minDate.toDate(), end: maxDate.toDate() }
  }, [processedStrategicInitiatives])

  const timelineOptions = useMemo(
    (): ModaTimelineOptions<StrategicInitiativeTimelineItem> => ({
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
        <Divider vertical style={{ height: '30px' }} />
        {props.viewSelector}
      </Flex>
      <Card size="small" variant="borderless">
        <ModaTimeline
          data={processedStrategicInitiatives}
          isLoading={props.isLoading}
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
        />
      )}
    </>
  )
}

export default StrategicInitiativesTimeline
