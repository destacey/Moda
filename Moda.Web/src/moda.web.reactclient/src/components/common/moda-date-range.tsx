import dayjs from 'dayjs'
import { DateRange } from '../types'

export interface ModaDateRangeProps {
  dateRange: DateRange
}

const ModaDateRange = ({ dateRange }: ModaDateRangeProps) => {
  if (!dateRange || !dateRange.start || !dateRange.end) return null

  const start = dayjs(dateRange.start).isSame(dayjs(dateRange.end), 'year')
    ? dayjs(dateRange.start).format('MMM D')
    : dayjs(dateRange.start).format('MMM D, YYYY')

  return (
    <span>
      {start} - {dayjs(dateRange.end).format('MMM D, YYYY')}
    </span>
  )
}

export default ModaDateRange
