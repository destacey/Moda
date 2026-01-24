import { render, screen } from '@testing-library/react'
import StatusMetric from './status-metric'

describe('StatusMetric', () => {
  it('renders status metric correctly', () => {
    render(
      <StatusMetric
        value={25}
        total={100}
        title="Active Items"
        tooltip="Test Tooltip"
      />,
    )

    expect(screen.getByText('Active Items')).toBeInTheDocument()
    expect(screen.getByText('25')).toBeInTheDocument()
    expect(screen.getByText('25.0%')).toBeInTheDocument()
  })

  it('renders 0% when total is 0', () => {
    render(<StatusMetric value={0} total={0} title="Empty" />)
    expect(screen.getByText('0%')).toBeInTheDocument()
  })
})
