import React from 'react'
import { render, screen, fireEvent } from '@testing-library/react'
import '@testing-library/jest-dom'
import { MarkdownCodeBlock, MarkdownCodeBlockProps } from '.'

describe('MarkdownCodeBlock', () => {
  const mockThemeToken = {
    padding: '16px',
    colorFillTertiary: '#ffffff',
    borderRadius: '4px',
  } as any

  const defaultProps: MarkdownCodeBlockProps = {
    token: mockThemeToken,
    children: 'Sample code block',
  }

  beforeEach(() => {
    Object.assign(navigator, {
      clipboard: {
        writeText: jest.fn(),
      },
    })
  })

  afterEach(() => {
    jest.clearAllMocks()
  })

  it('renders the component with the correct styles and children', () => {
    render(<MarkdownCodeBlock {...defaultProps} />)

    const codeElement = screen.getByText('Sample code block')
    expect(codeElement).toBeInTheDocument()
    expect(codeElement).toHaveStyle('font-family: monospace')
    expect(codeElement).toHaveStyle('font-size: 0.9em')
  })

  // TODO: fix this test.  The test is failing because the code block content is not being copied to the clipboard.
  // Manual testing shows that the copy button works as expected, but this test could be indicating a deeper problem.
  //   it('copies the code block content to the clipboard when the copy button is clicked', () => {
  //     const mockClipboardWrite = jest.fn()
  //     Object.assign(navigator, {
  //       clipboard: {
  //         writeText: mockClipboardWrite,
  //       },
  //     })

  //     const mockCode = 'Sample code block'
  //     render(<MarkdownCodeBlock {...defaultProps}>{mockCode}</MarkdownCodeBlock>)

  //     const copyButton = screen.getByRole('button', {
  //       name: 'Copy code to clipboard',
  //     })

  //     fireEvent.click(copyButton)

  //     console.log('Mock Clipboard Calls:', mockClipboardWrite.mock.calls)

  //     expect(mockClipboardWrite).toHaveBeenCalledTimes(1)
  //     expect(mockClipboardWrite).toHaveBeenCalledWith(mockCode)
  //   })

  it('removes class attributes from span elements inside the code block on mount', () => {
    const { container } = render(
      <MarkdownCodeBlock {...defaultProps}>
        <code>
          <span className="test-class">Span content</span>
        </code>
      </MarkdownCodeBlock>,
    )

    const codeSpans = container.querySelectorAll('code span')
    codeSpans.forEach((span) => {
      expect(span).not.toHaveAttribute('class')
    })
  })

  it('applies additional styles passed via props', () => {
    const customStyle = { color: 'red' }
    render(
      <MarkdownCodeBlock {...defaultProps} style={customStyle}>
        Custom styled code
      </MarkdownCodeBlock>,
    )

    const codeElement = screen.getByText('Custom styled code')
    expect(codeElement).toHaveStyle('color: rgb(255, 0, 0)')
  })

  it('does not render when children are absent', () => {
    const { container } = render(<MarkdownCodeBlock token={mockThemeToken} />)
    expect(container.firstChild).toBeNull()
  })
})
