'use client'

import '../../styles/globals.css'
import React from 'react'
import { Layout, Menu } from 'antd'
import { AuthenticatedTemplate } from '@azure/msal-react'
import AppHeader from './components/common/app-header'
import AppMenu from './components/common/menu'
import AppBreadcrumb from './components/common/app-breadcrumb'
import { ThemeProvider } from './components/contexts/theme'
import { AuthProvider } from './components/contexts/auth'
import { BreadcrumbsProvider } from './components/contexts/breadcrumbs'
import { MenuToggleProvider } from './components/contexts/menu-toggle'
import { QueryClient, QueryClientProvider } from 'react-query'

const { Content } = Layout

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,
    },
  },
})

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body>
        <AuthProvider>
          <ThemeProvider>
            <AuthenticatedTemplate>
              <QueryClientProvider client={queryClient}>
                <MenuToggleProvider>
                  <Layout className="layout" style={{ minHeight: '100vh' }}>
                    <AppHeader />
                    <Layout>
                      <AppMenu />
                      <Layout style={{ padding: '0 24px 24px' }}>
                        <BreadcrumbsProvider>
                          <AppBreadcrumb />
                          <Content
                            style={{
                              margin: 0,
                              minHeight: 280,
                            }}
                          >
                            {children}
                          </Content>
                        </BreadcrumbsProvider>
                      </Layout>
                    </Layout>
                  </Layout>
                </MenuToggleProvider>
              </QueryClientProvider>
            </AuthenticatedTemplate>
          </ThemeProvider>
        </AuthProvider>
      </body>
    </html>
  )
}
