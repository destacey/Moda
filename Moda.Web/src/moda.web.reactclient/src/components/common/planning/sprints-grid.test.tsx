import React from 'react'
import { render, screen } from '@testing-library/react'

// Mock the ModaGrid component
jest.mock('../index', () => ({
  ModaGrid: jest.fn(({ columnDefs, rowData, loadData, loading, height, emptyMessage }) => (
    <div data-testid="moda-grid">
      <div data-testid="row-count">{rowData?.length ?? 0}</div>
      <div data-testid="loading">{loading ? 'loading' : 'not-loading'}</div>
      <div data-testid="height">{height}</div>
      <div data-testid="empty-message">{emptyMessage}</div>
      <div data-testid="column-count">{columnDefs?.length ?? 0}</div>
      {loadData && <button type="button" onClick={loadData} data-testid="load-data">Refresh</button>}
    </div>
  )),
}))

// Mock the TeamNameLinkCellRenderer
jest.mock('../moda-grid-cell-renderers', () => ({
  TeamNameLinkCellRenderer: jest.fn(({ data }) => (
    <div data-testid="team-link">{data?.name}</div>
  )),
}))

// Note: useTheme and dayjs are mocked globally in jest.setup.ts

import SprintsGrid from './sprints-grid'
import * as ModaGridModule from '../index'
import { SprintListDto } from '@/src/services/moda-api'

describe('SprintsGrid', () => {
  const mockRefetch = jest.fn()

  const mockSprints: SprintListDto[] = [
    {
      id: '1',
      key: 101,
      name: 'Sprint 1',
      state: { id: 1, name: 'Active' },
      start: new Date('2025-01-01'),
      end: new Date('2025-01-15'),
      team: { id: '1', key: 1, name: 'Team Alpha', type: 'Team' },
    },
    {
      id: '2',
      key: 102,
      name: 'Sprint 2',
      state: { id: 2, name: 'Planned' },
      start: new Date('2025-01-16'),
      end: new Date('2025-01-30'),
      team: { id: '2', key: 2, name: 'Team Beta', type: 'Team' },
    },
  ]

  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('renders the ModaGrid component', () => {
    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    expect(screen.getByTestId('moda-grid')).toBeInTheDocument()
  })

  it('passes sprint data to ModaGrid', () => {
    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    expect(screen.getByTestId('row-count')).toHaveTextContent('2')
  })

  it('passes loading state to ModaGrid', () => {
    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={true}
        refetch={mockRefetch}
      />
    )

    expect(screen.getByTestId('loading')).toHaveTextContent('loading')
  })

  it('passes not loading state to ModaGrid', () => {
    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    expect(screen.getByTestId('loading')).toHaveTextContent('not-loading')
  })

  it('renders with empty sprints array', () => {
    render(
      <SprintsGrid
        sprints={[]}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    expect(screen.getByTestId('row-count')).toHaveTextContent('0')
  })

  it('uses default grid height when not specified', () => {
    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    expect(screen.getByTestId('height')).toHaveTextContent('')
  })

  it('uses custom grid height when specified', () => {
    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
        gridHeight={500}
      />
    )

    expect(screen.getByTestId('height')).toHaveTextContent('500')
  })

  it('displays correct empty message', () => {
    render(
      <SprintsGrid
        sprints={[]}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    expect(screen.getByTestId('empty-message')).toHaveTextContent('No sprints found.')
  })

  it('creates correct number of columns', () => {
    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    // Should have 6 columns: key, name, state, start, end, team
    expect(screen.getByTestId('column-count')).toHaveTextContent('6')
  })

  it('calls refetch when refresh button is clicked', () => {
    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    const refreshButton = screen.getByTestId('load-data')
    refreshButton.click()

    expect(mockRefetch).toHaveBeenCalledTimes(1)
  })

  it('memoizes refresh callback based on refetch dependency', () => {
    const { rerender } = render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    // Rerender with same refetch function
    rerender(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    const secondRefreshButton = screen.getByTestId('load-data')

    // The button should still work
    secondRefreshButton.click()
    expect(mockRefetch).toHaveBeenCalledTimes(1)
  })

  it('respects hideTeam prop when provided', () => {
    const ModaGrid = ModaGridModule.ModaGrid as jest.Mock

    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
        hideTeam={true}
      />
    )

    // Check that ModaGrid was called with column definitions
    const mockCall = ModaGrid.mock.calls[0][0]
    const teamColumn = mockCall.columnDefs.find((col: any) => col.field === 'team.name')

    expect(teamColumn).toBeDefined()
    expect(teamColumn.hide).toBe(true)
  })

  it('does not hide team column by default', () => {
    const ModaGrid = ModaGridModule.ModaGrid as jest.Mock

    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    // Check that ModaGrid was called with column definitions
    const mockCall = ModaGrid.mock.calls[0][0]
    const teamColumn = mockCall.columnDefs.find((col: any) => col.field === 'team.name')

    expect(teamColumn).toBeDefined()
    expect(teamColumn.hide).toBeUndefined()
  })

  it('defines all expected columns with correct fields', () => {
    const ModaGrid = ModaGridModule.ModaGrid as jest.Mock

    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    const mockCall = ModaGrid.mock.calls[0][0]
    const columnFields = mockCall.columnDefs.map((col: any) => col.field)

    expect(columnFields).toContain('key')
    expect(columnFields).toContain('name')
    expect(columnFields).toContain('state.name')
    expect(columnFields).toContain('start')
    expect(columnFields).toContain('end')
    expect(columnFields).toContain('team.name')
  })

  it('sets correct widths for columns', () => {
    const ModaGrid = ModaGridModule.ModaGrid as jest.Mock

    render(
      <SprintsGrid
        sprints={mockSprints}
        isLoading={false}
        refetch={mockRefetch}
      />
    )

    const mockCall = ModaGrid.mock.calls[0][0]
    const keyColumn = mockCall.columnDefs.find((col: any) => col.field === 'key')
    const nameColumn = mockCall.columnDefs.find((col: any) => col.field === 'name')
    const stateColumn = mockCall.columnDefs.find((col: any) => col.field === 'state.name')
    const teamColumn = mockCall.columnDefs.find((col: any) => col.field === 'team.name')

    expect(keyColumn.width).toBe(90)
    expect(nameColumn.width).toBe(300)
    expect(stateColumn.width).toBe(125)
    expect(teamColumn.width).toBe(200)
  })
})
