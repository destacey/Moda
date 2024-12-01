'use client'

import { createElement, useEffect, useState } from 'react'
import { HighlightFilled, HighlightOutlined } from '@ant-design/icons'
import useTheme from '../../contexts/theme'

interface ThemeToggleProps {
  onClick?: () => void
}

const ThemeToggle = ({ onClick }: ThemeToggleProps) => {
  const { setCurrentThemeName, currentThemeName } = useTheme()
  const [themeIcon, setThemeIcon] = useState(createElement(HighlightOutlined))

  const toggleTheme = () => {
    const newTheme = currentThemeName === 'light' ? 'dark' : 'light'
    setCurrentThemeName(newTheme)
    onClick?.()
  }

  useEffect(() => {
    setThemeIcon(
      createElement(
        currentThemeName === 'light' ? HighlightOutlined : HighlightFilled,
      ),
    )
  }, [currentThemeName])

  return {
    key: 'theme',
    label: 'Theme',
    icon: themeIcon,
    onClick: toggleTheme,
  }
}

export default ThemeToggle
