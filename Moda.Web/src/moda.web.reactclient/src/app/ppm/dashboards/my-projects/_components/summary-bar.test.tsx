import { render, screen, waitFor, act } from '@testing-library/react'
import MyProjectsSummaryBar from './summary-bar'
import { ProjectListDto } from '@/src/services/moda-api'

global.ResizeObserver = class {
  observe() {}
  unobserve() {}
  disconnect() {}
} as unknown as typeof ResizeObserver

jest.mock('@/src/components/contexts/theme', () => ({
  __esModule: true,
  default: () => ({
    token: {
      colorError: '#ff4d4f',
      colorWarning: '#faad14',
    },
  }),
}))

const mockGetProjectPlanSummary = jest.fn()

jest.mock('@/src/services/clients', () => ({
  getProjectsClient: jest.fn(() => ({
    getProjectPlanSummary: mockGetProjectPlanSummary,
  })),
}))

function createProject(key: string): ProjectListDto {
  return {
    id: `id-${key}`,
    key,
    name: `Project ${key}`,
    status: { name: 'Active', lifecyclePhase: 'Active' } as any,
    portfolio: { id: 'port-1', key: 1, name: 'Portfolio' } as any,
    projectSponsors: [],
    projectOwners: [],
    projectManagers: [],
    projectMembers: [],
    strategicThemes: [],
    phases: [],
  } as ProjectListDto
}

describe('MyProjectsSummaryBar', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('renders loading skeleton when isLoading is true', () => {
    const { container } = render(
      <MyProjectsSummaryBar projects={undefined} isLoading={true} />,
    )

    expect(container.querySelector('.ant-skeleton')).toBeInTheDocument()
  })

  it('renders project count', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 0,
      dueThisWeek: 0,
      upcoming: 0,
      totalLeafTasks: 0,
    })

    await act(async () => {
      render(
        <MyProjectsSummaryBar
          projects={[createProject('P1'), createProject('P2')]}
          isLoading={false}
        />,
      )
    })

    expect(screen.getByText('Projects')).toBeInTheDocument()
    expect(screen.getByText('2')).toBeInTheDocument()
  })

  it('renders zero project count when no projects', async () => {
    await act(async () => {
      render(
        <MyProjectsSummaryBar projects={[]} isLoading={false} />,
      )
    })

    expect(screen.getByText('Projects')).toBeInTheDocument()
    // Statistic renders value in content span
    const statValues = document.querySelectorAll(
      '.ant-statistic-content-value',
    )
    const projectValue = Array.from(statValues).find(
      (el) => el.textContent === '0',
    )
    expect(projectValue).toBeTruthy()
  })

  it('aggregates overdue across projects', async () => {
    mockGetProjectPlanSummary
      .mockResolvedValueOnce({
        overdue: 3,
        dueThisWeek: 0,
        upcoming: 0,
        totalLeafTasks: 5,
      })
      .mockResolvedValueOnce({
        overdue: 2,
        dueThisWeek: 0,
        upcoming: 0,
        totalLeafTasks: 5,
      })

    await act(async () => {
      render(
        <MyProjectsSummaryBar
          projects={[createProject('P1'), createProject('P2')]}
          isLoading={false}
        />,
      )
    })

    expect(screen.getByText('5')).toBeInTheDocument() // 3 + 2
  })

  it('aggregates dueThisWeek across projects', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 0,
      dueThisWeek: 4,
      upcoming: 0,
      totalLeafTasks: 5,
    })

    await act(async () => {
      render(
        <MyProjectsSummaryBar
          projects={[createProject('P1')]}
          isLoading={false}
        />,
      )
    })

    expect(screen.getByText('4')).toBeInTheDocument()
  })

  it('aggregates upcoming across projects', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 0,
      dueThisWeek: 0,
      upcoming: 7,
      totalLeafTasks: 10,
    })

    await act(async () => {
      render(
        <MyProjectsSummaryBar
          projects={[createProject('P1')]}
          isLoading={false}
        />,
      )
    })

    expect(screen.getByText('7')).toBeInTheDocument()
  })

  it('handles API failures gracefully', async () => {
    mockGetProjectPlanSummary.mockRejectedValue(new Error('fail'))

    await act(async () => {
      render(
        <MyProjectsSummaryBar
          projects={[createProject('P1')]}
          isLoading={false}
        />,
      )
    })

    // Should still render without crashing
    expect(screen.getByText('Projects')).toBeInTheDocument()
  })

  it('renders metric titles', async () => {
    mockGetProjectPlanSummary.mockResolvedValue({
      overdue: 0,
      dueThisWeek: 0,
      upcoming: 0,
      totalLeafTasks: 0,
    })

    await act(async () => {
      render(
        <MyProjectsSummaryBar
          projects={[createProject('P1')]}
          isLoading={false}
        />,
      )
    })

    expect(screen.getByText('Projects')).toBeInTheDocument()
    expect(screen.getByText('Overdue')).toBeInTheDocument()
    expect(screen.getByText('Due This Week')).toBeInTheDocument()
    expect(screen.getByText('Upcoming')).toBeInTheDocument()
  })
})
