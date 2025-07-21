'use client'

import '@/styles/globals.css'
import React, { useEffect, useState } from 'react'
import '@ant-design/v5-patch-for-react-19'
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community'
import { Provider } from 'react-redux'
import { Inter } from 'next/font/google'
import { Layout } from 'antd'
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
} from '@azure/msal-react'
import { LoadingAccount } from '../components/common'
import { MessageProvider } from '../components/contexts/messaging'

const { Content } = Layout

const inter = Inter({ subsets: ['latin'] })

// Register all community features for ag-grid
ModuleRegistry.registerModules([AllCommunityModule])

const RootLayout = ({ children }: React.PropsWithChildren) => {
  const [isMobile, setIsMobile] = useState(false)

  useEffect(() => {
    if (typeof window === 'undefined') return

    const handleResize = () => setIsMobile(window.innerWidth < 768)
    handleResize()

    window.addEventListener('resize', handleResize)
    return () => window.removeEventListener('resize', handleResize)
  }, [])

  return (
    <html lang="en">
      <body className={inter.className}>
        <AntdRegistry>
          <Provider store={store}>
            <MsalProvider instance={msalInstance}>
              <AuthProvider>
                <AuthenticatedTemplate>
                  <ThemeProvider>
                    <MenuToggleProvider>
                      <MessageProvider>
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
                      </MessageProvider>
                    </MenuToggleProvider>
                  </ThemeProvider>
                </AuthenticatedTemplate>
                <UnauthenticatedTemplate>
                  {/* TODO: change this to a login form after the login flow is manual rather than automatic */}
                  <LoadingAccount message="Unauthenticated Moda user.  Redirecting to login..." />
                </UnauthenticatedTemplate>
              </AuthProvider>
            </MsalProvider>
          </Provider>
        </AntdRegistry>
      </body>
    </html>
  )
}

export default RootLayout
