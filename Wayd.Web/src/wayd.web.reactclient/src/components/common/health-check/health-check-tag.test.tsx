import { render, screen } from '@testing-library/react'
import HealthCheckTag, {
  HealthCheckDetailsData,
  HealthCheckStatusTagData,
} from './health-check-tag'

jest.mock('../markdown', () => ({
  MarkdownRenderer: ({ markdown }: { markdown?: string }) => (
    <div data-testid="markdown-renderer">{markdown}</div>
  ),
}))

jest.mock('antd', () => {
  const actual = jest.requireActual('antd')

  return {
    ...actual,
    Popover: ({
      children,
      content,
      onOpenChange,
    }: {
      children: any
      content: any
      onOpenChange?: (open: boolean) => void
    }) => (
      <div>
        <div onMouseEnter={() => onOpenChange?.(true)}>{children}</div>
        <div>{typeof content === 'function' ? content() : content}</div>
      </div>
    ),
  }
})

describe('HealthCheckTag', () => {
  const healthCheck: HealthCheckStatusTagData = {
    status: { name: 'Healthy' },
    expiration: new Date(2026, 0, 15, 14, 30),
  }

  it('should return null when health check is not provided', () => {
    const { container } = render(<HealthCheckTag healthCheck={null} />)
    expect(container.firstChild).toBeNull()
  })

  it('should render status tag with mapped color', () => {
    const { container } = render(<HealthCheckTag healthCheck={healthCheck} />)
    expect(screen.getByText('Healthy')).toBeInTheDocument()
    expect(container.querySelector('.ant-tag-success')).toBeInTheDocument()
  })

  it('should render summary content when details are not provided', () => {
    render(<HealthCheckTag healthCheck={healthCheck} />)
    expect(screen.getByText('Expires On')).toBeInTheDocument()
    expect(screen.getByText(/2026-01-15/)).toBeInTheDocument()
  })

  it('should render loading indicator when loading details', () => {
    const { container } = render(
      <HealthCheckTag healthCheck={healthCheck} isLoading />,
    )
    expect(container.querySelector('.ant-spin')).toBeInTheDocument()
  })

  it('should render detailed content when details are provided', () => {
    const details: HealthCheckDetailsData = {
      ...healthCheck,
      reportedBy: { name: 'Jane Smith' },
      reportedOn: new Date(2026, 0, 10, 9, 0),
      note: 'Project remains on track',
    }

    render(<HealthCheckTag healthCheck={healthCheck} details={details} />)

    expect(screen.getByText('Reported By')).toBeInTheDocument()
    expect(screen.getByText('Jane Smith')).toBeInTheDocument()
    expect(screen.getByText('Reported On')).toBeInTheDocument()
    expect(screen.getByText(/1\/10\/2026/)).toBeInTheDocument()
    expect(screen.getByText('Expires On')).toBeInTheDocument()
    expect(screen.getByTestId('markdown-renderer')).toHaveTextContent(
      'Project remains on track',
    )
  })
})
