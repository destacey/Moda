import { GlobalToken } from 'antd'

type ThemeName = 'light' | 'dark'

export interface ThemeContextType {
  currentThemeName: ThemeName
  setCurrentThemeName: (themeName: ThemeName) => void
  appBarColor: string
  agGridTheme: string
  token: GlobalToken
}
