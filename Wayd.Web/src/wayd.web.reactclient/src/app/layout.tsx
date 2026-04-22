'use client'

import '@/styles/globals.css'
import React, {
  memo,
  PropsWithChildren,
  useEffect,
  useSyncExternalStore,
} from 'react'
import { Provider } from 'react-redux'
import { Inter } from 'next/font/google'
import { App, Grid, Layout } from 'antd'
import { store } from '../store'
import AppHeader from './_components/app-header'
import AppSideNav from './_components/menu/app-side-nav'
import AppBreadcrumb from './_components/app-breadcrumb'
import { ThemeProvider } from '../components/contexts/theme'
import { MenuToggleProvider } from '../components/contexts/menu-toggle'
import { AntdRegistry } from '@ant-design/nextjs-registry'
import { AuthProvider, msalInstance } from '../components/contexts/auth'
import { MsalProvider } from '@azure/msal-react'
import { MessageProvider } from '../components/contexts/messaging'
import LoginPage from './login/page'
import LogoutPage from './logout/page'
import { usePathname, useRouter } from 'next/navigation'
import { isAuthActive } from '../services/clients'

const { Content } = Layout
const { useBreakpoint } = Grid

const inter = Inter({ subsets: ['latin'] })

const RETURN_URL_KEY = 'wayd.returnUrl'

const AppContent = memo(({ children }: PropsWithChildren) => {
  const screens = useBreakpoint()
  const isMobile = !screens.md
  const router = useRouter()

  // After authentication, redirect to the originally requested URL if one was stored.
  useEffect(() => {
    const returnUrl = sessionStorage.getItem(RETURN_URL_KEY)
    if (returnUrl) {
      sessionStorage.removeItem(RETURN_URL_KEY)
      router.replace(returnUrl)
    }
  }, [router])

  return (
    <Layout>
      <AppHeader />
      <Layout hasSider className="app-main-layout">
        <AppSideNav isMobile={isMobile} />
        <Content className="app-main-content">
          <AppBreadcrumb />
          {children}
        </Content>
      </Layout>
    </Layout>
  )
})

AppContent.displayName = 'AppContent'

const SsrFallback = () => null

const AppProviders = ({ children }: PropsWithChildren) => (
  <ThemeProvider>
    <MenuToggleProvider>
      <App>
        <MessageProvider>{children}</MessageProvider>
      </App>
    </MenuToggleProvider>
  </ThemeProvider>
)

/**
 * Auth gate — single-check on whether a Wayd JWT is in storage. If yes,
 * render the app; otherwise render the login page (or logout on /logout).
 *
 * MSAL state is deliberately not part of the gate decision. Post-PR 3.2 the
 * Wayd JWT *is* the session; MSAL is just the mechanism the login page uses
 * to acquire an initial token via /api/auth/exchange. Treating MSAL state as
 * part of the gate would conflate "has an Entra session cookie" with "logged
 * in to Wayd" — the source of the logout re-login race that PR 3.2's earlier
 * revisions hit.
 */
const subscribeNoop = () => () => {}
const getAuthSnapshot = () => isAuthActive()
const getAuthServerSnapshot = () => false

const AuthGate = ({ children }: PropsWithChildren) => {
  const pathname = usePathname()
  const auth = useSyncExternalStore(
    subscribeNoop,
    getAuthSnapshot,
    getAuthServerSnapshot,
  )

  if (auth) {
    return (
      <AppProviders>
        <AuthProvider>
          <AppContent>{children}</AppContent>
        </AuthProvider>
      </AppProviders>
    )
  }

  // Preserve the intended URL so login can redirect back after authentication.
  if (
    typeof window !== 'undefined' &&
    pathname &&
    pathname !== '/' &&
    pathname !== '/login' &&
    pathname !== '/logout'
  ) {
    sessionStorage.setItem(RETURN_URL_KEY, pathname)
  }

  if (pathname === '/logout') {
    return <LogoutPage />
  }

  return <LoginPage />
}

const RootLayout = ({ children }: React.PropsWithChildren) => {
  // MSAL can't run during SSR (no window). Render the same shell on the server
  // AND on the first client render, then swap to the MsalProvider tree after
  // the mount effect fires. This keeps server HTML and first-paint client HTML
  // identical — required for hydration — while still booting MSAL on the client.
  const [mounted, setMounted] = React.useState(false)
  React.useEffect(() => {
    setMounted(true)
  }, [])

  return (
    <html lang="en">
      <head>
        <meta
          name="viewport"
          content="width=device-width, initial-scale=1, viewport-fit=cover"
        />
        <link rel="apple-touch-icon" href="/wayd-icon.png" />
        <meta name="mobile-web-app-capable" content="yes" />
        <meta name="apple-mobile-web-app-status-bar-style" content="default" />
      </head>
      <body className={inter.className}>
        {mounted && msalInstance ? (
          <AntdRegistry>
            <Provider store={store}>
              <MsalProvider instance={msalInstance}>
                <AuthGate>{children}</AuthGate>
              </MsalProvider>
            </Provider>
          </AntdRegistry>
        ) : (
          <SsrFallback />
        )}
      </body>
    </html>
  )
}

export default RootLayout
