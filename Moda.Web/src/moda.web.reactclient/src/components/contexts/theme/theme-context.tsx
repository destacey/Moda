import { createContext, useEffect, useMemo, useState } from 'react'
import { themeBalham, colorSchemeDark, type Theme as AgGridTheme } from 'ag-grid-community'
import { useLocalStorageState } from '@/src/hooks'
import { ConfigProvider, theme } from 'antd'
import lightTheme from '@/src/config/theme/light-theme'
import darkTheme from '@/src/config/theme/dark-theme'
import { ThemeContextType, ThemeName } from './types'

export const ThemeContext = createContext<ThemeContextType | null>(null)

const agGridLightTheme = themeBalham;
const agGridDarkTheme = themeBalham.withPart(colorSchemeDark);

export const ThemeProvider = ({ children }: { children: React.ReactNode }) => {
  const [currentThemeName, setCurrentThemeName] =
    useLocalStorageState<ThemeName>('modaTheme', 'light')

  const [agGridTheme, setAgGridTheme] = useState<AgGridTheme>(agGridLightTheme)
  const [badgeColor, setBadgeColor] = useState<string>(null)
  const [antDesignChartsTheme, setAntDesignChartsTheme] = useState('classic')
  const [antvisG6ChartsTheme, setAntvisG6ChartsTheme] = useState('light')

  // Create the theme configuration
  const currentTheme = useMemo(() => {
    const baseTheme = currentThemeName === 'light' ? lightTheme : darkTheme
    return {
      ...baseTheme,
      token: {
        ...theme.defaultConfig.token,
        ...baseTheme.token,
      },
    }
  }, [currentThemeName])

  useEffect(() => {
    setAgGridTheme(
      currentThemeName === 'light' ? agGridLightTheme : agGridDarkTheme,
    )
    setAntDesignChartsTheme(
      currentThemeName === 'light' ? 'classic' : 'classicDark',
    )
    setAntvisG6ChartsTheme(currentThemeName === 'light' ? 'light' : 'dark')
  }, [currentThemeName])

  // Use theme.useToken() inside ConfigProvider
  const ThemeContent = ({ children }: { children: React.ReactNode }) => {
    const { token } = theme.useToken()

    useEffect(() => {
      setBadgeColor(token.colorPrimary)
    }, [token.colorPrimary])

    const themeContextValue = useMemo(
      () => ({
        currentThemeName,
        setCurrentThemeName,
        agGridTheme,
        token,
        badgeColor,
        antDesignChartsTheme,
        antvisG6ChartsTheme,
      }),
      [token],
    )

    return (
      <ThemeContext.Provider value={themeContextValue}>
        <div data-theme={currentThemeName}>{children}</div>
      </ThemeContext.Provider>
    )
  }

  return (
    <ConfigProvider theme={currentTheme}>
      <ThemeContent>{children}</ThemeContent>
    </ConfigProvider>
  )
}
