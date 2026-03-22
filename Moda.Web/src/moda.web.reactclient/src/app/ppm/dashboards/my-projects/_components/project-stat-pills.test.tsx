import { render, screen, waitFor, act } from '@testing-library/react'
import ProjectStatPills from './project-stat-pills'

global.ResizeObserver = class {
  observe() {}
  unobserve() {}
  disconnect() {}
} as unknown as typeof ResizeObserver

const mockGetProjectPlanSummary = jest.fn()

jest.mock('@/src/services/clients', () => ({
  getProjectsClient: jest.fn(() => ({
    getProjectPlanSummary: mockGetProjectPlanSummary,
  })),
}))

describe('ProjectStatPills', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('renders nothing while loading', () => {
    mockGetProjectPlanSummary.mockReturnValue(new Promise(() => {}))

    const { container } = render(<ProjectStatPills projectKey="P1" />)

    expect(container).toBeEmptyDOMElement()
  })

  it('renders overdue pill when overdue > 0', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 3,
      dueThisWeek: 0,
      upcoming: 0,
      totalLeafTasks: 10,
    })

    await act(async () => {
      render(<ProjectStatPills projectKey="P1" />)
    })

    expect(screen.getByText('3 overdue')).toBeInTheDocument()
  })

  it('renders due this week pill when dueThisWeek > 0', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 0,
      dueThisWeek: 5,
      upcoming: 0,
      totalLeafTasks: 10,
    })

    await act(async () => {
      render(<ProjectStatPills projectKey="P1" />)
    })

    expect(screen.getByText('5 this week')).toBeInTheDocument()
  })

  it('renders upcoming pill when upcoming > 0', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 0,
      dueThisWeek: 0,
      upcoming: 2,
      totalLeafTasks: 10,
    })

    await act(async () => {
      render(<ProjectStatPills projectKey="P1" />)
    })

    expect(screen.getByText('2 upcoming')).toBeInTheDocument()
  })

  it('renders all pills when all counts > 0', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 1,
      dueThisWeek: 2,
      upcoming: 3,
      totalLeafTasks: 10,
    })

    await act(async () => {
      render(<ProjectStatPills projectKey="P1" />)
    })

    expect(screen.getByText('1 overdue')).toBeInTheDocument()
    expect(screen.getByText('2 this week')).toBeInTheDocument()
    expect(screen.getByText('3 upcoming')).toBeInTheDocument()
  })

  it('renders nothing when all counts are 0', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 0,
      dueThisWeek: 0,
      upcoming: 0,
      totalLeafTasks: 10,
    })

    const { container } = render(<ProjectStatPills projectKey="P1" />)

    await waitFor(() => {
      expect(mockGetProjectPlanSummary).toHaveBeenCalledWith('P1', undefined)
    })

    expect(container).toBeEmptyDOMElement()
  })

  it('renders nothing when API call fails', async () => {
    mockGetProjectPlanSummary.mockRejectedValue(new Error('fail'))

    const { container } = render(<ProjectStatPills projectKey="P1" />)

    await waitFor(() => {
      expect(mockGetProjectPlanSummary).toHaveBeenCalled()
    })

    expect(container).toBeEmptyDOMElement()
  })

  it('passes employeeId to API when provided', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 1,
      dueThisWeek: 0,
      upcoming: 0,
      totalLeafTasks: 5,
    })

    await act(async () => {
      render(<ProjectStatPills projectKey="P1" employeeId="emp-123" />)
    })

    expect(mockGetProjectPlanSummary).toHaveBeenCalledWith('P1', 'emp-123')
  })

  it('does not pass employeeId when not provided', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 1,
      dueThisWeek: 0,
      upcoming: 0,
      totalLeafTasks: 5,
    })

    await act(async () => {
      render(<ProjectStatPills projectKey="P1" />)
    })

    expect(mockGetProjectPlanSummary).toHaveBeenCalledWith('P1', undefined)
  })

  it('refetches when employeeId changes', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 2,
      dueThisWeek: 0,
      upcoming: 0,
      totalLeafTasks: 5,
    })

    const { rerender } = render(<ProjectStatPills projectKey="P1" />)

    await waitFor(() => {
      expect(mockGetProjectPlanSummary).toHaveBeenCalledWith('P1', undefined)
    })

    mockGetProjectPlanSummary.mockClear()
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 1,
      dueThisWeek: 0,
      upcoming: 0,
      totalLeafTasks: 5,
    })

    await act(async () => {
      rerender(<ProjectStatPills projectKey="P1" employeeId="emp-456" />)
    })

    expect(mockGetProjectPlanSummary).toHaveBeenCalledWith('P1', 'emp-456')
  })
})
