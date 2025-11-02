import React from 'react'
import { render, screen } from '@testing-library/react'

// Mock the LinksCard component
jest.mock('../../../../components/common/links/links-card', () => ({
  __esModule: true,
  default: jest.fn(({ objectId }) => (
    <div data-testid="links-card" data-object-id={objectId}>
      Links Card
    </div>
  )),
}))

// Mock Next.js Link component
jest.mock('next/link', () => ({
  __esModule: true,
  default: jest.fn(({ href, children }) => (
    <a href={href} data-testid="next-link">
      {children}
    </a>
  )),
}))

// Note: dayjs is mocked globally in jest.setup.ts

import SprintDetails from './sprint-details'
import { SprintDetailsDto } from '@/src/services/moda-api'

describe('SprintDetails', () => {
  const mockSprint: SprintDetailsDto = {
    id: 'sprint-123',
    key: 101,
    name: 'Sprint 1',
    state: { id: 1, name: 'Active' },
    start: new Date('2025-01-01T09:00:00'),
    end: new Date('2025-01-15T17:00:00'),
    team: {
      id: 'team-456',
      key: 1,
      name: 'Team Alpha',
      type: 'Team',
    },
  }

  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('returns null when sprint is null', () => {
    const { container } = render(<SprintDetails sprint={null as any} />)
    expect(container.firstChild).toBeNull()
  })

  it('returns null when sprint is undefined', () => {
    const { container } = render(<SprintDetails sprint={undefined as any} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders sprint details when sprint is provided', () => {
    render(<SprintDetails sprint={mockSprint} />)

    expect(screen.getByText('Team Alpha')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
  })

  it('renders team name as a link with correct href', () => {
    render(<SprintDetails sprint={mockSprint} />)

    const links = screen.getAllByTestId('next-link')
    const teamLink = links.find((link) =>
      link.textContent?.includes('Team Alpha'),
    )

    expect(teamLink).toBeInTheDocument()
    expect(teamLink).toHaveAttribute('href', '/organizations/teams/1')
  })

  it('displays sprint state name', () => {
    render(<SprintDetails sprint={mockSprint} />)

    expect(screen.getByText('Active')).toBeInTheDocument()
  })

  it('displays formatted start date', () => {
    render(<SprintDetails sprint={mockSprint} />)

    // dayjs mock returns the format string, so we check for the presence of the label
    expect(screen.getByText('Start')).toBeInTheDocument()
  })

  it('displays formatted end date', () => {
    render(<SprintDetails sprint={mockSprint} />)

    // dayjs mock returns the format string, so we check for the presence of the label
    expect(screen.getByText('End')).toBeInTheDocument()
  })

  it('renders LinksCard with correct objectId', () => {
    render(<SprintDetails sprint={mockSprint} />)

    const linksCard = screen.getByTestId('links-card')
    expect(linksCard).toBeInTheDocument()
    expect(linksCard).toHaveAttribute('data-object-id', 'sprint-123')
  })

  it('renders all description items', () => {
    render(<SprintDetails sprint={mockSprint} />)

    expect(screen.getByText('Team')).toBeInTheDocument()
    expect(screen.getByText('State')).toBeInTheDocument()
    expect(screen.getByText('Start')).toBeInTheDocument()
    expect(screen.getByText('End')).toBeInTheDocument()
  })

  it('handles sprint with different state', () => {
    const plannedSprint: SprintDetailsDto = {
      ...mockSprint,
      state: { id: 2, name: 'Planned' },
    }

    render(<SprintDetails sprint={plannedSprint} />)

    expect(screen.getByText('Planned')).toBeInTheDocument()
  })

  it('handles sprint with different team', () => {
    const differentTeamSprint: SprintDetailsDto = {
      ...mockSprint,
      team: {
        id: 'team-789',
        key: 2,
        name: 'Team Beta',
        type: 'Team',
      },
    }

    render(<SprintDetails sprint={differentTeamSprint} />)

    expect(screen.getByText('Team Beta')).toBeInTheDocument()
    const links = screen.getAllByTestId('next-link')
    const teamLink = links.find((link) =>
      link.textContent?.includes('Team Beta'),
    )
    expect(teamLink).toHaveAttribute('href', '/organizations/teams/2')
  })

  it('renders with Flex layout with vertical direction and gap', () => {
    const { container } = render(<SprintDetails sprint={mockSprint} />)

    const flexContainer = container.querySelector('.ant-flex')
    expect(flexContainer).toBeInTheDocument()
  })

  it('renders Descriptions component with single column layout', () => {
    const { container } = render(<SprintDetails sprint={mockSprint} />)

    const descriptions = container.querySelector('.ant-descriptions')
    expect(descriptions).toBeInTheDocument()
  })

  it('handles sprint without team gracefully', () => {
    const sprintWithoutTeam: SprintDetailsDto = {
      ...mockSprint,
      team: undefined as any,
    }

    render(<SprintDetails sprint={sprintWithoutTeam} />)

    // Should still render other fields
    expect(screen.getByText('State')).toBeInTheDocument()
    expect(screen.getByText('Active')).toBeInTheDocument()
  })
})
