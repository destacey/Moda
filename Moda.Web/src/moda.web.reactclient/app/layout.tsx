'use client'

import '../styles/globals.css'
import React, { createContext, use, useEffect, useState } from 'react';
import { Layout, ConfigProvider } from 'antd'
import lightTheme from './config/theme/light-theme';
import darkTheme from './config/theme/dark-theme';
import { usePathname } from 'next/navigation';
import { AuthenticatedTemplate, MsalProvider } from '@azure/msal-react';
import { msalInstance } from './services/auth';
import AppHeader from './components/common/app-header';
import AppMenu from './components/common/app-menu';
import AppBreadcrumb from './components/common/app-breadcrumb';
import { useLocalStorageState } from './hooks/use-local-storage-state';

const { Content } = Layout


export const ThemeContext = createContext([]);

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


  const [currentThemeName, setCurrentThemeName] = useLocalStorageState('modaTheme', 'light')
  const [currentTheme, setCurrentTheme] = useState(currentThemeName === 'light' ? lightTheme : darkTheme)
  const pathname = usePathname()

  useEffect(() => {
    setCurrentTheme(currentThemeName === 'light' ? lightTheme : darkTheme)
  }, [currentThemeName])

  useEffect(() => {
    async function initialize() {
      await msalInstance.initialize();
      if (!msalInstance.getActiveAccount()) {
        await msalInstance.loginRedirect()
          .catch((e) => { console.error(`loginRedirect failed: ${e}`) })
      }
    }
    initialize()
  }, [])

  return (
    <html lang="en">
      <body>
        <ConfigProvider theme={currentTheme}>
          <ThemeContext.Provider value={[ currentThemeName, setCurrentThemeName ]}>
            <MsalProvider instance={msalInstance}>
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
            </MsalProvider>
          </ThemeContext.Provider>
        </ConfigProvider>
      </body>
    </html>
  );
};
