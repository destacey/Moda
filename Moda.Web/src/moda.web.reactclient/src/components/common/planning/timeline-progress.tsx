import dayjs from 'dayjs'
import { CSSProperties, FC } from 'react'
import { Card, Flex, Grid, Progress, Typography } from 'antd'
import IterationDates from './iteration-dates'

const { Text } = Typography
const { useBreakpoint } = Grid

const DATE_FORMAT = 'MMM D'

export interface TimelineProgressProps {
  start: Date | null
  end: Date | null
  variant?: 'outlined' | 'borderless'
  size?: 'default' | 'small'
  style?: CSSProperties
  dateFormat?: string
}

const TimelineProgress: FC<TimelineProgressProps> = ({
  start,
  end,
  variant = 'outlined',
  size = 'default',
  style,
  dateFormat = DATE_FORMAT,
}: TimelineProgressProps) => {
  const screens = useBreakpoint()
  const isMobile = !screens.md // Mobile/tablet when viewport is below the md breakpoint (< 768px)

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
      <IterationDates
        start={start}
        end={end}
        dateFormat={dateFormat}
        style={style}
      />
    )
  }

  const totalDays = endDay.diff(startDay, 'day') + 1
  const currentDay = Math.min(
    Math.max(now.diff(startDay, 'day') + 1, 0),
    totalDays,
  )
  const progressPercent = Math.round((currentDay / totalDays) * 100)

  const fontSize = size === 'small' ? 11 : 12

  const borderlessStyles = variant === 'borderless'
    ? { boxShadow: 'none', background: 'transparent' }
    : {}

  const cardStyle = isMobile
    ? { width: '100%', ...borderlessStyles, ...style }
    : { minWidth: 275, width: 'fit-content', ...borderlessStyles, ...style }

  const cardBodyStyle =
    variant === 'borderless'
      ? { background: 'transparent', padding: 0 }
      : undefined

  return (
    <Card size="small" style={cardStyle} variant={variant} styles={{ body: cardBodyStyle }}>
      <Flex vertical gap={4}>
        <Text type="secondary" style={{ fontSize }}>Timeline</Text>
        <Progress
          percent={progressPercent}
          showInfo={false}
          style={{ margin: 0 }}
        />
        <Flex justify="space-between">
          <Text type="secondary" style={{ fontSize }}>
            {startDate.format(dateFormat)}
          </Text>
          <Text type="secondary" style={{ fontSize }}>
            {endDate.format(dateFormat)}
          </Text>
        </Flex>
        <Flex justify="center">
          <Text type="secondary" style={{ fontSize }}>
            Day {currentDay} of {totalDays} ({progressPercent}%)
          </Text>
        </Flex>
      </Flex>
    </Card>
  )
}

export default TimelineProgress
