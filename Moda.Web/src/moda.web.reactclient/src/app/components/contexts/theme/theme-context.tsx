import { createContext, useEffect, useState } from 'react'
import { useLocalStorageState } from '@/src/app/hooks'
import { ConfigProvider, ThemeConfig, theme } from 'antd'
import lightTheme from '@/src/config/theme/light-theme'
import darkTheme from '@/src/config/theme/dark-theme'
import { ThemeContextType } from './types'

export const ThemeContext = createContext<ThemeContextType | null>(null)

export const ThemeProvider = ({ children }) => {
  const [currentThemeName, setCurrentThemeName] = useLocalStorageState(
    'modaTheme',
    'light'
  )
  const [currentTheme, setCurrentTheme] = useState<ThemeConfig>(undefined)
  const [appBarColor, setAppBarColor] = useState('')
  const [agGridTheme, setAgGridTheme] = useState('')
  const { token } = theme.useToken()

  useEffect(() => {
    setCurrentTheme(currentThemeName === 'light' ? lightTheme : darkTheme)
    setAppBarColor(currentThemeName === 'light' ? '#2196f3' : '#262a2c')
    setAgGridTheme(
      currentThemeName === 'light' ? 'ag-theme-balham' : 'ag-theme-balham-dark'
    )
  }, [currentThemeName])

  return (
    <ThemeContext.Provider
      value={{
        currentThemeName,
        setCurrentThemeName,
        appBarColor,
        agGridTheme,
        token,
      }}
    >
      <ConfigProvider theme={currentTheme}>{children}</ConfigProvider>
    </ThemeContext.Provider>
  )
}
