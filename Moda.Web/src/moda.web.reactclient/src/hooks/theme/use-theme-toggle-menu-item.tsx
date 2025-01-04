'use client'

import { HighlightFilled, HighlightOutlined } from '@ant-design/icons'
import useTheme from '../../components/contexts/theme'

const useThemeToggleMenuItem = () => {
  const { setCurrentThemeName, currentThemeName } = useTheme()

  const toggleTheme = () => {
    const newTheme = currentThemeName === 'light' ? 'dark' : 'light'
    setCurrentThemeName(newTheme)
  }

  return {
    key: 'theme',
    label: 'Theme',
    icon:
      currentThemeName === 'light' ? (
        <HighlightOutlined />
      ) : (
        <HighlightFilled />
      ),
    onClick: toggleTheme,
  }
}

export default useThemeToggleMenuItem
