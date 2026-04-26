import { render, screen } from '@testing-library/react'
import ChartCard from './chart-card'

describe('ChartCard', () => {
  it('renders title and children', () => {
    render(
      <ChartCard title="Objectives By Health">
        <div>Chart Content</div>
      </ChartCard>,
    )

    expect(screen.getByText('Objectives By Health')).toBeInTheDocument()
    expect(screen.getByText('Chart Content')).toBeInTheDocument()
  })

  it('applies default card style when not provided', () => {
    const { container } = render(
      <ChartCard title="Default">
        <div>Content</div>
      </ChartCard>,
    )

    const card = container.querySelector('.ant-card')
    expect(card).toHaveStyle({ height: '100%' })
  })

  it('renders without card wrapper in embedded mode', () => {
    const { container } = render(
      <ChartCard title="Embedded" embedded>
        <div>Content</div>
      </ChartCard>,
    )

    expect(screen.getByText('Embedded')).toBeInTheDocument()
    expect(container.querySelector('.ant-card')).not.toBeInTheDocument()
  })

  it('renders secondary value', () => {
    render(
      <ChartCard title="With Secondary" secondaryValue="Detail">
        <div>Content</div>
      </ChartCard>,
    )

    expect(screen.getByText('With Secondary')).toBeInTheDocument()
    expect(screen.getByText('Detail')).toBeInTheDocument()
  })
})
