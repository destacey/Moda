import React from 'react'
import { render, screen } from '@testing-library/react'
import IterationDates from './iteration-dates'

describe('IterationDates', () => {
  const startDate = new Date('2025-10-26T17:00:00')
  const endDate = new Date('2025-11-08T16:00:00')

  it('renders start date, end date, and duration', () => {
    render(<IterationDates start={startDate} end={endDate} />)

    expect(screen.getByText('Start Date')).toBeInTheDocument()
    expect(screen.getByText('End Date')).toBeInTheDocument()
    expect(screen.getByText('Duration')).toBeInTheDocument()

    expect(screen.getByText('Oct 26, 2025 5:00 PM')).toBeInTheDocument()
    expect(screen.getByText('Nov 8, 2025 4:00 PM')).toBeInTheDocument()
    expect(screen.getByText('14 Days')).toBeInTheDocument()
  })

  it('calculates duration correctly (calendar days, inclusive)', () => {
    render(<IterationDates start={startDate} end={endDate} />)

    // Oct 26 to Nov 8 is 14 calendar days (13 days difference + 1 for inclusive)
    expect(screen.getByText('14 Days')).toBeInTheDocument()
  })

  it('renders arrow separator between dates', () => {
    render(<IterationDates start={startDate} end={endDate} />)

    expect(screen.getByText('→')).toBeInTheDocument()
  })

  it('returns null when start date is null', () => {
    const { container } = render(<IterationDates start={null} end={endDate} />)

    expect(container.firstChild).toBeNull()
  })

  it('returns null when end date is null', () => {
    const { container } = render(
      <IterationDates start={startDate} end={null} />,
    )

    expect(container.firstChild).toBeNull()
  })

  it('returns null when both dates are null', () => {
    const { container } = render(<IterationDates start={null} end={null} />)

    expect(container.firstChild).toBeNull()
  })

  it('handles single day duration', () => {
    const sameDate = new Date('2025-10-26T17:00:00')
    render(<IterationDates start={sameDate} end={sameDate} />)

    expect(screen.getByText('1 Days')).toBeInTheDocument()
  })

  it('handles multi-week duration', () => {
    const start = new Date('2025-01-01T00:00:00')
    const end = new Date('2025-01-29T00:00:00')
    render(<IterationDates start={start} end={end} />)

    expect(screen.getByText('29 Days')).toBeInTheDocument()
  })

  it('ignores time when counting days', () => {
    const morningStart = new Date('2025-10-26T06:00:00')
    const eveningEnd = new Date('2025-11-08T16:00:00')
    render(<IterationDates start={morningStart} end={eveningEnd} />)

    // Should be same as 5pm start - time should be ignored
    expect(screen.getByText('14 Days')).toBeInTheDocument()
  })

  it('formats dates correctly with AM/PM', () => {
    const morningStart = new Date('2025-10-26T09:30:00')
    const afternoonEnd = new Date('2025-11-08T14:45:00')
    render(<IterationDates start={morningStart} end={afternoonEnd} />)

    expect(screen.getByText('Oct 26, 2025 9:30 AM')).toBeInTheDocument()
    expect(screen.getByText('Nov 8, 2025 2:45 PM')).toBeInTheDocument()
  })

  it('handles midnight times correctly', () => {
    const midnightStart = new Date('2025-10-26T00:00:00')
    const midnightEnd = new Date('2025-11-08T00:00:00')
    render(<IterationDates start={midnightStart} end={midnightEnd} />)

    expect(screen.getByText('Oct 26, 2025 12:00 AM')).toBeInTheDocument()
    expect(screen.getByText('Nov 8, 2025 12:00 AM')).toBeInTheDocument()
  })

  it('handles noon times correctly', () => {
    const noonStart = new Date('2025-10-26T12:00:00')
    const noonEnd = new Date('2025-11-08T12:00:00')
    render(<IterationDates start={noonStart} end={noonEnd} />)

    expect(screen.getByText('Oct 26, 2025 12:00 PM')).toBeInTheDocument()
    expect(screen.getByText('Nov 8, 2025 12:00 PM')).toBeInTheDocument()
  })

  it('hides duration when showDurationDays is false', () => {
    render(
      <IterationDates
        start={startDate}
        end={endDate}
        showDurationDays={false}
      />,
    )

    expect(screen.getByText('Start Date')).toBeInTheDocument()
    expect(screen.getByText('End Date')).toBeInTheDocument()
    expect(screen.queryByText('Duration')).not.toBeInTheDocument()
    expect(screen.queryByText('14 Days')).not.toBeInTheDocument()
  })

  it('shows duration by default', () => {
    render(<IterationDates start={startDate} end={endDate} />)

    expect(screen.getByText('Duration')).toBeInTheDocument()
    expect(screen.getByText('14 Days')).toBeInTheDocument()
  })

  it('applies default width fit-content style', () => {
    const { container } = render(
      <IterationDates start={startDate} end={endDate} />,
    )

    const card = container.querySelector('.ant-card')
    expect(card).toHaveStyle({ width: 'fit-content' })
  })

  it('applies custom styles', () => {
    const { container } = render(
      <IterationDates
        start={startDate}
        end={endDate}
        style={{ width: '100%', padding: '10px' }}
      />,
    )

    const card = container.querySelector('.ant-card')
    expect(card).toHaveStyle({ width: '100%' })
    expect(card).toHaveStyle({ padding: '10px' })
  })

  it('custom styles override default styles', () => {
    const { container } = render(
      <IterationDates
        start={startDate}
        end={endDate}
        style={{ width: '500px' }}
      />,
    )

    const card = container.querySelector('.ant-card')
    expect(card).toHaveStyle({ width: '500px' })
  })

  it('uses custom dateFormat when provided', () => {
    render(
      <IterationDates
        start={startDate}
        end={endDate}
        dateFormat="MMM D"
      />,
    )

    expect(screen.getByText('Oct 26')).toBeInTheDocument()
    expect(screen.getByText('Nov 8')).toBeInTheDocument()
  })

  it('uses default date format with full datetime', () => {
    render(<IterationDates start={startDate} end={endDate} />)

    // Default format is 'MMM D, YYYY h:mm A'
    expect(screen.getByText('Oct 26, 2025 5:00 PM')).toBeInTheDocument()
    expect(screen.getByText('Nov 8, 2025 4:00 PM')).toBeInTheDocument()
  })
})
