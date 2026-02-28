import { createContext, ReactNode, useEffect, useMemo } from 'react'
import {
  themeBalham,
  colorSchemeDark,
} from 'ag-grid-community'
import { useLocalStorageState } from '@/src/hooks'
import { ConfigProvider, theme } from 'antd'
import lightTheme from '@/src/config/theme/light-theme'
import darkTheme from '@/src/config/theme/dark-theme'
import { ThemeContextType, ThemeName } from './types'

export const ThemeContext = createContext<ThemeContextType | null>(null)

const agGridLightTheme = themeBalham
const agGridDarkTheme = themeBalham.withPart(colorSchemeDark)

export const ThemeProvider = ({ children }: { children: ReactNode }) => {
  const [currentThemeName, setCurrentThemeName] =
    useLocalStorageState<ThemeName>('modaTheme', 'light')

  const agGridTheme =
    currentThemeName === 'light' ? agGridLightTheme : agGridDarkTheme
  const antDesignChartsTheme =
    currentThemeName === 'light' ? 'classic' : 'classicDark'
  const antvisG6ChartsTheme = currentThemeName === 'light' ? 'light' : 'dark'

  // Create the theme configuration
  const currentTheme = useMemo(
    () => (currentThemeName === 'light' ? lightTheme : darkTheme),
    [currentThemeName],
  )

  // Use theme.useToken() inside ConfigProvider
  const ThemeContent = ({ children }: { children: ReactNode }) => {
    const { token } = theme.useToken()
    const badgeColor = token.colorPrimary

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
      // eslint-disable-next-line react-hooks/exhaustive-deps
      [
        currentThemeName,
        setCurrentThemeName,
        agGridTheme,
        token,
        badgeColor,
        antDesignChartsTheme,
        antvisG6ChartsTheme,
      ],
    )

    return (
      <ThemeContext.Provider value={themeContextValue}>
        {children}
      </ThemeContext.Provider>
    )
  }

  return (
    <ConfigProvider theme={currentTheme} modal={{ closable: true }}>
      <ThemeContent>{children}</ThemeContent>
    </ConfigProvider>
  )
}
