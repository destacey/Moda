import dayjs from 'dayjs'
import { DateRange } from '../types'

const SHORT_DATE_FORMAT = 'MMM D'
const DATE_FORMAT = 'MMM D, YYYY'
const SHORT_DATE_TIME_FORMAT = 'MMM D, h:mm A'
const DATE_TIME_FORMAT = 'MMM D, YYYY h:mm A'

export interface ModaDateRangeProps {
  dateRange: DateRange
  withTime?: boolean
}

const ModaDateRange = ({ dateRange, withTime = false }: ModaDateRangeProps) => {
  if (!dateRange || !dateRange.start || !dateRange.end) return null

  const shortFormat = withTime ? SHORT_DATE_TIME_FORMAT : SHORT_DATE_FORMAT
  const fullFormat = withTime ? DATE_TIME_FORMAT : DATE_FORMAT

  const start = dayjs(dateRange.start).isSame(dayjs(dateRange.end), 'year')
    ? dayjs(dateRange.start).format(shortFormat)
    : dayjs(dateRange.start).format(fullFormat)

  return (
    <span>
      {start} - {dayjs(dateRange.end).format(fullFormat)}
    </span>
  )
}

export default ModaDateRange
