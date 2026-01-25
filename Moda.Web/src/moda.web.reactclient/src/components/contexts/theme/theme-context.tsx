import { createContext, useEffect, useMemo, useState } from 'react'
import {
  themeBalham,
  colorSchemeDark,
  type Theme as AgGridTheme,
} from 'ag-grid-community'
import { useLocalStorageState } from '@/src/hooks'
import { ConfigProvider, theme } from 'antd'
import lightTheme from '@/src/config/theme/light-theme'
import darkTheme from '@/src/config/theme/dark-theme'
import { ThemeContextType, ThemeName } from './types'

export const ThemeContext = createContext<ThemeContextType | null>(null)

const agGridLightTheme = themeBalham
const agGridDarkTheme = themeBalham.withPart(colorSchemeDark)

export const ThemeProvider = ({ children }: { children: React.ReactNode }) => {
  const [currentThemeName, setCurrentThemeName] =
    useLocalStorageState<ThemeName>('modaTheme', 'light')

  const [agGridTheme, setAgGridTheme] = useState<AgGridTheme>(agGridLightTheme)
  const [badgeColor, setBadgeColor] = useState<string | null>(null)
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

    useEffect(() => {
      // Set data-theme on document.documentElement (html element) for global theme access
      // This includes portaled elements like drawers that render outside the theme context
      document.documentElement.setAttribute('data-theme', currentThemeName)
      // currentThemeName is needed as dependency - when it changes in parent state,
      // this component re-renders and we need to update the DOM attribute
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [currentThemeName])

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
        {children}
      </ThemeContext.Provider>
    )
  }

  return (
    <ConfigProvider theme={currentTheme}>
      <ThemeContent>{children}</ThemeContent>
    </ConfigProvider>
  )
}
