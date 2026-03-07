'use client'

import '@/styles/globals.css'
import React, { memo, PropsWithChildren, useMemo, useSyncExternalStore } from 'react'
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
import {
  AuthenticatedTemplate,
  MsalProvider,
  UnauthenticatedTemplate,
  useMsal,
} from '@azure/msal-react'
import { InteractionStatus } from '@azure/msal-browser'
import { MessageProvider } from '../components/contexts/messaging'
import LoginPage from './login/page'
import LogoutPage from './logout/page'
import logoutStyles from './logout/page.module.css'
// Note: LogoutPage is only used in UnauthenticatedView (after MSAL determines auth state)
import { usePathname } from 'next/navigation'
import { isLocalAuthActive } from '../services/clients'

const { Content } = Layout

const inter = Inter({ subsets: ['latin'] })

/**
 * Shows the appropriate page for unauthenticated users based on route
 */
const UnauthenticatedView = () => {
  const pathname = usePathname()

  // Show logout page if on logout route
  if (pathname === '/logout') {
    return <LogoutPage />
  }

  // If locally authenticated, don't show login page — AuthProvider handles it
  if (isLocalAuthActive()) {
    return null
  }

  return <LoginPage />
}

/**
 * Shows loading state during MSAL initialization (before auth state is determined)
 * Must render the same markup as SsrFallback to avoid hydration mismatch
 */
const MsalInitializingView = () => {
  const { inProgress } = useMsal()
  const pathname = usePathname()
  const isLogoutRoute = pathname === '/logout'

  // Only show during MSAL startup - once ready, Auth/Unauth templates take over
  // Skip for local auth users (MSAL isn't relevant) and users with no cached accounts
  let hasCachedAccounts = false
  try {
    hasCachedAccounts = (msalInstance?.getAllAccounts().length ?? 0) > 0
  } catch {
    // MSAL may not be fully initialized yet — treat as no cached accounts
  }
  if (
    inProgress === InteractionStatus.Startup &&
    !isLocalAuthActive() &&
    hasCachedAccounts
  ) {
    // Render same markup as SsrFallback to avoid hydration mismatch
    return (
      <div className={logoutStyles.pageBackground}>
        <div className={`${logoutStyles.bgCircle} ${logoutStyles.bgCircle1}`} />
        <div className={`${logoutStyles.bgCircle} ${logoutStyles.bgCircle2}`} />
        <div className={`${logoutStyles.bgCircle} ${logoutStyles.bgCircle3}`} />
        <div className={logoutStyles.card}>
          <div className={logoutStyles.content}>
            <div className={logoutStyles.logo}>
              {/* eslint-disable-next-line @next/next/no-img-element */}
              <img
                src="/moda-icon.png"
                alt="Moda"
                className={logoutStyles.logoIcon}
              />
              <div className={logoutStyles.logoDivider} />
              <span className={logoutStyles.logoText}>moda</span>
            </div>
            <div className={logoutStyles.spinnerWrapper}>
              <LoadingSpinner />
            </div>
            <h1 className={logoutStyles.title}>
              {isLogoutRoute ? 'Signing out...' : 'Loading...'}
            </h1>
            <p className={logoutStyles.subtitle}>
              {isLogoutRoute
                ? 'Please wait while we sign you out of your account.'
                : 'Please wait while we initialize the application.'}
            </p>
          </div>
        </div>
      </div>
    )
  }

  return null
}

const AppContent = memo(({ children }: PropsWithChildren) => {
  const screens = Grid.useBreakpoint()
  const isMobile = useMemo(() => !screens.md, [screens.md]) // md breakpoint is 768px in Ant Design

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

/**
 * Loading spinner component for SSR fallback
 */
const LoadingSpinner = () => (
  <svg
    className={logoutStyles.spinner}
    width="48"
    height="48"
    viewBox="0 0 24 24"
    fill="none"
  >
    <circle
      cx="12"
      cy="12"
      r="10"
      stroke="currentColor"
      strokeWidth="3"
      strokeLinecap="round"
      strokeDasharray="31.4 31.4"
    />
  </svg>
)

/**
 * SSR fallback when msalInstance is null (server-side).
 * Renders nothing — the client will immediately hydrate with the MsalProvider branch,
 * which handles its own loading state via MsalInitializingView.
 */
const SsrFallback = () => null

/**
 * Shared UI provider stack (theme, menu, Ant Design App, messaging).
 * Extracted to avoid duplicating across local auth and MSAL branches.
 */
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
 * Auth gate that renders the full app for locally-authenticated users
 * or falls through to MSAL Authenticated/Unauthenticated templates.
 */
// useSyncExternalStore helpers for reading localStorage without SSR mismatch.
// subscribe is a no-op because we only need the value once on mount — the
// component tree fully remounts on login/logout via window.location.href.
const subscribeNoop = () => () => {}
const getLocalAuthSnapshot = () => isLocalAuthActive()
const getLocalAuthServerSnapshot = () => false

const LocalOrMsalAuthGate = ({ children }: PropsWithChildren) => {
  const localAuth = useSyncExternalStore(
    subscribeNoop,
    getLocalAuthSnapshot,
    getLocalAuthServerSnapshot,
  )

  if (localAuth) {
    return (
      <AppProviders>
        <AuthProvider>
          <AppContent>{children}</AppContent>
        </AuthProvider>
      </AppProviders>
    )
  }

  return (
    <>
      <AuthenticatedTemplate>
        <AppProviders>
          <AuthProvider>
            <AppContent>{children}</AppContent>
          </AuthProvider>
        </AppProviders>
      </AuthenticatedTemplate>
      <UnauthenticatedTemplate>
        <UnauthenticatedView />
      </UnauthenticatedTemplate>
    </>
  )
}

const RootLayout = ({ children }: React.PropsWithChildren) => {
  // Guard against SSR where msalInstance is null - show appropriate page based on route
  if (!msalInstance) {
    return (
      <html lang="en">
        <head>
          <meta
            name="viewport"
            content="width=device-width, initial-scale=1, viewport-fit=cover"
          />
        </head>
        <body className={inter.className}>
          <SsrFallback />
        </body>
      </html>
    )
  }

  return (
    <html lang="en">
      <head>
        <meta
          name="viewport"
          content="width=device-width, initial-scale=1, viewport-fit=cover"
        />
      </head>
      <body className={inter.className}>
        <AntdRegistry>
          <Provider store={store}>
            <MsalProvider instance={msalInstance}>
              <LocalOrMsalAuthGate>
                {children}
              </LocalOrMsalAuthGate>
              <MsalInitializingView />
            </MsalProvider>
          </Provider>
        </AntdRegistry>
      </body>
    </html>
  )
}

export default RootLayout
