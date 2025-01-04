'use client'

import '@/styles/globals.css'
import 'ag-grid-community/styles/ag-grid.css'
import 'ag-grid-community/styles/ag-theme-balham.css'
import React, { useEffect, useState } from 'react'
import { Provider } from 'react-redux'
import { Inter } from 'next/font/google'
import { Layout } from 'antd'
import { AuthenticatedTemplate } from '@azure/msal-react'
import { store } from '../store'
import AppHeader from './_components/app-header'
import AppSideNav from './_components/menu/app-side-nav'
import AppBreadcrumb from './_components/app-breadcrumb'
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

const RootLayout = ({ children }: React.PropsWithChildren) => {
  const [isMobile, setIsMobile] = useState(false)

  useEffect(() => {
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
                          <Layout hasSider className="app-main-layout">
                            <AppSideNav isMobile={isMobile} />
                            <Content className="app-main-content">
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
}

export default RootLayout
