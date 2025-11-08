import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import SprintMetrics from './sprint-metrics'
import { IterationState, WorkStatusCategory } from '@/src/components/types'
import { SprintBacklogItemDto, SprintDetailsDto } from '@/src/services/moda-api'

// Mock the MetricCard and DaysCountdownMetric components
jest.mock('../../../../components/common/metrics', () => ({
  MetricCard: ({
    title,
    value,
    precision,
    suffix,
    secondaryValue,
  }: {
    title: string
    value: number | null
    precision?: number
    suffix?: string
    secondaryValue?: string | number
  }) => (
    <div data-testid={`metric-${title}`}>
      <span>{title}</span>
      <span data-testid={`value-${title}`}>
        {value !== null && value !== undefined
          ? precision !== undefined
            ? value.toFixed(precision)
            : value
          : 'N/A'}
      </span>
      {suffix && <span>{suffix}</span>}
      {secondaryValue !== undefined && (
        <span data-testid={`secondary-${title}`}>{secondaryValue}</span>
      )}
    </div>
  ),
  DaysCountdownMetric: ({ state }: { state: number }) => (
    <div data-testid="countdown-metric">
      <span>State: {state}</span>
    </div>
  ),
}))

describe('SprintMetrics', () => {
  const mockSprint: SprintDetailsDto = {
    id: 'sprint-1',
    key: 1,
    name: 'Sprint 1',
    start: new Date('2025-01-01'),
    end: new Date('2025-01-14'),
    state: { id: IterationState.Active, name: 'Active' },
    team: {
      id: 'team-1',
      key: 1,
      name: 'Team 1',
    },
  }

  const createBacklogItem = (
    overrides: Partial<SprintBacklogItemDto> = {},
  ): SprintBacklogItemDto => ({
    id: 'item-1',
    key: 'WI-1',
    title: 'Test Item',
    workspace: { id: '1', key: 'TEST', name: 'Workspace 1' },
    type: 'User Story',
    status: 'New',
    statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
    parent: undefined,
    team: undefined,
    sprint: { id: '105', key: 1, name: 'Sprint 25.4.2' },
    assignedTo: undefined,
    created: new Date('2025-01-01'),
    activated: undefined,
    done: undefined,
    rank: 1,
    parentRank: undefined,
    project: undefined,
    externalViewWorkItemUrl: undefined,
    stackRank: 1.0,
    storyPoints: undefined,
    cycleTime: undefined,
    ...overrides,
  })

  describe('Story Points mode', () => {
    it('renders all metrics with story points by default', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 2,
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('metric-Completion Rate')).toBeInTheDocument()
      expect(screen.getByTestId('value-Completion Rate')).toHaveTextContent(
        '50.0',
      ) // 5/10 * 100
      expect(screen.getByTestId('metric-Total')).toBeInTheDocument()
      expect(screen.getByTestId('value-Total')).toHaveTextContent('10') // 5+3+2
      expect(screen.getByTestId('metric-Velocity')).toBeInTheDocument()
      expect(screen.getByTestId('value-Velocity')).toHaveTextContent('5')
      expect(screen.getByTestId('metric-In Progress')).toBeInTheDocument()
      expect(screen.getByTestId('value-In Progress')).toHaveTextContent('3')
      expect(screen.getByTestId('metric-Not Started')).toBeInTheDocument()
      expect(screen.getByTestId('value-Not Started')).toHaveTextContent('2')
    })

    it('calculates completion rate correctly with story points', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 8,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 2,
          statusCategory: { id: WorkStatusCategory.Removed, name: 'Removed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // (8 + 2) / (8 + 5 + 2) * 100 = 66.67%
      expect(screen.getByTestId('value-Completion Rate')).toHaveTextContent(
        '66.7',
      )
    })

    it('handles items without story points', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: null,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Total')).toHaveTextContent('5')
      expect(screen.getByTestId('value-Velocity')).toHaveTextContent('0')
      expect(screen.getByTestId('value-In Progress')).toHaveTextContent('5')
    })
  })

  describe('Count mode', () => {
    it('switches to count mode when segmented control is clicked', async () => {
      const user = userEvent.setup()
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 2,
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      const countOption = screen.getByText('Count')
      await user.click(countOption)

      expect(screen.getByTestId('value-Total')).toHaveTextContent('3') // 3 items
      expect(screen.getByTestId('value-Velocity')).toHaveTextContent('1')
      expect(screen.getByTestId('value-In Progress')).toHaveTextContent('1')
      expect(screen.getByTestId('value-Not Started')).toHaveTextContent('1')
      expect(screen.getByTestId('value-Completion Rate')).toHaveTextContent(
        '33.3',
      ) // 1/3 * 100
    })

    it('switches back to story points mode', async () => {
      const user = userEvent.setup()
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // Switch to count mode
      await user.click(screen.getByText('Count'))
      expect(screen.getByTestId('value-Total')).toHaveTextContent('1')

      // Switch back to story points
      await user.click(screen.getByText('Story Points'))
      expect(screen.getByTestId('value-Total')).toHaveTextContent('5')
    })
  })

  describe('Average Cycle Time', () => {
    it('calculates average cycle time for completed items', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          activated: new Date('2025-01-01'),
          done: new Date('2025-01-06'),
          cycleTime: 5.0, // 5 days
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          activated: new Date('2025-01-02'),
          done: new Date('2025-01-05'),
          cycleTime: 3.0, // 3 days
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // Average: (5 + 3) / 2 = 4
      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        '4.00',
      )
    })

    it('calculates average cycle time including removed items', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: 6.0,
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Removed, name: 'Removed' },
          cycleTime: 4.0,
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // Average: (6 + 4) / 2 = 5
      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        '5.00',
      )
    })

    it('excludes items without cycle time from average', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: 5.0,
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: null, // No cycle time
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: 3.0,
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // Average: (5 + 3) / 2 = 4 (excludes the null)
      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        '4.00',
      )
    })

    it('excludes in-progress items from average cycle time', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: 5.0,
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
          cycleTime: null, // Active items don't have cycle time
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // Average: 5 / 1 = 5 (only the Done item)
      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        '5.00',
      )
    })

    it('displays N/A when no items have cycle time', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
          cycleTime: null,
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
          cycleTime: null,
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        'N/A',
      )
    })

    it('displays N/A when backlog is empty', () => {
      render(<SprintMetrics sprint={mockSprint} backlog={[]} />)

      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        'N/A',
      )
    })

    it('handles fractional cycle times correctly', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: 2.5,
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: 3.7,
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // Average: (2.5 + 3.7) / 2 = 3.1
      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        '3.10',
      )
    })
  })

  describe('Status category breakdown', () => {
    it('counts items in each status category correctly', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 2,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 1,
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Velocity')).toHaveTextContent('8') // 5+3
      expect(screen.getByTestId('value-In Progress')).toHaveTextContent('2')
      expect(screen.getByTestId('value-Not Started')).toHaveTextContent('1')
    })

    it('treats removed items as completed', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Removed, name: 'Removed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Velocity')).toHaveTextContent('8') // 5+3
    })
  })

  describe('Edge cases', () => {
    it('handles empty backlog', () => {
      render(<SprintMetrics sprint={mockSprint} backlog={[]} />)

      expect(screen.getByTestId('value-Total')).toHaveTextContent('0')
      expect(screen.getByTestId('value-Velocity')).toHaveTextContent('0')
      expect(screen.getByTestId('value-In Progress')).toHaveTextContent('0')
      expect(screen.getByTestId('value-Not Started')).toHaveTextContent('0')
      expect(screen.getByTestId('value-Completion Rate')).toHaveTextContent(
        '0.0',
      )
    })

    it('handles backlog with all items having zero story points', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 0,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 0,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Total')).toHaveTextContent('0')
      expect(screen.getByTestId('value-Completion Rate')).toHaveTextContent(
        '0.0',
      )
    })

    it('handles 100% completion rate', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Removed, name: 'Removed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Completion Rate')).toHaveTextContent(
        '100.0',
      )
    })

    it('handles 0% completion rate', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Completion Rate')).toHaveTextContent(
        '0.0',
      )
    })
  })

  describe('Countdown metric', () => {
    it('renders countdown metric with active sprint state', () => {
      render(<SprintMetrics sprint={mockSprint} backlog={[]} />)

      expect(screen.getByTestId('countdown-metric')).toBeInTheDocument()
      expect(screen.getByText(`State: ${IterationState.Active}`))
    })

    it('renders countdown metric for future sprint', () => {
      const futureSprint = {
        ...mockSprint,
        state: { id: IterationState.Future, name: 'Future' },
      }

      render(<SprintMetrics sprint={futureSprint} backlog={[]} />)

      expect(screen.getByTestId('countdown-metric')).toBeInTheDocument()
      expect(screen.getByText(`State: ${IterationState.Future}`))
    })

    it('does not render countdown metric for completed sprint', () => {
      const completedSprint = {
        ...mockSprint,
        state: { id: IterationState.Completed, name: 'Completed' },
      }

      render(<SprintMetrics sprint={completedSprint} backlog={[]} />)

      expect(screen.queryByTestId('countdown-metric')).not.toBeInTheDocument()
    })
  })

  describe('Performance', () => {
    it('recalculates metrics when backlog changes', () => {
      const initialBacklog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
      ]

      const { rerender } = render(
        <SprintMetrics sprint={mockSprint} backlog={initialBacklog} />,
      )

      expect(screen.getByTestId('value-Total')).toHaveTextContent('5')

      const updatedBacklog: SprintBacklogItemDto[] = [
        ...initialBacklog,
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
      ]

      rerender(<SprintMetrics sprint={mockSprint} backlog={updatedBacklog} />)

      expect(screen.getByTestId('value-Total')).toHaveTextContent('8')
    })

    it('recalculates average cycle time when backlog changes', () => {
      const initialBacklog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: 5.0,
        }),
      ]

      const { rerender } = render(
        <SprintMetrics sprint={mockSprint} backlog={initialBacklog} />,
      )

      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        '5.00',
      )

      const updatedBacklog: SprintBacklogItemDto[] = [
        ...initialBacklog,
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
          cycleTime: 3.0,
        }),
      ]

      rerender(<SprintMetrics sprint={mockSprint} backlog={updatedBacklog} />)

      expect(screen.getByTestId('value-Avg Cycle Time')).toHaveTextContent(
        '4.00',
      )
    })
  })

  describe('WIP (Work In Process)', () => {
    it('displays WIP metric when sprint is active', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('metric-WIP')).toBeInTheDocument()
      expect(screen.getByTestId('value-WIP')).toHaveTextContent('2')
    })

    it('does not display WIP metric when sprint is future', () => {
      const futureSprint = {
        ...mockSprint,
        state: { id: IterationState.Future, name: 'Future' },
      }
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
      ]

      render(<SprintMetrics sprint={futureSprint} backlog={backlog} />)

      expect(screen.queryByTestId('metric-WIP')).not.toBeInTheDocument()
    })

    it('does not display WIP metric when sprint is completed', () => {
      const completedSprint = {
        ...mockSprint,
        state: { id: IterationState.Completed, name: 'Completed' },
      }
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
      ]

      render(<SprintMetrics sprint={completedSprint} backlog={backlog} />)

      expect(screen.queryByTestId('metric-WIP')).not.toBeInTheDocument()
    })

    it('displays 0 when no items are in progress', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-WIP')).toHaveTextContent('0')
    })

    it('counts only active items regardless of story points', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: undefined,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-WIP')).toHaveTextContent('2')
    })
  })

  describe('Secondary percentage values', () => {
    it('displays percentage values for velocity, in progress, and not started in story points mode', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 2,
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // Total is 10 (5+3+2)
      expect(screen.getByTestId('secondary-Velocity')).toHaveTextContent('50.0%') // 5/10 * 100
      expect(screen.getByTestId('secondary-In Progress')).toHaveTextContent(
        '30.0%',
      ) // 3/10 * 100
      expect(screen.getByTestId('secondary-Not Started')).toHaveTextContent(
        '20.0%',
      ) // 2/10 * 100
    })

    it('displays percentage values in count mode', async () => {
      const user = userEvent.setup()
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 8,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 2,
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      await user.click(screen.getByText('Count'))

      // Total is 3 items
      expect(screen.getByTestId('secondary-Velocity')).toHaveTextContent('33.3%') // 1/3 * 100
      expect(screen.getByTestId('secondary-In Progress')).toHaveTextContent(
        '33.3%',
      ) // 1/3 * 100
      expect(screen.getByTestId('secondary-Not Started')).toHaveTextContent(
        '33.3%',
      ) // 1/3 * 100
    })

    it('displays WIP percentage when sprint is active', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 2,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      // WIP shows count (2), percentage based on total story points (10)
      expect(screen.getByTestId('value-WIP')).toHaveTextContent('2')
      expect(screen.getByTestId('secondary-WIP')).toHaveTextContent('20.0%') // 2/10 * 100
    })

    it('handles 100% completion with correct percentages', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Removed, name: 'Removed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('secondary-Velocity')).toHaveTextContent(
        '100.0%',
      )
      expect(screen.getByTestId('secondary-In Progress')).toHaveTextContent(
        '0.0%',
      )
      expect(screen.getByTestId('secondary-Not Started')).toHaveTextContent(
        '0.0%',
      )
    })

    it('displays 0% for all metrics when backlog is empty', () => {
      render(<SprintMetrics sprint={mockSprint} backlog={[]} />)

      expect(screen.getByTestId('secondary-Velocity')).toHaveTextContent('0%')
      expect(screen.getByTestId('secondary-In Progress')).toHaveTextContent(
        '0%',
      )
      expect(screen.getByTestId('secondary-Not Started')).toHaveTextContent(
        '0%',
      )
    })
  })

  describe('Missing Story Points', () => {
    it('displays missing story points metric in story points mode', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: undefined,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: null,
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('metric-Missing SPs')).toBeInTheDocument()
      expect(screen.getByTestId('value-Missing SPs')).toHaveTextContent('2')
    })

    it('hides missing story points metric in count mode', async () => {
      const user = userEvent.setup()
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: undefined,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('metric-Missing SPs')).toBeInTheDocument()

      await user.click(screen.getByText('Count'))

      expect(screen.queryByTestId('metric-Missing SPs')).not.toBeInTheDocument()
    })

    it('counts items with zero story points as missing', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 0,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Missing SPs')).toHaveTextContent('1')
    })

    it('displays 0 when all items have story points', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: 5,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: 3,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Missing SPs')).toHaveTextContent('0')
    })

    it('displays total count when no items have story points', () => {
      const backlog: SprintBacklogItemDto[] = [
        createBacklogItem({
          storyPoints: undefined,
          statusCategory: { id: WorkStatusCategory.Done, name: 'Done' },
        }),
        createBacklogItem({
          storyPoints: null,
          statusCategory: { id: WorkStatusCategory.Active, name: 'Active' },
        }),
        createBacklogItem({
          storyPoints: 0,
          statusCategory: { id: WorkStatusCategory.Proposed, name: 'Proposed' },
        }),
      ]

      render(<SprintMetrics sprint={mockSprint} backlog={backlog} />)

      expect(screen.getByTestId('value-Missing SPs')).toHaveTextContent('3')
    })
  })
})

