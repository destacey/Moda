import { createContext, useEffect, useMemo, useState } from 'react'
import { useLocalStorageState } from '@/src/app/hooks'
import { ConfigProvider, ThemeConfig, theme } from 'antd'
import lightTheme from '@/src/config/theme/light-theme'
import darkTheme from '@/src/config/theme/dark-theme'
import { ThemeContextType, ThemeName } from './types'

export const ThemeContext = createContext<ThemeContextType | null>(null)

export const ThemeProvider = ({ children }: { children: React.ReactNode }) => {
  const [currentThemeName, setCurrentThemeName] =
    useLocalStorageState<ThemeName>('modaTheme', 'light')

  const [agGridTheme, setAgGridTheme] = useState('ag-theme-balham')
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
      currentThemeName === 'light' ? 'ag-theme-balham' : 'ag-theme-balham-dark',
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
