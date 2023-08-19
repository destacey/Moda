import { createContext, useEffect, useState } from 'react'
import { useLocalStorageState } from '@/src/app/hooks'
import { ConfigProvider, ThemeConfig, theme } from 'antd'
import lightTheme from '@/src/config/theme/light-theme'
import darkTheme from '@/src/config/theme/dark-theme'
import { ThemeContextType, ThemeName } from './types'

export const ThemeContext = createContext<ThemeContextType | null>(null)

export const ThemeProvider = ({ children }) => {
  const [currentThemeName, setCurrentThemeName] =
    useLocalStorageState<ThemeName>('modaTheme', 'light')
  const [currentTheme, setCurrentTheme] = useState<ThemeConfig>(lightTheme)
  const [agGridTheme, setAgGridTheme] = useState('ag-theme-balham')
  const { token } = theme.useToken()

  useEffect(() => {
    setCurrentTheme(currentThemeName === 'light' ? lightTheme : darkTheme)
    setAgGridTheme(
      currentThemeName === 'light' ? 'ag-theme-balham' : 'ag-theme-balham-dark',
    )
  }, [currentThemeName])

  return (
    <ThemeContext.Provider
      value={{
        currentThemeName,
        setCurrentThemeName,
        agGridTheme,
        token,
      }}
    >
      <ConfigProvider theme={currentTheme}>{children}</ConfigProvider>
    </ThemeContext.Provider>
  )
}
