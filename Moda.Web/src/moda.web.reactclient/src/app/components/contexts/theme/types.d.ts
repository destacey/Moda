import { GlobalToken } from 'antd'

export interface ThemeContextType {
  currentThemeName: string
  setCurrentThemeName: (themeName: string) => void
  appBarColor: string
  agGridTheme: string
  token: GlobalToken
}
