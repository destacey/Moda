'use client'

import { Layout } from 'antd'
import DocsSidebar from './docs-sidebar'
import type { DocNavItem } from '@/src/services/docs'

const { Sider, Content } = Layout

interface DocsLayoutShellProps {
  navigation: DocNavItem[]
  children: React.ReactNode
}

export default function DocsLayoutShell({
  navigation,
  children,
}: DocsLayoutShellProps) {
  return (
    <Layout className="docs-layout">
      <Sider
        width={260}
        theme="light"
      >
        <DocsSidebar navigation={navigation} />
      </Sider>
      <Content
        style={{
          padding: '24px',
          maxWidth: '900px',
        }}
      >
        {children}
      </Content>
    </Layout>
  )
}
