import { fireEvent, render, screen } from '@testing-library/react'
import ExpandableContent from './expandable-content'

// jsdom does not compute layout, so scrollHeight is always 0.
// These helpers let individual tests control whether content "overflows".
const setScrollHeight = (value: number) => {
  Object.defineProperty(HTMLElement.prototype, 'scrollHeight', {
    configurable: true,
    get: () => value,
  })
}

const LINE_HEIGHT_PX = 24
const DEFAULT_LINES = 4
const DEFAULT_MAX_HEIGHT = LINE_HEIGHT_PX * DEFAULT_LINES // 96px

describe('ExpandableContent', () => {
  describe('when content does not overflow', () => {
    beforeEach(() => {
      setScrollHeight(DEFAULT_MAX_HEIGHT - 1) // 95px — fits within default clamp
    })

    it('renders children', () => {
      render(<ExpandableContent><p>Short content</p></ExpandableContent>)
      expect(screen.getByText('Short content')).toBeInTheDocument()
    })

    it('does not render the Show more button', () => {
      render(<ExpandableContent><p>Short content</p></ExpandableContent>)
      expect(screen.queryByRole('button')).not.toBeInTheDocument()
    })

    it('does not render the fade overlay', () => {
      const { container } = render(
        <ExpandableContent><p>Short content</p></ExpandableContent>,
      )
      expect(
        container.querySelector('.expandable-content-fade'),
      ).not.toBeInTheDocument()
    })
  })

  describe('when content overflows', () => {
    beforeEach(() => {
      setScrollHeight(DEFAULT_MAX_HEIGHT + 1) // 97px — exceeds default clamp
    })

    it('renders children', () => {
      render(<ExpandableContent><p>Long content</p></ExpandableContent>)
      expect(screen.getByText('Long content')).toBeInTheDocument()
    })

    it('renders the Show more button', () => {
      render(<ExpandableContent><p>Long content</p></ExpandableContent>)
      expect(screen.getByRole('button', { name: /show more/i })).toBeInTheDocument()
    })

    it('renders the fade overlay', () => {
      const { container } = render(
        <ExpandableContent><p>Long content</p></ExpandableContent>,
      )
      expect(
        container.querySelector('.expandable-content-fade'),
      ).toBeInTheDocument()
    })

    it('clamps the content height to the default max height', () => {
      const { container } = render(
        <ExpandableContent><p>Long content</p></ExpandableContent>,
      )
      const clamped = container.querySelector<HTMLElement>(
        '.expandable-content-clamp > div',
      )
      expect(clamped?.style.maxHeight).toBe(`${DEFAULT_MAX_HEIGHT}px`)
    })

    describe('after clicking Show more', () => {
      it('changes button label to Show less', () => {
        render(<ExpandableContent><p>Long content</p></ExpandableContent>)
        fireEvent.click(screen.getByRole('button', { name: /show more/i }))
        expect(screen.getByRole('button', { name: /show less/i })).toBeInTheDocument()
      })

      it('removes the max height constraint', () => {
        const { container } = render(
          <ExpandableContent><p>Long content</p></ExpandableContent>,
        )
        fireEvent.click(screen.getByRole('button', { name: /show more/i }))
        const clamped = container.querySelector<HTMLElement>(
          '.expandable-content-clamp > div',
        )
        expect(clamped?.style.maxHeight).toBe('')
      })

      it('removes the fade overlay', () => {
        const { container } = render(
          <ExpandableContent><p>Long content</p></ExpandableContent>,
        )
        fireEvent.click(screen.getByRole('button', { name: /show more/i }))
        expect(
          container.querySelector('.expandable-content-fade'),
        ).not.toBeInTheDocument()
      })
    })

    describe('after expanding then collapsing', () => {
      it('restores Show more button label', () => {
        render(<ExpandableContent><p>Long content</p></ExpandableContent>)
        fireEvent.click(screen.getByRole('button', { name: /show more/i }))
        fireEvent.click(screen.getByRole('button', { name: /show less/i }))
        expect(screen.getByRole('button', { name: /show more/i })).toBeInTheDocument()
      })

      it('restores the max height clamp', () => {
        const { container } = render(
          <ExpandableContent><p>Long content</p></ExpandableContent>,
        )
        fireEvent.click(screen.getByRole('button', { name: /show more/i }))
        fireEvent.click(screen.getByRole('button', { name: /show less/i }))
        const clamped = container.querySelector<HTMLElement>(
          '.expandable-content-clamp > div',
        )
        expect(clamped?.style.maxHeight).toBe(`${DEFAULT_MAX_HEIGHT}px`)
      })

      it('restores the fade overlay', () => {
        const { container } = render(
          <ExpandableContent><p>Long content</p></ExpandableContent>,
        )
        fireEvent.click(screen.getByRole('button', { name: /show more/i }))
        fireEvent.click(screen.getByRole('button', { name: /show less/i }))
        expect(
          container.querySelector('.expandable-content-fade'),
        ).toBeInTheDocument()
      })
    })
  })

  describe('custom lines prop', () => {
    it('uses the provided lines value to compute max height', () => {
      const customLines = 6
      const customMaxHeight = LINE_HEIGHT_PX * customLines // 144px
      setScrollHeight(customMaxHeight + 1) // overflow at custom height

      const { container } = render(
        <ExpandableContent lines={customLines}><p>Long content</p></ExpandableContent>,
      )
      const clamped = container.querySelector<HTMLElement>(
        '.expandable-content-clamp > div',
      )
      expect(clamped?.style.maxHeight).toBe(`${customMaxHeight}px`)
    })

    it('does not overflow when content fits within custom lines', () => {
      const customLines = 6
      const customMaxHeight = LINE_HEIGHT_PX * customLines
      setScrollHeight(customMaxHeight - 1) // fits within custom height

      render(
        <ExpandableContent lines={customLines}><p>Short content</p></ExpandableContent>,
      )
      expect(screen.queryByRole('button')).not.toBeInTheDocument()
    })

    it('overflows when content exceeds custom lines', () => {
      const customLines = 2
      const customMaxHeight = LINE_HEIGHT_PX * customLines // 48px
      setScrollHeight(customMaxHeight + 1)

      render(
        <ExpandableContent lines={customLines}><p>Long content</p></ExpandableContent>,
      )
      expect(screen.getByRole('button', { name: /show more/i })).toBeInTheDocument()
    })
  })
})
