'use client'

import './globals.css'
import React, { useState } from 'react';
import { Inter } from 'next/font/google'
import type { MenuProps } from 'antd';
import { Breadcrumb, Layout, Menu, Button, ConfigProvider, theme } from 'antd'
import { LaptopOutlined, NotificationOutlined, UserOutlined, VideoCameraOutlined, UploadOutlined, MenuUnfoldOutlined, MenuFoldOutlined } from '@ant-design/icons'
import lightTheme from './config/theme/light-theme';
import darkTheme from './config/theme/dark-theme';
import ThemeProvider from './config/theme/theme-provider';
import Icon from '@ant-design/icons/lib/components/Icon';

const { Header, Sider, Content } = Layout;
const inter = Inter({ subsets: ['latin'] });

const items1: MenuProps['items'] = ['1', '2', '3'].map((key) => ({
  key,
  label: `nav ${key}`,
}));

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

export const metadata = {
  title: 'Moda',
  description: 'Moda is a work management system used to plan, manage, and create associations across work items, projects, teams, planning and products. It helps track, align, and deliver work across organizations.',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  theme.useToken();
  const [currentTheme, setCurrentTheme] = useState('light');
  return (
    <html lang="en">
      <body className={inter.className}>
        <ConfigProvider theme={{ token: currentTheme === 'light' ? lightTheme : darkTheme }}>
          <ThemeProvider>
            <Layout className="layout" style={{ height: '100vh' }}>
              <Header style={{ display: 'flex', alignItems: 'center', backgroundColor: currentTheme === 'dark' ? '#262a2c' : '#2196f3' }}>
                
                <h2>Moda</h2>
                <label><input type="checkbox" checked={currentTheme === 'dark'} onChange={() => setCurrentTheme(currentTheme === 'dark' ? 'light' : 'dark')} /> Dark Mode</label>
              </Header>
              <Layout>
                <Sider width={200}>
                  <Menu
                    mode="inline"
                    defaultSelectedKeys={['1']}
                    defaultOpenKeys={['sub1']}
                    style={{ height: '100%', borderRight: 0 }}
                    items={items2}
                  />
                </Sider>
                <Layout style={{ padding: '0 24px 24px' }}>
                  <Breadcrumb style={{ margin: '16px 0' }}>
                    <Breadcrumb.Item>Home</Breadcrumb.Item>
                    <Breadcrumb.Item>List</Breadcrumb.Item>
                    <Breadcrumb.Item>App</Breadcrumb.Item>
                  </Breadcrumb>
                  <Content
                    style={{
                      padding: 24,
                      margin: 0,
                      minHeight: 280,
                    }}
                  >
                    Content
                  </Content>
                </Layout>
              </Layout>
            </Layout>
          </ThemeProvider>
        </ConfigProvider>
      </body>
    </html>
  );
};
