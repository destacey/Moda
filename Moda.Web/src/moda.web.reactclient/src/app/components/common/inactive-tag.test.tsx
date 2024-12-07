import { render, screen } from '@testing-library/react'
import InactiveTag from './inactive-tag'

// Mock the useTheme hook
jest.mock('../contexts/theme', () => ({
  __esModule: true,
  default: () => ({
    token: {
      colorWarning: '#faad14',
    },
  }),
}))

describe('InactiveTag', () => {
  it('should return null when isActive is true', () => {
    const { container } = render(<InactiveTag isActive={true} />)
    expect(container.firstChild).toBeNull()
  })

  it('should return null when isActive is undefined', () => {
    const { container } = render(<InactiveTag isActive={undefined} />)
    expect(container.firstChild).toBeNull()
  })

  it('should return null when isActive is null', () => {
    const { container } = render(<InactiveTag isActive={null} />)
    expect(container.firstChild).toBeNull()
  })

  it('should render Inactive tag when isActive is false', () => {
    render(<InactiveTag isActive={false} />)
    const tag = screen.getByText('Inactive')
    expect(tag).toBeInTheDocument()
  })

  it('should use correct color from theme token', () => {
    render(<InactiveTag isActive={false} />)
    const tag = screen.getByText('Inactive')
    expect(tag).toHaveStyle({ backgroundColor: '#faad14' })
  })
})
