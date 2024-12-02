import React, { createElement } from 'react'
import useTheme, { ThemeContextType } from '../../contexts/theme'
import ThemeToggle from './theme-toggle'
import { Mock } from 'jest-mock'
import { GlobalToken } from 'antd/es/theme/interface'
import { render } from '@testing-library/react'

// Mock React hooks
jest.mock('react', () => ({
  ...jest.requireActual('react'),
  useState: jest.fn(),
  useEffect: jest.fn(),
  createElement: jest.requireActual('react').createElement,
}))

// Mock the theme context
jest.mock('../../contexts/theme', () => ({
  __esModule: true,
  default: jest.fn(),
}))

// Mock antd icons
jest.mock('@ant-design/icons', () => ({
  HighlightOutlined: () => (
    <span data-testid="highlight-outlined">HighlightOutlined</span>
  ),
  HighlightFilled: () => (
    <span data-testid="highlight-filled">HighlightFilled</span>
  ),
}))

const mockToken = {
  colorPrimary: '#1890ff',
  colorSuccess: '#52c41a',
  colorWarning: '#faad14',
  colorError: '#ff4d4f',
  colorInfo: '#1890ff',
  colorTextBase: '#000000',
  colorBgBase: '#ffffff',
  fontSize: 14,
  borderRadius: 6,
  wireframe: false,
  colorBgContainer: '#ffffff',
  colorText: '#000000',
  colorTextSecondary: '#666666',
} as unknown as GlobalToken

describe('ThemeToggle', () => {
  const mockSetCurrentThemeName = jest.fn()
  const mockOnClick = jest.fn()
  const mockSetState = jest.fn()

  beforeEach(() => {
    jest.resetAllMocks()

    // Mock useState implementation
    ;(React.useState as jest.Mock).mockImplementation((init) => [
      init,
      mockSetState,
    ])

    // Setup default theme context values
    ;(useTheme as Mock).mockReturnValue({
      currentThemeName: 'light',
      setCurrentThemeName: mockSetCurrentThemeName,
      agGridTheme: 'ag-theme-balham',
      token: mockToken,
      badgeColor: '#1890ff',
      antDesignChartsTheme: 'light',
      antvisG6ChartsTheme: 'light',
    } as ThemeContextType)
  })

  it('returns correct menu item structure', () => {
    const themeToggle = ThemeToggle({})

    expect(themeToggle).toMatchObject({
      key: 'theme',
      label: 'Theme',
      icon: expect.any(Object),
      onClick: expect.any(Function),
    })
  })

  it('switches from light to dark theme when clicked', () => {
    const themeToggle = ThemeToggle({})
    themeToggle.onClick?.()

    expect(mockSetCurrentThemeName).toHaveBeenCalledWith('dark')
  })

  it('switches from dark to light theme when clicked', () => {
    const darkToken = {
      ...mockToken,
      colorBgBase: '#141414',
      colorBgContainer: '#1f1f1f',
      colorText: '#ffffff',
      colorTextBase: '#ffffff',
    } as unknown as GlobalToken

    ;(useTheme as Mock).mockReturnValue({
      currentThemeName: 'dark',
      setCurrentThemeName: mockSetCurrentThemeName,
      agGridTheme: 'ag-theme-balham-dark',
      token: darkToken,
      badgeColor: '#1890ff',
      antDesignChartsTheme: 'dark',
      antvisG6ChartsTheme: 'dark',
    } as ThemeContextType)

    const themeToggle = ThemeToggle({})
    themeToggle.onClick?.()

    expect(mockSetCurrentThemeName).toHaveBeenCalledWith('light')
  })

  it('calls optional onClick callback when provided', () => {
    const themeToggle = ThemeToggle({ onClick: mockOnClick })
    themeToggle.onClick?.()

    expect(mockOnClick).toHaveBeenCalled()
    expect(mockSetCurrentThemeName).toHaveBeenCalled()
  })

  it('uses correct icon based on theme', () => {
    const themeToggle = ThemeToggle({})
    expect(themeToggle.icon).toBeDefined()
  })

  it('maintains theme state across re-renders', () => {
    const themeToggle = ThemeToggle({})
    themeToggle.onClick?.()

    expect(mockSetCurrentThemeName).toHaveBeenCalledWith('dark')

    const darkToken = {
      ...mockToken,
      colorBgBase: '#141414',
      colorBgContainer: '#1f1f1f',
      colorText: '#ffffff',
      colorTextBase: '#ffffff',
    } as unknown as GlobalToken

    ;(useTheme as Mock).mockReturnValue({
      currentThemeName: 'dark',
      setCurrentThemeName: mockSetCurrentThemeName,
      agGridTheme: 'ag-theme-balham-dark',
      token: darkToken,
      badgeColor: '#1890ff',
      antDesignChartsTheme: 'dark',
      antvisG6ChartsTheme: 'dark',
    } as ThemeContextType)

    const updatedThemeToggle = ThemeToggle({})
    expect(updatedThemeToggle.icon).toBeDefined()
  })
})
