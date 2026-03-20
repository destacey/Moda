import { render, screen } from '@testing-library/react'
import { userEvent } from '@testing-library/user-event'
import { ProjectPhaseListDto } from '@/src/services/moda-api'
import PhaseTimeline from './phase-timeline'

// Mock ResizeObserver for antd Steps component
global.ResizeObserver = class ResizeObserver {
  observe() {}
  unobserve() {}
  disconnect() {}
} as unknown as typeof ResizeObserver

let nextId = 0

function createPhase(
  overrides: Partial<ProjectPhaseListDto> & { name: string; order: number },
): ProjectPhaseListDto {
  return {
    id: `test-phase-${nextId++}`,
    status: { id: 1, name: 'Not Started' },
    start: undefined,
    end: undefined,
    progress: 0,
    ...overrides,
  }
}

describe('PhaseTimeline', () => {
  it('renders nothing when phases is empty', () => {
    const { container } = render(<PhaseTimeline phases={[]} />)
    expect(container).toBeEmptyDOMElement()
  })

  it('renders phase names', () => {
    const phases = [
      createPhase({ name: 'Discovery', order: 1 }),
      createPhase({ name: 'Development', order: 2 }),
      createPhase({ name: 'Launch', order: 3 }),
    ]

    render(<PhaseTimeline phases={phases} />)

    expect(screen.getByText('Discovery')).toBeInTheDocument()
    expect(screen.getByText('Development')).toBeInTheDocument()
    expect(screen.getByText('Launch')).toBeInTheDocument()
  })

  it('sorts phases by order', () => {
    const phases = [
      createPhase({ name: 'Launch', order: 3 }),
      createPhase({ name: 'Discovery', order: 1 }),
      createPhase({ name: 'Development', order: 2 }),
    ]

    const { container } = render(<PhaseTimeline phases={phases} />)

    const titles = container.querySelectorAll('.ant-steps-item-title')
    expect(titles[0]).toHaveTextContent('Discovery')
    expect(titles[1]).toHaveTextContent('Development')
    expect(titles[2]).toHaveTextContent('Launch')
  })

  it('renders completed phases with finish status', () => {
    const phases = [
      createPhase({
        name: 'Discovery',
        order: 1,
        status: { id: 3, name: 'Completed' },
      }),
    ]

    const { container } = render(<PhaseTimeline phases={phases} />)

    const step = container.querySelector('.ant-steps-item-finish')
    expect(step).toBeInTheDocument()
  })

  it('renders in-progress phases with process status', () => {
    const phases = [
      createPhase({
        name: 'Development',
        order: 1,
        status: { id: 2, name: 'In Progress' },
      }),
    ]

    const { container } = render(<PhaseTimeline phases={phases} />)

    const step = container.querySelector('.ant-steps-item-process')
    expect(step).toBeInTheDocument()
  })

  it('renders cancelled phases with error status', () => {
    const phases = [
      createPhase({
        name: 'Cancelled Phase',
        order: 1,
        status: { id: 4, name: 'Cancelled' },
      }),
    ]

    const { container } = render(<PhaseTimeline phases={phases} />)

    const step = container.querySelector('.ant-steps-item-error')
    expect(step).toBeInTheDocument()
  })

  it('renders not-started phases with wait status', () => {
    const phases = [
      createPhase({
        name: 'Future Phase',
        order: 1,
        status: { id: 1, name: 'Not Started' },
      }),
    ]

    const { container } = render(<PhaseTimeline phases={phases} />)

    const step = container.querySelector('.ant-steps-item-wait')
    expect(step).toBeInTheDocument()
  })

  it('handles mixed statuses', () => {
    const phases = [
      createPhase({
        name: 'Done',
        order: 1,
        status: { id: 3, name: 'Completed' },
      }),
      createPhase({
        name: 'Active',
        order: 2,
        status: { id: 2, name: 'In Progress' },
      }),
      createPhase({
        name: 'Upcoming',
        order: 3,
        status: { id: 1, name: 'Not Started' },
      }),
    ]

    const { container } = render(<PhaseTimeline phases={phases} />)

    expect(container.querySelector('.ant-steps-item-finish')).toBeInTheDocument()
    expect(container.querySelector('.ant-steps-item-process')).toBeInTheDocument()
    expect(container.querySelector('.ant-steps-item-wait')).toBeInTheDocument()
  })

  it('shows tooltip with status on hover', async () => {
    const phases = [
      createPhase({
        name: 'Discovery',
        order: 1,
        status: { id: 2, name: 'In Progress' },
        start: new Date('2026-01-15T12:00:00'),
        end: new Date('2026-03-15T12:00:00'),
        progress: 45,
      }),
    ]

    render(<PhaseTimeline phases={phases} />)

    await userEvent.hover(screen.getByText('Discovery'))

    expect(await screen.findByText('In Progress')).toBeInTheDocument()
  })

  it('shows dates inline below phase name', () => {
    const phases = [
      createPhase({
        name: 'Discovery',
        order: 1,
        status: { id: 2, name: 'In Progress' },
        start: new Date('2026-01-15T12:00:00'),
        end: new Date('2026-03-15T12:00:00'),
      }),
    ]

    render(<PhaseTimeline phases={phases} />)

    expect(screen.getByText('Jan 15 - Mar 15, 2026')).toBeInTheDocument()
  })

  it('shows progress inline below phase name', () => {
    const phases = [
      createPhase({
        name: 'Discovery',
        order: 1,
        status: { id: 2, name: 'In Progress' },
        progress: 45,
      }),
    ]

    render(<PhaseTimeline phases={phases} />)

    expect(screen.getByText('45%')).toBeInTheDocument()
  })

  it('does not show dates when dates are not set', () => {
    const phases = [
      createPhase({
        name: 'Discovery',
        order: 1,
        status: { id: 1, name: 'Not Started' },
      }),
    ]

    render(<PhaseTimeline phases={phases} />)

    expect(screen.queryByText(/Jan|Feb|Mar/)).not.toBeInTheDocument()
  })

  it('shows start-only date inline', () => {
    const phases = [
      createPhase({
        name: 'Discovery',
        order: 1,
        status: { id: 2, name: 'In Progress' },
        start: new Date('2026-02-01T12:00:00'),
      }),
    ]

    render(<PhaseTimeline phases={phases} />)

    expect(screen.getByText('Starts Feb 1, 2026')).toBeInTheDocument()
  })

  it('shows end-only date inline', () => {
    const phases = [
      createPhase({
        name: 'Discovery',
        order: 1,
        status: { id: 2, name: 'In Progress' },
        end: new Date('2026-06-30T12:00:00'),
      }),
    ]

    render(<PhaseTimeline phases={phases} />)

    expect(screen.getByText('Ends Jun 30, 2026')).toBeInTheDocument()
  })

  it('does not mutate the original phases array', () => {
    const phases = [
      createPhase({ name: 'B', order: 2 }),
      createPhase({ name: 'A', order: 1 }),
    ]
    const original = [...phases]

    render(<PhaseTimeline phases={phases} />)

    expect(phases[0].name).toBe(original[0].name)
    expect(phases[1].name).toBe(original[1].name)
  })
})
