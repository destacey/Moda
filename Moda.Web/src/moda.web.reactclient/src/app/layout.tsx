'use client'

import '../../styles/globals.css'
import React from 'react';
import { Layout } from 'antd'
import { usePathname } from 'next/navigation';
import { AuthenticatedTemplate, MsalProvider } from '@azure/msal-react';
import AppHeader from './components/common/app-header';
import AppMenu from './components/common/app-menu';
import AppBreadcrumb from './components/common/app-breadcrumb';
import { ThemeProvider } from './components/contexts/theme';
import { AuthProvider } from './components/contexts/auth';

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
  const pathname = usePathname()

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
                    <AppBreadcrumb pathname={pathname} />
                    <Content
                      style={{
                        margin: 0,
                        minHeight: 280,
                      }}
                    >
                      {children}
                    </Content>
                  </Layout>
                </Layout>
              </Layout>
            </AuthenticatedTemplate>
          </ThemeProvider>
        </AuthProvider>
      </body>
    </html>
  );
};
