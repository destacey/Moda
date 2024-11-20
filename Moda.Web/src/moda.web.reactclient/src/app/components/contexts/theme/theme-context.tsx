import { createContext, useEffect, useState } from 'react'
import { useLocalStorageState } from '@/src/app/hooks'
import { ConfigProvider, ThemeConfig, theme } from 'antd'
import lightTheme from '@/src/config/theme/light-theme'
import darkTheme from '@/src/config/theme/dark-theme'
import { ThemeContextType, ThemeName } from './types'

export const ThemeContext = createContext<ThemeContextType | null>(null)

export const ThemeProvider = ({ children }) => {
  const { token } = theme.useToken()
  const [currentThemeName, setCurrentThemeName] =
    useLocalStorageState<ThemeName>('modaTheme', 'light')
  const [currentTheme, setCurrentTheme] = useState<ThemeConfig>(lightTheme)
  const [agGridTheme, setAgGridTheme] = useState('ag-theme-balham')
  const [badgeColor, setBadgeColor] = useState<string>(null)
  const [antDesignChartsTheme, setAntDesignChartsTheme] = useState('classic')

  useEffect(() => {
    setCurrentTheme(currentThemeName === 'light' ? lightTheme : darkTheme)
    setAgGridTheme(
      currentThemeName === 'light' ? 'ag-theme-balham' : 'ag-theme-balham-dark',
    )
    setBadgeColor(
      currentThemeName === 'light' ? token.colorPrimary : token.colorPrimary, // token.colorBorderBg, // TODO: something broke in and color is not working the same way (see: objective count badge)
    )
    setAntDesignChartsTheme(
      currentThemeName === 'light' ? 'classic' : 'classicDark',
    )
  }, [currentThemeName, token.colorBorderBg, token.colorPrimary])

  return (
    <ThemeContext.Provider
      value={{
        currentThemeName,
        setCurrentThemeName,
        agGridTheme,
        token,
        badgeColor,
        antDesignChartsTheme,
      }}
    >
      <ConfigProvider theme={currentTheme}>
        <div data-theme={currentThemeName}>{children}</div>
      </ConfigProvider>
    </ThemeContext.Provider>
  )
}
