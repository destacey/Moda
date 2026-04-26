import React from 'react'
import { render, screen } from '@testing-library/react'
import { IterationState } from '../../types'
import PlanningIntervalCard from './planning-interval-card'
import { useGetPlanningIntervalIterationsQuery } from '@/src/store/features/planning/planning-interval-api'

jest.mock('@/src/store/features/planning/planning-interval-api', () => ({
  useGetPlanningIntervalIterationsQuery: jest.fn(),
}))

jest.mock('next/link', () => {
  const MockedLink = ({
    children,
    href,
  }: {
    children: React.ReactNode
    href: string
  }) => <a href={href}>{children}</a>
  MockedLink.displayName = 'Link'
  return MockedLink
})

const mockIterationsQuery =
  useGetPlanningIntervalIterationsQuery as unknown as jest.Mock

const createPlanningInterval = (overrides: Record<string, unknown> = {}) =>
  ({
    id: 'pi-1',
    key: 42,
    name: '23.4',
    start: new Date('2026-01-01'),
    end: new Date('2026-03-31'),
    state: { id: IterationState.Active, name: 'Active' },
    ...overrides,
  }) as any

describe('PlanningIntervalCard', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    mockIterationsQuery.mockReturnValue({ data: [] })
  })

  it('shows a Details link for Future planning intervals', () => {
    render(
      <PlanningIntervalCard
        planningInterval={createPlanningInterval({
          state: { id: IterationState.Future, name: 'Future' },
        })}
      />,
    )

    expect(screen.getByRole('link', { name: 'Details' })).toHaveAttribute(
      'href',
      '/planning/planning-intervals/42/details',
    )
    expect(screen.queryByRole('link', { name: 'Overview' })).not.toBeInTheDocument()
  })

  it('shows an Overview link for non-future planning intervals', () => {
    render(
      <PlanningIntervalCard
        planningInterval={createPlanningInterval({
          state: { id: IterationState.Active, name: 'Active' },
        })}
      />,
    )

    expect(screen.getByRole('link', { name: 'Overview' })).toHaveAttribute(
      'href',
      '/planning/planning-intervals/42/overview',
    )
    expect(screen.queryByRole('link', { name: 'Details' })).not.toBeInTheDocument()
  })
})

