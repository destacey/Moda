import React from 'react'
import { render, screen } from '@testing-library/react'
import dayjs from 'dayjs'
import TimelineProgress from './timeline-progress'

// Mock IterationDates
jest.mock('./iteration-dates', () => ({
  __esModule: true,
  default: ({ start, end, dateFormat, style }: { start: Date; end: Date; dateFormat?: string; style?: React.CSSProperties }) => (
    <div data-testid="iteration-dates" data-dateformat={dateFormat} data-style={JSON.stringify(style)}>
      Iteration Dates Mock
    </div>
  ),
}))

// Mock Ant Design Grid useBreakpoint hook
const mockUseBreakpoint = jest.fn(() => ({
  xs: true,
  sm: true,
  md: true, // Default to desktop (md and above)
  lg: true,
  xl: true,
  xxl: true,
}))

jest.mock('antd', () => {
  const actualAntd = jest.requireActual('antd')
  return {
    ...actualAntd,
    Grid: {
      ...actualAntd.Grid,
      useBreakpoint: mockUseBreakpoint,
    },
  }
})

// Mock dayjs to control "now" for consistent tests
jest.mock('dayjs', () => {
  const originalDayjs = jest.requireActual('dayjs')
  const mockDayjs = (date?: string | Date) => {
    if (date === undefined) {
      // Return mocked "now"
      return originalDayjs(mockDayjs.mockedNow)
    }
    return originalDayjs(date)
  }
  mockDayjs.mockedNow = '2025-11-01T12:00:00'
  Object.assign(mockDayjs, originalDayjs)
  return mockDayjs
})

describe('TimelineProgress', () => {
  const startDate = new Date('2025-10-26T17:00:00')
  const endDate = new Date('2025-11-08T16:00:00')

  beforeEach(() => {
    // Set "now" to Nov 1, which is day 7 of 14 (50%)
    ;(dayjs as unknown as { mockedNow: string }).mockedNow = '2025-11-01T12:00:00'
  })

  it('renders title, dates, and progress info', () => {
    render(<TimelineProgress start={startDate} end={endDate} />)

    expect(screen.getByText('Timeline Progress')).toBeInTheDocument()
    expect(screen.getByText('Oct 26')).toBeInTheDocument()
    expect(screen.getByText('Nov 8')).toBeInTheDocument()
    expect(screen.getByText('Day 7 of 14 (50%)')).toBeInTheDocument()
  })

  it('renders progress bar', () => {
    const { container } = render(
      <TimelineProgress start={startDate} end={endDate} />,
    )

    const progressBar = container.querySelector('.ant-progress')
    expect(progressBar).toBeInTheDocument()
  })

  it('returns null when start date is null', () => {
    const { container } = render(
      <TimelineProgress start={null} end={endDate} />,
    )

    expect(container.firstChild).toBeNull()
  })

  it('returns null when end date is null', () => {
    const { container } = render(
      <TimelineProgress start={startDate} end={null} />,
    )

    expect(container.firstChild).toBeNull()
  })

  it('returns null when both dates are null', () => {
    const { container } = render(<TimelineProgress start={null} end={null} />)

    expect(container.firstChild).toBeNull()
  })

  it('calculates total days correctly (inclusive)', () => {
    render(<TimelineProgress start={startDate} end={endDate} />)

    // Oct 26 to Nov 8 is 14 calendar days
    expect(screen.getByText(/of 14/)).toBeInTheDocument()
  })

  it('handles single day duration', () => {
    const sameDate = new Date('2025-11-01T17:00:00')
    render(<TimelineProgress start={sameDate} end={sameDate} />)

    expect(screen.getByText('Day 1 of 1 (100%)')).toBeInTheDocument()
  })

  it('renders IterationDates when start date is in the future', () => {
    // Set "now" to before the start date
    ;(dayjs as unknown as { mockedNow: string }).mockedNow = '2025-10-20T12:00:00'

    render(<TimelineProgress start={startDate} end={endDate} />)

    expect(screen.getByTestId('iteration-dates')).toBeInTheDocument()
    expect(screen.queryByText('Timeline Progress')).not.toBeInTheDocument()
  })

  it('passes props to IterationDates when falling back', () => {
    // Set "now" to before the start date
    ;(dayjs as unknown as { mockedNow: string }).mockedNow = '2025-10-20T12:00:00'

    render(
      <TimelineProgress
        start={startDate}
        end={endDate}
        dateFormat="MMM D - h:mm A"
        style={{ width: '100%' }}
      />,
    )

    const iterationDates = screen.getByTestId('iteration-dates')
    expect(iterationDates).toHaveAttribute('data-dateformat', 'MMM D - h:mm A')
    expect(iterationDates).toHaveAttribute('data-style', JSON.stringify({ width: '100%' }))
  })

  it('clamps current day to total when after end date', () => {
    // Set "now" to after the end date
    ;(dayjs as unknown as { mockedNow: string }).mockedNow = '2025-11-15T12:00:00'

    render(<TimelineProgress start={startDate} end={endDate} />)

    expect(screen.getByText('Day 14 of 14 (100%)')).toBeInTheDocument()
  })

  it('calculates progress at start of timeline', () => {
    // Set "now" to start date
    ;(dayjs as unknown as { mockedNow: string }).mockedNow = '2025-10-26T12:00:00'

    render(<TimelineProgress start={startDate} end={endDate} />)

    expect(screen.getByText('Day 1 of 14 (7%)')).toBeInTheDocument()
  })

  it('calculates progress at end of timeline', () => {
    // Set "now" to end date
    ;(dayjs as unknown as { mockedNow: string }).mockedNow = '2025-11-08T12:00:00'

    render(<TimelineProgress start={startDate} end={endDate} />)

    expect(screen.getByText('Day 14 of 14 (100%)')).toBeInTheDocument()
  })

  it('uses custom dateFormat when provided', () => {
    render(
      <TimelineProgress
        start={startDate}
        end={endDate}
        dateFormat="MMM D - h:mm A"
      />,
    )

    expect(screen.getByText('Oct 26 - 5:00 PM')).toBeInTheDocument()
    expect(screen.getByText('Nov 8 - 4:00 PM')).toBeInTheDocument()
  })

  it('uses default date format without time', () => {
    render(<TimelineProgress start={startDate} end={endDate} />)

    // Default format is 'MMM D'
    expect(screen.getByText('Oct 26')).toBeInTheDocument()
    expect(screen.getByText('Nov 8')).toBeInTheDocument()
  })

  it('preserves time in date display when format includes time', () => {
    const morningStart = new Date('2025-10-26T09:30:00')
    const afternoonEnd = new Date('2025-11-08T14:45:00')

    render(
      <TimelineProgress
        start={morningStart}
        end={afternoonEnd}
        dateFormat="MMM D h:mm A"
      />,
    )

    expect(screen.getByText('Oct 26 9:30 AM')).toBeInTheDocument()
    expect(screen.getByText('Nov 8 2:45 PM')).toBeInTheDocument()
  })

  it('applies default minWidth style on desktop', () => {
    const { container } = render(
      <TimelineProgress start={startDate} end={endDate} />,
    )

    const card = container.querySelector('.ant-card')
    expect(card).toHaveStyle({ minWidth: '275px', width: 'fit-content' })
  })

  it('applies full width on mobile', () => {
    // Mock mobile breakpoint (md is false when screen is < 768px)
    mockUseBreakpoint.mockReturnValueOnce({
      xs: true,
      sm: true,
      md: false, // Mobile/tablet
      lg: false,
      xl: false,
      xxl: false,
    })

    const { container } = render(
      <TimelineProgress start={startDate} end={endDate} />,
    )

    const card = container.querySelector('.ant-card')
    expect(card).toHaveStyle({ width: '100%' })
  })

  it('applies custom styles', () => {
    const { container } = render(
      <TimelineProgress
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
      <TimelineProgress
        start={startDate}
        end={endDate}
        style={{ minWidth: '500px' }}
      />,
    )

    const card = container.querySelector('.ant-card')
    expect(card).toHaveStyle({ minWidth: '500px' })
  })
})
