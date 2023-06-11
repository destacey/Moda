'use client'

import './globals.css'
import React, { useEffect, useState } from 'react';
import { Inter } from 'next/font/google'
import type { MenuProps } from 'antd';
import { Breadcrumb, Layout, Menu, ConfigProvider, theme, Space, Typography } from 'antd'
import { LaptopOutlined, NotificationOutlined, UserOutlined } from '@ant-design/icons'
import lightTheme from './config/theme/light-theme';
import darkTheme from './config/theme/dark-theme';
import { Metadata } from 'next';
import { AuthenticatedTemplate, MsalProvider } from '@azure/msal-react';
import { msalInstance } from './services/auth';
import AppHeader from './components/common/app-header';

const { Header, Sider, Content } = Layout;
const { Title } = Typography;
const inter = Inter({ subsets: ['latin'] });

const items2: MenuProps['items'] = [UserOutlined, LaptopOutlined, NotificationOutlined].map(
  (icon, index) => {
    const key = String(index + 1);

    return {
      key: `sub${key}`,
      icon: React.createElement(icon),
      label: `subnav ${key}`,

      children: new Array(4).fill(null).map((_, j) => {
        const subKey = index * 4 + j + 1;
        return {
          key: subKey,
          label: `option${subKey}`,
        };
      }),
    };
  },
);

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
  const [collapsed, setCollapsed] = useState(false);

  useEffect(() => {
    async function initialize() {
      await msalInstance.initialize();
      if (!msalInstance.getActiveAccount()) {
        await msalInstance.loginPopup()
          .catch((e) => { console.error(`loginRedirect failed: ${e}`) });;
      }
    }
    initialize();
  }, []);

  return (
    <html lang="en">
      <body className={inter.className}>
        <ConfigProvider theme={{ token: currentTheme === 'light' ? lightTheme : darkTheme }}>
          <MsalProvider instance={msalInstance}>
            <AuthenticatedTemplate>
              <Layout className="layout" style={{ minHeight: '100vh' }}>
                <AppHeader currentTheme={currentTheme} setTheme={setTheme} />
                <Layout>
                  <Sider width={200} collapsedWidth={50} collapsible collapsed={collapsed} onCollapse={(value) => setCollapsed(value)}>
                    <Menu
                      mode="inline"
                      defaultSelectedKeys={['1']}
                      defaultOpenKeys={['sub1']}
                      style={{ height: '100%', borderRight: 0 }}
                      items={items2}
                    />
                  </Sider>
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
