import useTheme from '../../contexts/theme'
import useThemeToggleMenuItem from './use-theme-toggle-menu-item'
import { Mock } from 'jest-mock'

jest.mock('../../contexts/theme', () => ({
  __esModule: true,
  default: jest.fn(),
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
}

const mockThemeContext = {
  currentThemeName: 'light',
  setCurrentThemeName: jest.fn(),
  agGridTheme: 'ag-theme-balham',
  token: mockToken as any,
  badgeColor: '#1890ff',
  antDesignChartsTheme: 'classic',
  antvisG6ChartsTheme: 'light',
}

describe('useThemeToggleMenuItem', () => {
  beforeEach(() => {
    jest.resetAllMocks()
    ;(useTheme as Mock).mockReturnValue(mockThemeContext)
  })

  it('returns correct menu item structure', () => {
    const themeToggle = useThemeToggleMenuItem()
    expect(themeToggle).toMatchObject({
      key: 'theme',
      label: 'Theme',
      icon: expect.any(Object),
      onClick: expect.any(Function),
    })
  })

  it('toggles from light to dark theme when clicked', () => {
    const themeToggle = useThemeToggleMenuItem()
    themeToggle.onClick()

    expect(mockThemeContext.setCurrentThemeName).toHaveBeenCalledWith('dark')
  })

  it('toggles from dark to light theme when clicked', () => {
    ;(useTheme as Mock).mockReturnValue({
      ...mockThemeContext,
      currentThemeName: 'dark',
    })

    const themeToggle = useThemeToggleMenuItem()
    themeToggle.onClick()

    expect(mockThemeContext.setCurrentThemeName).toHaveBeenCalledWith('light')
  })

  it('uses the correct icon for the light theme', () => {
    const themeToggle = useThemeToggleMenuItem()
    expect(themeToggle.icon).toMatchSnapshot()
  })

  it('uses the correct icon for the dark theme', () => {
    ;(useTheme as Mock).mockReturnValue({
      ...mockThemeContext,
      currentThemeName: 'dark',
    })

    const themeToggle = useThemeToggleMenuItem()
    expect(themeToggle.icon).toMatchSnapshot()
  })
})
