import { GlobalToken } from 'antd'

type ThemeName = 'light' | 'dark'

export interface ThemeContextType {
  currentThemeName: ThemeName
  setCurrentThemeName: (themeName: ThemeName) => void
  agGridTheme: string
  token: GlobalToken
  badgeColor: string
}
