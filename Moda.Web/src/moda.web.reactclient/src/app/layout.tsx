'use client'

import '../../styles/globals.css'
import React, { use, useEffect } from 'react'
import { Layout } from 'antd'
import { AuthenticatedTemplate } from '@azure/msal-react'
import AppHeader from './components/common/app-header'
import AppMenu from './components/common/menu'
import AppBreadcrumb from './components/common/app-breadcrumb'
import { ThemeProvider } from './components/contexts/theme'
import { AuthProvider } from './components/contexts/auth'
import { BreadcrumbsProvider } from './components/contexts/breadcrumbs/breadcrumbs-context'

const { Content } = Layout
//export const metadata: Metadata = {
//  title: {
//    template: 'Moda | {{title}}',
//    default: 'Moda'
//  },
//  description: 'Moda is a work management system used to plan, manage, and create associations across work items, projects, teams, planning and products. It helps track, align, and deliver work across organizations.',
//}

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
            </AuthenticatedTemplate>
          </ThemeProvider>
        </AuthProvider>
      </body>
    </html>
  )
}
