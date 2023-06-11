'use client'

import './globals.css'
import React, { useEffect, useState } from 'react';
import { Inter } from 'next/font/google'
import type { MenuProps } from 'antd';
import { Breadcrumb, Layout, Menu, ConfigProvider, theme, Switch, Space, Typography } from 'antd'
import { LaptopOutlined, NotificationOutlined, UserOutlined, VideoCameraOutlined, UploadOutlined, MenuUnfoldOutlined, MenuFoldOutlined } from '@ant-design/icons'
import lightTheme from './config/theme/light-theme';
import darkTheme from './config/theme/dark-theme';
import { Metadata } from 'next';
import { MsalProvider } from '@azure/msal-react';
import Profile from './components/Profile';
import { msalInstance } from './services/auth';

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
    template: 'Moda | {{title}}' ,
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
  const [currentTheme, setCurrentTheme] = useState('light');
  const [collapsed, setCollapsed] = useState(false);

  useEffect(() => {
    async function initialize() {
      await msalInstance.initialize();
      if(!msalInstance.getActiveAccount()){
        await msalInstance.loginPopup();
      }
    }
    initialize();
  }, []);

  return (
    <html lang="en">
      <body className={inter.className}>
        <ConfigProvider theme={{ token: currentTheme === 'light' ? lightTheme : darkTheme }}>
          <MsalProvider instance={msalInstance}>
            <Layout className="layout" style={{ minHeight: '100vh' }}>
              <Header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', backgroundColor: currentTheme === 'dark' ? '#262a2c' : '#2196f3' }}>
                <Title>Moda</Title>
                <Space>             
                  <Switch checked={currentTheme === 'dark'}
                    onChange={() => setCurrentTheme(currentTheme === 'dark' ? 'light' : 'dark')}
                    checkedChildren="Dark"
                    unCheckedChildren="Light" />
                  <Profile currentTheme={currentTheme} setTheme={setCurrentTheme} />
                </Space>
              </Header>
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
          </MsalProvider>
        </ConfigProvider>
      </body>
    </html>
  );
};
