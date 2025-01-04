'use client'

import '@/styles/globals.css'
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-balham.css'
import React from 'react'
import { Provider } from 'react-redux'
import { Inter } from 'next/font/google'
import { Layout } from 'antd'
import { AuthenticatedTemplate } from '@azure/msal-react'
import { store } from '../store'
import AppHeader from './components/common/app-header'
import AppMenu from './components/common/menu'
import AppBreadcrumb from './components/common/app-breadcrumb'
import { ThemeProvider } from './components/contexts/theme'
import { AuthProvider } from './components/contexts/auth'
import { MenuToggleProvider } from './components/contexts/menu-toggle'
import { QueryClient, QueryClientProvider } from 'react-query'
import LoadingAccount from './components/common/loading-account'
import { AntdRegistry } from '@ant-design/nextjs-registry'

const { Content } = Layout

const inter = Inter({ subsets: ['latin'] })

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
      refetchOnWindowFocus: false,
    },
  },
})

const RootLayout = ({ children }: React.PropsWithChildren) => (
  <html lang="en">
    <body className={inter.className}>
      <AntdRegistry>
        <Provider store={store}>
          <AuthProvider>
            <ThemeProvider>
              <AuthenticatedTemplate>
                <QueryClientProvider client={queryClient}>
                  <MenuToggleProvider>
                    {/* Main Layout */}
                    <Layout>
                      {/* Fixed Header */}
                      <AppHeader />
                      <LoadingAccount>
                        {/* Sidebar and Content */}
                        <Layout hasSider className="ant-layout-has-sider">
                          <AppMenu />
                          <Content className="ant-layout-content">
                            <AppBreadcrumb />
                            {children}
                          </Content>
                        </Layout>
                      </LoadingAccount>
                    </Layout>
                  </MenuToggleProvider>
                </QueryClientProvider>
              </AuthenticatedTemplate>
            </ThemeProvider>
          </AuthProvider>
        </Provider>
      </AntdRegistry>
    </body>
  </html>
)

export default RootLayout
