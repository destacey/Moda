import { type Theme as AgGridTheme } from 'ag-grid-community'
import { GlobalToken } from 'antd'

type ThemeName = 'light' | 'dark'

export interface ThemeContextType {
  currentThemeName: ThemeName
  setCurrentThemeName: (themeName: ThemeName) => void
  agGridTheme: AgGridTheme
  token: GlobalToken
  badgeColor: string
  antDesignChartsTheme: string
  antvisG6ChartsTheme: string
}
