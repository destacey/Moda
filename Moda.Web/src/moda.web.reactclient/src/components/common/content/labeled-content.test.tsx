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
})
