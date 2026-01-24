import { render, screen } from '@testing-library/react'
import CompletionRateMetric from './completion-rate-metric'

// Mock useTheme
jest.mock('../../contexts/theme', () => ({
  __esModule: true,
  default: () => ({
    token: {
      colorSuccess: '#52c41a',
    },
  }),
}))

describe('CompletionRateMetric', () => {
  it('renders completion rate correctly', () => {
    render(<CompletionRateMetric completed={50} total={100} />)

    expect(screen.getByText('Completion Rate')).toBeInTheDocument()
    // Ant Design Statistic splits integer and decimal parts
    expect(screen.getByText('50')).toBeInTheDocument()
    expect(screen.getByText('.0')).toBeInTheDocument()
    expect(screen.getByText('%')).toBeInTheDocument()
  })

  it('renders 0% when total is 0', () => {
    render(<CompletionRateMetric completed={0} total={0} />)

    expect(screen.getByText('0')).toBeInTheDocument()
    expect(screen.getByText('.0')).toBeInTheDocument()
  })

  it('calculates percentage correctly', () => {
    render(<CompletionRateMetric completed={1} total={3} />)
    // 1/3 = 33.333... -> 33.3
    expect(screen.getByText('33')).toBeInTheDocument()
    expect(screen.getByText('.3')).toBeInTheDocument()
  })
})
