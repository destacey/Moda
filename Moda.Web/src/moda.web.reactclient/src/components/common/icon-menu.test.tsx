import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { SwapOutlined } from '@ant-design/icons'
import IconMenu, { IconMenuProps } from './icon-menu'
import useTheme from '../contexts/theme'

// Mock the useTheme hook
jest.mock('../contexts/theme', () => ({
  __esModule: true,
  default: jest.fn(),
}))

// Mock matchMedia for Ant Design components
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: jest.fn().mockImplementation((query) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: jest.fn(),
    removeListener: jest.fn(),
    addEventListener: jest.fn(),
    removeEventListener: jest.fn(),
    dispatchEvent: jest.fn(),
  })),
})

describe('IconMenu', () => {
  const mockToken = {
    colorTextQuaternary: 'rgba(0, 0, 0, 0.25)',
    colorBgElevated: '#ffffff',
  }

  const mockItems = [
    { label: 'Sprint 1', value: 1, extra: 'Active' },
    { label: 'Sprint 2', value: 2, extra: 'Completed' },
    { label: 'Sprint 3', value: 3, extra: 'Future' },
  ]

  const defaultProps: IconMenuProps = {
    icon: <SwapOutlined />,
    items: mockItems,
    tooltip: 'Switch sprint',
    onChange: jest.fn(),
  }

  beforeEach(() => {
    ;(useTheme as jest.Mock).mockReturnValue({ token: mockToken })
    jest.clearAllMocks()
  })

  it('renders the icon button', () => {
    render(<IconMenu {...defaultProps} />)
    const button = screen.getByRole('button')
    expect(button).toBeInTheDocument()
  })

  it('renders tooltip when provided', async () => {
    render(<IconMenu {...defaultProps} />)
    const button = screen.getByRole('button')

    // Hover over the button to trigger tooltip
    fireEvent.mouseEnter(button)

    // Wait for tooltip to appear
    await waitFor(() => {
      expect(screen.getByText('Switch sprint')).toBeInTheDocument()
    })
  })

  it('does not render when icon is missing', () => {
    render(<IconMenu {...defaultProps} icon={null} />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('does not render when items array is empty', () => {
    render(<IconMenu {...defaultProps} items={[]} />)
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('opens dropdown menu when clicked', async () => {
    render(<IconMenu {...defaultProps} />)
    const button = screen.getByRole('button')
    fireEvent.click(button)

    // Check if menu items are in the document (even if off-screen)
    await waitFor(() => {
      const menuItems = screen.getAllByRole('menuitem')
      expect(menuItems).toHaveLength(3)
    })
  })

  it('renders items without extra when extra is not provided', async () => {
    const itemsWithoutExtra = [
      { label: 'Sprint 1', value: 1 },
      { label: 'Sprint 2', value: 2 },
    ]
    render(<IconMenu {...defaultProps} items={itemsWithoutExtra} />)
    const button = screen.getByRole('button')
    fireEvent.click(button)

    await waitFor(() => {
      const menuItems = screen.getAllByRole('menuitem')
      expect(menuItems).toHaveLength(2)
    })
  })

  it('calls onChange with correct value when menu item is clicked', async () => {
    const onChange = jest.fn()
    render(<IconMenu {...defaultProps} onChange={onChange} />)

    const button = screen.getByRole('button')
    fireEvent.click(button)

    await waitFor(() => {
      const menuItems = screen.getAllByRole('menuitem')
      expect(menuItems).toHaveLength(3)
    })

    // Click the second menu item
    const menuItems = screen.getAllByRole('menuitem')
    fireEvent.click(menuItems[1])

    expect(onChange).toHaveBeenCalledWith(2)
  })

  it('highlights selected item based on selectedKeys', async () => {
    render(<IconMenu {...defaultProps} selectedKeys={['2']} />)

    const button = screen.getByRole('button')
    fireEvent.click(button)

    await waitFor(() => {
      const menuItems = screen.getAllByRole('menuitem')
      expect(menuItems).toHaveLength(3)
      expect(menuItems[1]).toHaveClass('ant-dropdown-menu-item-selected')
    })
  })

  it('uses default maxHeight of 400 when not specified', async () => {
    render(<IconMenu {...defaultProps} />)
    const button = screen.getByRole('button')
    fireEvent.click(button)

    await waitFor(() => {
      const menu = document.querySelector('.ant-dropdown-menu')
      expect(menu).toHaveStyle({ maxHeight: '400px' })
    })
  })

  it('uses custom maxHeight when provided', async () => {
    render(<IconMenu {...defaultProps} maxHeight={300} />)
    const button = screen.getByRole('button')
    fireEvent.click(button)

    await waitFor(() => {
      const menu = document.querySelector('.ant-dropdown-menu')
      expect(menu).toHaveStyle({ maxHeight: '300px' })
    })
  })

  it('applies theme-aware scrollbar styling', () => {
    render(<IconMenu {...defaultProps} />)
    const button = screen.getByRole('button')
    fireEvent.click(button)

    const menu = document.querySelector('.ant-dropdown-menu')
    expect(menu).toHaveStyle({
      scrollbarWidth: 'thin',
      scrollbarColor: `${mockToken.colorTextQuaternary} ${mockToken.colorBgElevated}`,
    })
  })

  it('handles items with different value types (string and number)', async () => {
    const mixedItems = [
      { label: 'Item 1', value: 1 },
      { label: 'Item 2', value: '2' },
    ]
    const onChange = jest.fn()
    render(<IconMenu {...defaultProps} items={mixedItems} onChange={onChange} />)

    const button = screen.getByRole('button')
    fireEvent.click(button)

    await waitFor(() => {
      const menuItems = screen.getAllByRole('menuitem')
      expect(menuItems).toHaveLength(2)
    })

    const menuItems = screen.getAllByRole('menuitem')
    fireEvent.click(menuItems[1])

    expect(onChange).toHaveBeenCalledWith('2')
  })

  it('does not call onChange when onChange is not provided', async () => {
    render(<IconMenu {...defaultProps} onChange={undefined} />)

    const button = screen.getByRole('button')
    fireEvent.click(button)

    await waitFor(() => {
      const menuItems = screen.getAllByRole('menuitem')
      expect(menuItems).toHaveLength(3)
    })

    const menuItems = screen.getAllByRole('menuitem')
    // Should not throw error when clicked
    expect(() => fireEvent.click(menuItems[0])).not.toThrow()
  })
})
