import {
  createContext,
  ReactNode,
  useEffect,
  useLayoutEffect,
  useMemo,
  useRef,
} from 'react'
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
  const hasMountedRef = useRef(false)
  const transitionTimeoutRef = useRef<number | null>(null)

  const agGridTheme =
    currentThemeName === 'light' ? agGridLightTheme : agGridDarkTheme
  const antDesignChartsTheme =
    currentThemeName === 'light' ? 'classic' : 'classicDark'
  const antvisG6ChartsTheme = currentThemeName === 'light' ? 'light' : 'dark'

  const currentTheme = currentThemeName === 'light' ? lightTheme : darkTheme

  useLayoutEffect(() => {
    const root = document.documentElement
    root.setAttribute('data-theme', currentThemeName)

    // Skip animation for first paint; only animate explicit theme changes.
    if (!hasMountedRef.current) {
      hasMountedRef.current = true
      return
    }

    root.classList.add('theme-transitioning')
    if (transitionTimeoutRef.current) {
      window.clearTimeout(transitionTimeoutRef.current)
    }
    transitionTimeoutRef.current = window.setTimeout(() => {
      root.classList.remove('theme-transitioning')
      transitionTimeoutRef.current = null
    }, 350)
  }, [currentThemeName])

  useEffect(
    () => () => {
      if (transitionTimeoutRef.current) {
        window.clearTimeout(transitionTimeoutRef.current)
      }
      document.documentElement.classList.remove('theme-transitioning')
    },
    [],
  )

  return (
    <ConfigProvider theme={currentTheme} modal={{ closable: true, mask: { closable: false } }}>
      <ThemeTokenProvider
        currentThemeName={currentThemeName}
        setCurrentThemeName={setCurrentThemeName}
        agGridTheme={agGridTheme}
        antDesignChartsTheme={antDesignChartsTheme}
        antvisG6ChartsTheme={antvisG6ChartsTheme}
      >
        {children}
      </ThemeTokenProvider>
    </ConfigProvider>
  )
}

interface ThemeTokenProviderProps {
  children: ReactNode
  currentThemeName: ThemeName
  setCurrentThemeName: (value: ThemeName) => void
  agGridTheme: typeof agGridLightTheme
  antDesignChartsTheme: string
  antvisG6ChartsTheme: string
}

const ThemeTokenProvider = ({
  children,
  currentThemeName,
  setCurrentThemeName,
  agGridTheme,
  antDesignChartsTheme,
  antvisG6ChartsTheme,
}: ThemeTokenProviderProps) => {
  const { token } = theme.useToken()
  const badgeColor = token.colorPrimary

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
