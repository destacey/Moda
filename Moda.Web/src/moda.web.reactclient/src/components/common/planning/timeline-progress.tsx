import dayjs from 'dayjs'
import { CSSProperties, FC } from 'react'
import { Card, Flex, Progress, Typography } from 'antd'
import IterationDates from './iteration-dates'

const { Text } = Typography

const DATE_FORMAT = 'MMM D'

export interface TimelineProgressProps {
  start: Date | null
  end: Date | null
  style?: CSSProperties
  dateFormat?: string
}

const TimelineProgress: FC<TimelineProgressProps> = ({
  start,
  end,
  style,
  dateFormat = DATE_FORMAT,
}: TimelineProgressProps) => {
  if (!start || !end) return null

  const now = dayjs()
  const startDate = dayjs(start)
  const endDate = dayjs(end)

  // For calendar day counting, ignore time and count inclusive days
  const startDay = startDate.startOf('day')
  const endDay = endDate.startOf('day')

  // If start date is in the future, show iteration dates instead
  if (now.isBefore(startDay)) {
    return (
      <IterationDates start={start} end={end} dateFormat={dateFormat} style={style} />
    )
  }

  const totalDays = endDay.diff(startDay, 'day') + 1
  const currentDay = Math.min(
    Math.max(now.diff(startDay, 'day') + 1, 0),
    totalDays,
  )
  const progressPercent = Math.round((currentDay / totalDays) * 100)

  return (
    <Card
      size="small"
      style={{ minWidth: 300, width: 'fit-content', ...style }}
    >
      <Flex vertical gap={4}>
        <Text type="secondary">Timeline Progress</Text>
        <Progress
          percent={progressPercent}
          showInfo={false}
          style={{ margin: 0 }}
        />
        <Flex justify="space-between">
          <Text type="secondary" style={{ fontSize: 12 }}>
            {startDate.format(dateFormat)}
          </Text>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {endDate.format(dateFormat)}
          </Text>
        </Flex>
        <Flex justify="center">
          <Text type="secondary" style={{ fontSize: 12 }}>
            Day {currentDay} of {totalDays} ({progressPercent}%)
          </Text>
        </Flex>
      </Flex>
    </Card>
  )
}

export default TimelineProgress
