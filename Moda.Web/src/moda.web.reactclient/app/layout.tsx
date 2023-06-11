'use client'

import '../styles/globals.css'
import React, { useEffect, useState } from 'react';
import { Layout, ConfigProvider } from 'antd'
import lightTheme from './config/theme/light-theme';
import darkTheme from './config/theme/dark-theme';
import { Metadata } from 'next';
import { usePathname } from 'next/navigation';
import { AuthenticatedTemplate, MsalProvider } from '@azure/msal-react';
import { msalInstance } from './services/auth';
import AppHeader from './components/common/app-header';
import NavMenu from './components/common/nav-menu';
import AppBreadcrumb from './components/common/app-breadcrumb';

const { Content } = Layout;

export const metadata: Metadata = {
  title: {
    template: 'Moda | {{title}}',
    default: 'Moda'
  },
  description: 'Moda is a work management system used to plan, manage, and create associations across work items, projects, teams, planning and products. It helps track, align, and deliver work across organizations.',
}
export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {

  const [currentTheme, setCurrentTheme] = useState('light');
  const pathname = usePathname();

  useEffect(() => {
    async function initialize() {
      await msalInstance.initialize();
      if (!msalInstance.getActiveAccount()) {
        await msalInstance.loginRedirect()
          .catch((e) => { console.error(`loginRedirect failed: ${e}`) });;
      }
    }
    initialize();
  }, []);

  return (
    <html lang="en">
      <body>
        <ConfigProvider theme={ currentTheme === 'light' ? lightTheme : darkTheme }>
          <MsalProvider instance={msalInstance}>
            <AuthenticatedTemplate>
              <Layout className="layout" style={{ minHeight: '100vh' }}>
                <AppHeader currentTheme={currentTheme} setTheme={setCurrentTheme} />
                <Layout>
                  <NavMenu />
                  <Layout style={{ padding: '0 24px 24px' }}>
                    <AppBreadcrumb pathname={pathname} />
                    <Content
                      style={{
                        padding: 24,
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
          </MsalProvider>
        </ConfigProvider>
      </body>
    </html>
  );
};
