import { render, screen } from '@testing-library/react'
import TreeGridToolbar from '../tree-grid-toolbar'

describe('TreeGridToolbar', () => {
  const defaultProps = {
    displayedRowCount: 5,
    totalRowCount: 10,
    searchValue: '',
    onSearchChange: jest.fn(),
    onClearFilters: jest.fn(),
    hasActiveFilters: false,
    isLoading: false,
  }

  it('renders row count', () => {
    render(<TreeGridToolbar {...defaultProps} />)
    expect(screen.getByText('5 of 10')).toBeInTheDocument()
  })

  it('renders search input with value', () => {
    render(<TreeGridToolbar {...defaultProps} searchValue="test query" />)
    const input = screen.getByPlaceholderText('Search')
    expect(input).toHaveValue('test query')
  })

  it('renders leftSlot content', () => {
    render(
      <TreeGridToolbar
        {...defaultProps}
        leftSlot={<button>Create Task</button>}
      />,
    )
    expect(screen.getByText('Create Task')).toBeInTheDocument()
  })

  it('renders help popover button when helpContent provided', () => {
    render(
      <TreeGridToolbar
        {...defaultProps}
        helpContent={<div>Keyboard shortcuts here</div>}
      />,
    )
    // QuestionCircleOutlined icon has aria-label="question-circle"
    expect(screen.getByLabelText('question-circle')).toBeInTheDocument()
  })

  it('does not render help button when helpContent is not provided', () => {
    render(<TreeGridToolbar {...defaultProps} />)
    expect(
      screen.queryByLabelText('question-circle'),
    ).not.toBeInTheDocument()
  })

  it('disables clear filters button when hasActiveFilters is false', () => {
    render(<TreeGridToolbar {...defaultProps} hasActiveFilters={false} />)
    // ClearOutlined icon has aria-label="clear"
    const clearIcon = screen.getByLabelText('clear')
    expect(clearIcon.closest('button')).toBeDisabled()
  })

  it('enables clear filters button when hasActiveFilters is true', () => {
    render(<TreeGridToolbar {...defaultProps} hasActiveFilters={true} />)
    const clearIcon = screen.getByLabelText('clear')
    expect(clearIcon.closest('button')).not.toBeDisabled()
  })

  it('renders refresh button when onRefresh is provided', () => {
    render(<TreeGridToolbar {...defaultProps} onRefresh={jest.fn()} />)
    expect(screen.getByLabelText('reload')).toBeInTheDocument()
  })

  it('does not render refresh button when onRefresh is not provided', () => {
    render(<TreeGridToolbar {...defaultProps} />)
    expect(screen.queryByLabelText('reload')).not.toBeInTheDocument()
  })

  it('renders export button when onExportCsv is provided', () => {
    render(<TreeGridToolbar {...defaultProps} onExportCsv={jest.fn()} />)
    expect(screen.getByLabelText('download')).toBeInTheDocument()
  })

  it('disables export button when loading', () => {
    render(
      <TreeGridToolbar
        {...defaultProps}
        onExportCsv={jest.fn()}
        isLoading={true}
      />,
    )
    const downloadIcon = screen.getByLabelText('download')
    expect(downloadIcon.closest('button')).toBeDisabled()
  })

  it('disables export button when displayedRowCount is 0', () => {
    render(
      <TreeGridToolbar
        {...defaultProps}
        onExportCsv={jest.fn()}
        displayedRowCount={0}
      />,
    )
    const downloadIcon = screen.getByLabelText('download')
    expect(downloadIcon.closest('button')).toBeDisabled()
  })
})
