'use client'

import '../../styles/globals.css'
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-balham.css'
import React from 'react'
import { Layout } from 'antd'
import { AuthenticatedTemplate } from '@azure/msal-react'
import AppHeader from './components/common/app-header'
import AppMenu from './components/common/menu'
import AppBreadcrumb from './components/common/app-breadcrumb'
import { ThemeProvider } from './components/contexts/theme'
import { AuthProvider } from './components/contexts/auth'
import { BreadcrumbsProvider } from './components/contexts/breadcrumbs'
import { MenuToggleProvider } from './components/contexts/menu-toggle'
import LoadingAccount from './components/common/loading-account'

const { Content } = Layout

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
              <MenuToggleProvider>
                <Layout>
                  <AppHeader />
                  <LoadingAccount>
                    <Layout>
                      <AppMenu />
                      <Layout style={{ padding: '0 24px 24px' }}>
                        <BreadcrumbsProvider>
                          <AppBreadcrumb />
                          <Content
                            style={{
                              margin: 0,
                              height: '100vh',
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
            </AuthenticatedTemplate>
          </ThemeProvider>
        </AuthProvider>
      </body>
    </html>
  )
}
