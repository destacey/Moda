'use client'

import '../styles/globals.css'
import React, { useEffect, useState } from 'react';
import { Montserrat } from 'next/font/google'
import { Breadcrumb, Layout, Menu, ConfigProvider, theme, Space, Typography } from 'antd'
import lightTheme from './config/theme/light-theme';
import darkTheme from './config/theme/dark-theme';
import { Metadata } from 'next';
import { AuthenticatedTemplate, MsalProvider } from '@azure/msal-react';
import { msalInstance } from './services/auth';
import AppHeader from './components/common/app-header';
import NavMenu from './components/common/nav-menu';

const { Sider, Content } = Layout;

// TODO: not working.  This should be the primary font for the app.
const primaryFont = Montserrat({ subsets: ["latin"] });

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
  theme.useToken();
  const [currentTheme, setTheme] = useState('light');

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
    <html lang="en" className={primaryFont.className}>
      <body>
        <ConfigProvider theme={{ token: currentTheme === 'light' ? lightTheme : darkTheme }}>
          <MsalProvider instance={msalInstance}>
            <AuthenticatedTemplate>
              <Layout className="layout" style={{ minHeight: '100vh' }}>
                <AppHeader currentTheme={currentTheme} setTheme={setTheme} />
                <Layout>
                  <NavMenu />
                  <Layout style={{ padding: '0 24px 24px' }}>
                    <Breadcrumb separator='>' style={{ margin: '16px 0' }} items={[{ title: 'Home', href: '/' }]} />
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
