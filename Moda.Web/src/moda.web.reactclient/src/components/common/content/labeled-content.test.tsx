import { render, screen } from '@testing-library/react'
import LabeledContent from './labeled-content'

describe('LabeledContent', () => {
  it('renders the label', () => {
    render(<LabeledContent label="Status">value</LabeledContent>)
    expect(screen.getByText('Status')).toBeInTheDocument()
  })

  it('renders string children', () => {
    render(<LabeledContent label="Status">Active</LabeledContent>)
    expect(screen.getByText('Active')).toBeInTheDocument()
  })

  it('renders element children', () => {
    render(
      <LabeledContent label="Owner">
        <span data-testid="child">Jane Doe</span>
      </LabeledContent>,
    )
    expect(screen.getByTestId('child')).toBeInTheDocument()
    expect(screen.getByText('Jane Doe')).toBeInTheDocument()
  })

  it('renders multiple children', () => {
    render(
      <LabeledContent label="Tags">
        <span>Alpha</span>
        <span>Beta</span>
      </LabeledContent>,
    )
    expect(screen.getByText('Alpha')).toBeInTheDocument()
    expect(screen.getByText('Beta')).toBeInTheDocument()
  })

  it('renders label without tooltip when tooltip is not provided', () => {
    render(<LabeledContent label="Status">value</LabeledContent>)
    const label = screen.getByText('Status')
    expect(label).toBeInTheDocument()
    expect(label).not.toHaveStyle({ cursor: 'help' })
  })

  it('renders label with help cursor when tooltip is provided', () => {
    render(
      <LabeledContent label="Description" tooltip="Some help text">
        value
      </LabeledContent>,
    )
    const label = screen.getByText('Description')
    expect(label).toBeInTheDocument()
    expect(label).toHaveStyle({ cursor: 'help' })
  })

  it('wraps label in tooltip when tooltip is provided', () => {
    render(
      <LabeledContent label="Business Case" tooltip="Why this project matters">
        value
      </LabeledContent>,
    )
    // The label should still render
    expect(screen.getByText('Business Case')).toBeInTheDocument()
    // The tooltip text is not visible until hover, but the label should have cursor: help
    expect(screen.getByText('Business Case')).toHaveStyle({ cursor: 'help' })
  })
})
