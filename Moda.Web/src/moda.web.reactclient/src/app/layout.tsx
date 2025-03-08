'use client'

import '@/styles/globals.css'
import React, { useEffect, useState } from 'react'
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
import { QueryClient, QueryClientProvider } from 'react-query'
import { AntdRegistry } from '@ant-design/nextjs-registry'
import { AuthProvider } from '../components/contexts/auth'

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
            <AuthProvider>
              <ThemeProvider>
                <QueryClientProvider client={queryClient}>
                  <MenuToggleProvider>
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
                  </MenuToggleProvider>
                </QueryClientProvider>
              </ThemeProvider>
            </AuthProvider>
          </Provider>
        </AntdRegistry>
      </body>
    </html>
  )
}

export default RootLayout
