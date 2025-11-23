import { render, screen } from '@testing-library/react'
import '@testing-library/jest-dom'
import { CustomCellRendererProps } from 'ag-grid-react'
import {
  AssignedToLinkCellRenderer,
  DateTimeCellRenderer,
  DependencyHealthCellRenderer,
  MarkdownCellRenderer,
  NestedTeamNameLinkCellRenderer,
  NestedWorkSprintLinkCellRenderer,
  ParentWorkItemLinkCellRenderer,
  PlanningIntervalLinkCellRenderer,
  PortfolioLinkCellRenderer,
  ProgramLinkCellRenderer,
  ProjectLinkCellRenderer,
  TeamNameLinkCellRenderer,
  WorkItemLinkCellRenderer,
  WorkSprintLinkCellRenderer,
  WorkStatusTagCellRenderer,
  WorkspaceLinkCellRenderer,
  renderSprintLinkHelper,
  renderTeamLinkHelper,
  renderWorkItemLinkHelper,
} from './moda-grid-cell-renderers'
import { DependencyHealth, WorkStatusCategory } from '../types'

// Mock Next.js Link component
jest.mock('next/link', () => {
  const MockLink = ({ children, href, target, ...props }: any) => {
    return <a href={href} target={target} {...props}>{children}</a>
  }
  MockLink.displayName = 'MockLink'
  return MockLink
})

// Helper to create mock cell renderer props
const createMockProps = <T,>(data: T | null, value?: any): CustomCellRendererProps<T> => {
  return {
    data,
    value: value ?? null,
    valueFormatted: null,
    getValue: () => value,
    setValue: () => {},
    formatValue: () => value,
    node: {} as any,
    colDef: {},
    column: {} as any,
    api: {} as any,
    columnApi: {} as any,
    context: {},
    refreshCell: () => {},
    eGridCell: {} as any,
    eParentOfValue: {} as any,
    registerRowDragger: () => {},
  }
}

describe('Work Item Cell Renderers', () => {
  describe('WorkItemLinkCellRenderer', () => {
    it('should render work item link with key', () => {
      const data = {
        key: 'WI-123',
        workspace: { key: 'workspace-1' },
      }
      const props = createMockProps(data, 'WI-123')

      render(<>{WorkItemLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'WI-123' })
      expect(link).toBeInTheDocument()
      expect(link).toHaveAttribute('href', '/work/workspaces/workspace-1/work-items/WI-123')
    })

    it('should render external link icon when externalViewWorkItemUrl is present', () => {
      const data = {
        key: 'WI-123',
        workspace: { key: 'workspace-1' },
        externalViewWorkItemUrl: 'https://external.com/wi/123',
      }
      const props = createMockProps(data, 'WI-123')

      render(<>{WorkItemLinkCellRenderer(props)}</>)

      const links = screen.getAllByRole('link')
      expect(links).toHaveLength(2)
      expect(links[1]).toHaveAttribute('href', 'https://external.com/wi/123')
      expect(links[1]).toHaveAttribute('target', '_blank')
    })

    it('should return null when data is null', () => {
      const props = createMockProps(null)
      const result = WorkItemLinkCellRenderer(props)
      expect(result).toBeNull()
    })
  })

  describe('ParentWorkItemLinkCellRenderer', () => {
    it('should render parent work item link', () => {
      const data = {
        parent: {
          key: 'WI-100',
          workspaceKey: 'workspace-1',
        },
      }
      const props = createMockProps(data, 'WI-100')

      render(<>{ParentWorkItemLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'WI-100' })
      expect(link).toBeInTheDocument()
      expect(link).toHaveAttribute('href', '/work/workspaces/workspace-1/work-items/WI-100')
    })

    it('should return null when parent is null', () => {
      const data = { parent: null }
      const props = createMockProps(data, 'WI-100')
      const result = ParentWorkItemLinkCellRenderer(props)
      expect(result).toBeNull()
    })
  })

  describe('AssignedToLinkCellRenderer', () => {
    it('should render assigned employee link', () => {
      const data = {
        assignedTo: {
          key: 'emp-123',
          name: 'John Doe',
        },
      }
      const props = createMockProps(data, 'John Doe')

      render(<>{AssignedToLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'John Doe' })
      expect(link).toBeInTheDocument()
      expect(link).toHaveAttribute('href', '/organizations/employees/emp-123')
    })

    it('should return null when assignedTo is null', () => {
      const data = { assignedTo: null }
      const props = createMockProps(data)
      const result = AssignedToLinkCellRenderer(props)
      expect(result).toBeNull()
    })
  })
})

describe('Team Cell Renderers', () => {
  describe('TeamNameLinkCellRenderer', () => {
    it('should render team link for Team type', () => {
      const data = {
        key: 1,
        name: 'Engineering Team',
        type: 'Team',
      }
      const props = createMockProps(data)

      render(<>{TeamNameLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Engineering Team' })
      expect(link).toBeInTheDocument()
      expect(link).toHaveAttribute('href', '/organizations/teams/1')
    })

    it('should render team-of-teams link for non-Team type', () => {
      const data = {
        key: 2,
        name: 'Product Division',
        type: 'TeamOfTeams',
      }
      const props = createMockProps(data)

      render(<>{TeamNameLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Product Division' })
      expect(link).toHaveAttribute('href', '/organizations/team-of-teams/2')
    })
  })

  describe('NestedTeamNameLinkCellRenderer', () => {
    it('should render nested team link', () => {
      const data = {
        team: {
          key: 1,
          name: 'Dev Team',
          type: 'Team',
        },
      }
      const props = createMockProps(data)

      render(<>{NestedTeamNameLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Dev Team' })
      expect(link).toHaveAttribute('href', '/organizations/teams/1')
    })

    it('should return null when team is null', () => {
      const data = { team: null }
      const props = createMockProps(data)
      const result = NestedTeamNameLinkCellRenderer(props)
      expect(result).toBeNull()
    })
  })
})

describe('Sprint Cell Renderers', () => {
  describe('WorkSprintLinkCellRenderer', () => {
    it('should render sprint link', () => {
      const data = {
        key: 10,
        name: 'Sprint 23',
      }
      const props = createMockProps(data)

      render(<>{WorkSprintLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Sprint 23' })
      expect(link).toBeInTheDocument()
      expect(link).toHaveAttribute('href', '/planning/sprints/10')
    })

    it('should render sprint with team code when showTeamCode is true', () => {
      const data = {
        key: 10,
        name: 'Sprint 23',
        team: { code: 'ENG' },
      }
      const props = { ...createMockProps(data), showTeamCode: true }

      render(<>{WorkSprintLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Sprint 23 (ENG)' })
      expect(link).toBeInTheDocument()
    })

    it('should not render team code when showTeamCode is false', () => {
      const data = {
        key: 10,
        name: 'Sprint 23',
        team: { code: 'ENG' },
      }
      const props = { ...createMockProps(data), showTeamCode: false }

      render(<>{WorkSprintLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Sprint 23' })
      expect(link).toBeInTheDocument()
    })
  })

  describe('NestedWorkSprintLinkCellRenderer', () => {
    it('should render nested sprint link', () => {
      const data = {
        sprint: {
          key: 10,
          name: 'Sprint 23',
        },
      }
      const props = createMockProps(data)

      render(<>{NestedWorkSprintLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Sprint 23' })
      expect(link).toBeInTheDocument()
    })
  })
})

describe('Project/Program/Portfolio Cell Renderers', () => {
  describe('PortfolioLinkCellRenderer', () => {
    it('should render portfolio link', () => {
      const data = {
        key: 'portfolio-1',
        name: 'Enterprise Portfolio',
      }
      const props = createMockProps(data)

      render(<>{PortfolioLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Enterprise Portfolio' })
      expect(link).toHaveAttribute('href', '/ppm/portfolios/portfolio-1')
    })
  })

  describe('ProgramLinkCellRenderer', () => {
    it('should render program link from direct data', () => {
      const data = {
        key: 'program-1',
        name: 'Digital Transformation',
      }
      const props = createMockProps(data)

      render(<>{ProgramLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Digital Transformation' })
      expect(link).toHaveAttribute('href', '/ppm/programs/program-1')
    })

    it('should render program link from nested data', () => {
      const data = {
        program: {
          key: 'program-1',
          name: 'Digital Transformation',
        },
      }
      const props = createMockProps(data)

      render(<>{ProgramLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Digital Transformation' })
      expect(link).toHaveAttribute('href', '/ppm/programs/program-1')
    })
  })

  describe('ProjectLinkCellRenderer', () => {
    it('should render project link from nested data', () => {
      const data = {
        project: {
          key: 'project-1',
          name: 'Customer Portal',
        },
      }
      const props = createMockProps(data)

      render(<>{ProjectLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Customer Portal' })
      expect(link).toHaveAttribute('href', '/ppm/projects/project-1')
    })
  })
})

describe('Planning Interval Cell Renderers', () => {
  describe('PlanningIntervalLinkCellRenderer', () => {
    it('should render planning interval link', () => {
      const data = {
        key: 'pi-2024-q1',
        name: '2024 Q1',
      }
      const props = createMockProps(data)

      render(<>{PlanningIntervalLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: '2024 Q1' })
      expect(link).toHaveAttribute('href', '/planning/planning-intervals/pi-2024-q1')
    })
  })
})

describe('Workspace Cell Renderer', () => {
  describe('WorkspaceLinkCellRenderer', () => {
    it('should render workspace link', () => {
      const data = {
        key: 'workspace-1',
        name: 'Main Workspace',
      }
      const props = createMockProps(data)

      render(<>{WorkspaceLinkCellRenderer(props)}</>)

      const link = screen.getByRole('link', { name: 'Main Workspace' })
      expect(link).toHaveAttribute('href', '/work/workspaces/workspace-1')
    })
  })
})

describe('Status Cell Renderers', () => {
  describe('WorkStatusTagCellRenderer', () => {
    it('should render work status tag', () => {
      const data = {
        status: 'In Progress',
        statusCategory: {
          id: WorkStatusCategory.Active,
        },
      }
      const props = createMockProps(data, 'In Progress')

      // Just verify it doesn't throw - actual tag rendering is handled by WorkStatusTag component
      const result = WorkStatusTagCellRenderer(props)
      expect(result).not.toBeNull()
    })

    it('should return null when status is missing', () => {
      const data = {
        status: null,
        statusCategory: { id: WorkStatusCategory.Active },
      }
      const props = createMockProps(data)
      const result = WorkStatusTagCellRenderer(props)
      expect(result).toBeNull()
    })
  })

  describe('DependencyHealthCellRenderer', () => {
    it('should render dependency health tag', () => {
      const data = {
        health: {
          id: DependencyHealth.Healthy,
          name: 'Healthy',
        },
      }
      const props = createMockProps(data, 'Healthy')

      // Just verify it doesn't throw - actual tag rendering is handled by DependencyHealthTag component
      const result = DependencyHealthCellRenderer(props)
      expect(result).not.toBeNull()
    })
  })
})

describe('Markdown Cell Renderer', () => {
  describe('MarkdownCellRenderer', () => {
    it('should render markdown content', () => {
      const props = createMockProps(null, '# Hello World')

      // Just verify it doesn't throw - actual markdown rendering is handled by MarkdownRenderer component
      const result = MarkdownCellRenderer(props)
      expect(result).not.toBeNull()
    })

    it('should return null when value is empty', () => {
      const props = createMockProps(null, null)
      const result = MarkdownCellRenderer(props)
      expect(result).toBeNull()
    })
  })
})

describe('Helper Functions', () => {
  describe('renderWorkItemLinkHelper', () => {
    it('should render work item link', () => {
      const workItem = {
        key: 'WI-456',
        workspaceKey: 'ws-1',
      }

      render(<>{renderWorkItemLinkHelper(workItem)}</>)

      const link = screen.getByRole('link', { name: 'WI-456' })
      expect(link).toHaveAttribute('href', '/work/workspaces/ws-1/work-items/WI-456')
    })

    it('should return null when workItem is null', () => {
      const result = renderWorkItemLinkHelper(null)
      expect(result).toBeNull()
    })

    it('should return null when workItem is undefined', () => {
      const result = renderWorkItemLinkHelper(undefined)
      expect(result).toBeNull()
    })
  })

  describe('renderTeamLinkHelper', () => {
    it('should render team link with number key', () => {
      const team = {
        key: 123,
        name: 'Backend Team',
        type: 'Team',
      }

      render(<>{renderTeamLinkHelper(team)}</>)

      const link = screen.getByRole('link', { name: 'Backend Team' })
      expect(link).toHaveAttribute('href', '/organizations/teams/123')
    })

    it('should render team link with string key', () => {
      const team = {
        key: 'team-abc',
        name: 'Frontend Team',
        type: 'Team',
      }

      render(<>{renderTeamLinkHelper(team)}</>)

      const link = screen.getByRole('link', { name: 'Frontend Team' })
      expect(link).toHaveAttribute('href', '/organizations/teams/team-abc')
    })

    it('should render team-of-teams link', () => {
      const team = {
        key: 456,
        name: 'Engineering Division',
      }

      render(<>{renderTeamLinkHelper(team)}</>)

      const link = screen.getByRole('link', { name: 'Engineering Division' })
      expect(link).toHaveAttribute('href', '/organizations/team-of-teams/456')
    })

    it('should return null when team is null', () => {
      const result = renderTeamLinkHelper(null)
      expect(result).toBeNull()
    })
  })

  describe('renderSprintLinkHelper', () => {
    it('should render sprint link with number key', () => {
      const sprint = {
        key: 42,
        name: 'Sprint 42',
      }

      render(<>{renderSprintLinkHelper(sprint)}</>)

      const link = screen.getByRole('link', { name: 'Sprint 42' })
      expect(link).toHaveAttribute('href', '/planning/sprints/42')
    })

    it('should render sprint link with string key', () => {
      const sprint = {
        key: 'sprint-q1',
        name: 'Q1 Sprint',
      }

      render(<>{renderSprintLinkHelper(sprint)}</>)

      const link = screen.getByRole('link', { name: 'Q1 Sprint' })
      expect(link).toHaveAttribute('href', '/planning/sprints/sprint-q1')
    })

    it('should return null when sprint is null', () => {
      const result = renderSprintLinkHelper(null)
      expect(result).toBeNull()
    })
  })
})

describe('DateTime Cell Renderer', () => {
  describe('DateTimeCellRenderer', () => {
    it('should format date with time', () => {
      const dateValue = '2025-11-23T14:30:00.000Z'
      const props = createMockProps(null, dateValue)

      const result = DateTimeCellRenderer(props)

      // The result should be a formatted string
      expect(typeof result).toBe('string')
      expect(result).toBeTruthy()
      // Should contain year
      expect(result).toContain('2025')
      // Should contain month (11)
      expect(result).toContain('11')
      // Should contain day (23)
      expect(result).toContain('23')
    })

    it('should return empty string when value is null', () => {
      const props = createMockProps(null, null)
      const result = DateTimeCellRenderer(props)
      expect(result).toBe('')
    })

    it('should return empty string when value is undefined', () => {
      const props = createMockProps(null, undefined)
      const result = DateTimeCellRenderer(props)
      expect(result).toBe('')
    })

    it('should handle Date objects', () => {
      const dateValue = new Date('2025-11-23T14:30:00.000Z')
      const props = createMockProps(null, dateValue)

      const result = DateTimeCellRenderer(props)

      expect(typeof result).toBe('string')
      expect(result).toBeTruthy()
      expect(result).toContain('2025')
    })

    it('should format date with two-digit month and day', () => {
      // Create a date with single digit month and day to test formatting
      const dateValue = '2025-01-05T09:05:00.000Z'
      const props = createMockProps(null, dateValue)

      const result = DateTimeCellRenderer(props)

      // Should contain two-digit month and day
      expect(result).toContain('01')
      expect(result).toContain('05')
    })
  })
})
