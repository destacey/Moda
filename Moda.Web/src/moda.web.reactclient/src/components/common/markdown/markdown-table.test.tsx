import React from 'react'
import { render, screen } from '@testing-library/react'
import '@testing-library/jest-dom'
import { MarkdownTable } from '.'

describe('MarkdownTable', () => {
  it('renders a table with the provided content', () => {
    render(
      <MarkdownTable>
        <thead>
          <tr>
            <th>Header 1</th>
            <th>Header 2</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Row 1, Col 1</td>
            <td>Row 1, Col 2</td>
          </tr>
          <tr>
            <td>Row 2, Col 1</td>
            <td>Row 2, Col 2</td>
          </tr>
        </tbody>
      </MarkdownTable>,
    )

    const tableElement = screen.getByRole('table')
    expect(tableElement).toBeInTheDocument()

    expect(screen.getByText('Header 1')).toBeInTheDocument()
    expect(screen.getByText('Header 2')).toBeInTheDocument()
    expect(screen.getByText('Row 1, Col 1')).toBeInTheDocument()
    expect(screen.getByText('Row 2, Col 2')).toBeInTheDocument()
  })

  it('applies overflowX style to the Paragraph wrapper', () => {
    const { container } = render(
      <MarkdownTable>
        <thead>
          <tr>
            <th>Header</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Content</td>
          </tr>
        </tbody>
      </MarkdownTable>,
    )

    const paragraphElement = container.querySelector('.ant-typography')
    expect(paragraphElement).toBeInTheDocument()
    expect(paragraphElement).toHaveStyle('overflow-x: auto')
  })

  it('supports additional table props', () => {
    render(
      <MarkdownTable border={1}>
        <thead>
          <tr>
            <th>Header</th>
          </tr>
        </thead>
        <tbody>
          <tr>
            <td>Content</td>
          </tr>
        </tbody>
      </MarkdownTable>,
    )

    const tableElement = screen.getByRole('table')
    expect(tableElement).toHaveAttribute('border', '1')
  })

  it('handles empty children gracefully', () => {
    render(<MarkdownTable />)

    const tableElement = screen.queryByRole('table')
    expect(tableElement).not.toBeInTheDocument()
  })
})
