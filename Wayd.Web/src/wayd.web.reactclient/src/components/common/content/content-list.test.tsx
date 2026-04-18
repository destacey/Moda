import { render, screen } from '@testing-library/react'
import ContentList from './content-list'

describe('ContentList', () => {
  describe('when items is empty', () => {
    it('renders nothing when no emptyText is provided', () => {
      const { container } = render(<ContentList items={[]} />)
      expect(container).toBeEmptyDOMElement()
    })

    it('renders emptyText when provided', () => {
      render(<ContentList items={[]} emptyText="None" />)
      expect(screen.getByText('None')).toBeInTheDocument()
    })
  })

  describe('when items has entries', () => {
    const items = ['Alice', 'Bob', 'Charlie']

    it('renders a list item for each entry', () => {
      render(<ContentList items={items} />)
      expect(screen.getByText('Alice')).toBeInTheDocument()
      expect(screen.getByText('Bob')).toBeInTheDocument()
      expect(screen.getByText('Charlie')).toBeInTheDocument()
    })

    it('renders the correct number of list items', () => {
      render(<ContentList items={items} />)
      expect(screen.getAllByRole('listitem')).toHaveLength(items.length)
    })

    it('does not render emptyText even when provided', () => {
      render(<ContentList items={items} emptyText="None" />)
      expect(screen.queryByText('None')).not.toBeInTheDocument()
    })
  })
})
