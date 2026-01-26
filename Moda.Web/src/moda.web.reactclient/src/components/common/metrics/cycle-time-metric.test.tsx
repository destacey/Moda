import { render, screen } from '@testing-library/react'
import CycleTimeMetric from './cycle-time-metric'

describe('CycleTimeMetric', () => {
  it('renders cycle time correctly', () => {
    render(<CycleTimeMetric value={5.123} />)

    expect(screen.getByText('Avg Cycle Time')).toBeInTheDocument()
    // Ant Design Statistic splits integer and decimal parts
    expect(screen.getByText('5')).toBeInTheDocument()
    expect(screen.getByText('.12')).toBeInTheDocument()
    expect(screen.getByText('days')).toBeInTheDocument()
  })

  it('renders custom title', () => {
    render(<CycleTimeMetric value={10} title="My Cycle Time" />)
    expect(screen.getByText('My Cycle Time')).toBeInTheDocument()
  })
})
