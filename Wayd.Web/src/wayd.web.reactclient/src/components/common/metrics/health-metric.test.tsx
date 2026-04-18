import { render, screen } from '@testing-library/react'
import HealthMetric from './health-metric'

// Mock useTheme
jest.mock('../../contexts/theme', () => ({
  __esModule: true,
  default: () => ({
    token: {
      colorSuccess: '#52c41a',
      colorError: '#ff4d4f',
    },
  }),
}))

describe('HealthMetric', () => {
  it('renders health metric correctly', () => {
    render(<HealthMetric value={5} title="Issues" />)
    expect(screen.getByText('Issues')).toBeInTheDocument()
    expect(screen.getByText('5')).toBeInTheDocument()
  })
})
