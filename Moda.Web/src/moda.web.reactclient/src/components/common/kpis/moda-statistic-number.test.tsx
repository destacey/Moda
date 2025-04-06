import { render, screen } from '@testing-library/react'
import ModaStatisticNumber from './moda-statistic-number'

describe('ModaStatisticNumber', () => {
  it('renders the number correctly', () => {
    render(<ModaStatisticNumber value={12345.67} />)
    const numberElement = screen.getByText((content, element) => {
      const hasText = (node: Element) => node.textContent === '12,345.67'
      const elementHasText = hasText(element!)
      const childrenDontHaveText = Array.from(element!.children).every(
        (child) => !hasText(child),
      )
      return elementHasText && childrenDontHaveText
    })
    expect(numberElement).toBeInTheDocument()
  })

  it('renders nothing when value is null', () => {
    const { container } = render(<ModaStatisticNumber value={null} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders nothing when value is undefined', () => {
    const { container } = render(<ModaStatisticNumber value={undefined} />)
    expect(container.firstChild).toBeNull()
  })

  it('renders 0 correctly', () => {
    render(<ModaStatisticNumber value={0} />)
    const numberElement = screen.getByText('0')
    expect(numberElement).toBeInTheDocument()
  })

  it('renders the number with the specified precision (no rounding)', () => {
    render(<ModaStatisticNumber value={12345.6789} precision={2} />)

    // Match the entire formatted number
    const numberElement = screen.getByText((content, element) => {
      const hasText = (node: Element) => node.textContent === '12,345.67'
      const elementHasText = hasText(element!)
      const childrenDontHaveText = Array.from(element!.children).every(
        (child) => !hasText(child),
      )
      return elementHasText && childrenDontHaveText
    })

    expect(numberElement).toBeInTheDocument()
  })
})
