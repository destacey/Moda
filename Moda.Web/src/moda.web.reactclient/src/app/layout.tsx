'use client'

import '@/styles/globals.css'
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-balham.css'
import React from 'react'
import { Inter } from 'next/font/google'
import StyledComponentsRegistry from '../lib/antd-registry'
import { Layout } from 'antd'
import { AuthenticatedTemplate } from '@azure/msal-react'
import AppHeader from './components/common/app-header'
import AppMenu from './components/common/menu'
import AppBreadcrumb from './components/common/app-breadcrumb'
import { ThemeProvider } from './components/contexts/theme'
import { AuthProvider } from './components/contexts/auth'
import { BreadcrumbsProvider } from './components/contexts/breadcrumbs'
import { MenuToggleProvider } from './components/contexts/menu-toggle'
import { QueryClient, QueryClientProvider } from 'react-query'
import LoadingAccount from './components/common/loading-account'

const { Content } = Layout

const inter = Inter({ subsets: ['latin'] })

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
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
      <body className={inter.className}>
        {/* <StyledComponentsRegistry> */}
        <AuthProvider>
          <ThemeProvider>
            <AuthenticatedTemplate>
              <QueryClientProvider client={queryClient}>
                <MenuToggleProvider>
                  <Layout>
                    <AppHeader />
                    <LoadingAccount>
                      <Layout hasSider style={{ minHeight: '100vh' }}>
                        <AppMenu />
                        <Layout style={{ padding: '0 24px 24px' }}>
                          <BreadcrumbsProvider>
                            <AppBreadcrumb />
                            <Content
                              style={{
                                margin: 0,
                                height: '100%',
                              }}
                            >
                              {children}
                            </Content>
                          </BreadcrumbsProvider>
                        </Layout>
                      </Layout>
                    </LoadingAccount>
                  </Layout>
                </MenuToggleProvider>
              </QueryClientProvider>
            </AuthenticatedTemplate>
          </ThemeProvider>
        </AuthProvider>
        {/* </StyledComponentsRegistry> */}
      </body>
    </html>
  )
}
