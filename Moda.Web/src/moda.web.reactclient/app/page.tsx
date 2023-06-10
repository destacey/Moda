'use client'

import { Content } from "antd/es/layout/layout";

export default function Home() {
  return (
    <Content
      className="site-layout-background"
      style={{
        margin: '24px 16px',
        padding: 24,
      }}
    >
      <h1>Home</h1>
    </Content>
  )
}